using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Text.Json;

namespace Bachelor_Testing_V1
{
    public partial class Form1 : Form
    {
        Session? OpcSession;
        bool flipBool = false;
        string Slot1NodeId = "ns=4;s=|var|WAGO 751-9401 Compact Controller 100.Application.PLC_PRG.Slot1";
        PostgresDbHandler? postgresDbHandler;
        private TestProcedure? procedure;
        int selectedStepIdx;

        public Form1()
        {
            InitializeComponent();
            SetTabStopRecursive(this);
            clbRequirements.TabStop = true;
            clbRequirements.Focus();
            ConnectDb();
            //try
            //{
            //    ConnectOpcUa();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("opc ua failed to connect: ", ex.Message);
            //}
            if (postgresDbHandler != null)
            {
                postgresDbHandler.PopulateSwitchboardComboBox(cbbSwitchboards);
            }
            cbbSwitchboards.SelectedIndex = 0;

            // Attach KeyDown event handler to the form
            this.KeyDown += Form1_KeyDown;
        }

        private void SetTabStopRecursive(Control parent)
        {
            // Set TabStop = false for all controls except myCheckListBox
            foreach (Control ctrl in parent.Controls)
            {
                ctrl.TabStop = false;

                // Recursively apply to children
                if (ctrl.HasChildren)
                {
                    SetTabStopRecursive(ctrl);
                }
            }
        }

        private void LoadTestProcedure(int switchboardId)
        {
            selectedStepIdx = 0;
            string? json = null;
            if (postgresDbHandler != null)
            {
                json = postgresDbHandler.GetTestProcedureJsonById(switchboardId);
            }
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            if (json != null)
            {
                procedure = JsonSerializer.Deserialize<TestProcedure>(json, options);
            }
            else
            {
                MessageBox.Show("test procedure was not found");
                throw new("test procedure was not found");
            }
            UpdateProcedureInfo();
        }

        private void UpdateProcedureInfo()
        {
            lsbSafetyReq.DataSource = null;
            if (procedure != null)
            {
                lsbSafetyReq.DataSource = procedure.SafetyRequirements;
                var selectedStep = procedure.Steps[selectedStepIdx];
                lblStepName.Text = $"Step {selectedStepIdx + 1}: {selectedStep.Name}";
                rtbDescription.Text = selectedStep.Description;
                rtbEquipment.Clear();
                if (selectedStep.EquipmentNeeded != null)
                {
                    rtbEquipment.Text = "- " + string.Join("\n- ", selectedStep.EquipmentNeeded);
                }
                UpdateRequirementsCompletions();
                clbRequirements.Items.Clear();
                foreach (Requirement requirement in selectedStep.Requirements)
                {
                    int index = clbRequirements.Items.Add(requirement);
                    if (requirement.Completed)
                    {
                        clbRequirements.SetItemChecked(index, true);
                    }
                }
            }
        }

        private void UpdateRequirementsCompletions()
        {
            int i = 0;
            foreach (var item in clbRequirements.Items)
            {
                if (item is Requirement)
                {
                    Requirement requirement = (Requirement)item;
                    bool isChecked = clbRequirements.GetItemChecked(i);
                    if (isChecked)
                    {
                        requirement.Completed = true;
                    }
                    else
                    {
                        requirement.Completed = false;
                    }
                    i++;
                }
            }
        }

        private void ConnectOpcUa()
        {
            var config = new Opc.Ua.ApplicationConfiguration()
            {
                ApplicationName = "TestStationClient",
                ApplicationUri = Utils.Format(@"urn:{0}:MyClient", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault", SubjectName = "MyClientSubjectName" },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                    AutoAcceptUntrustedCertificates = false
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };
            config.Validate(ApplicationType.Client).GetAwaiter().GetResult();

            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (s, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
            }

            var application = new ApplicationInstance
            {
                ApplicationName = "TestStationClient",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };
            try
            {
                EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(config, "opc.tcp://192.168.1.17:4840/", true, 4000); //the address of the wago opc ua server, and 100 ms in timeout for fast testing
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(config);
                ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                UserIdentity user = new UserIdentity();
                bool useCredentials = true;
                if (useCredentials)
                {
                    user = new UserIdentity("admin", "wago");
                }
                bool updateBeforeConnect = false;
                bool checkDomain = false;
                string sessionName = config.ApplicationName;
                uint sessionTimeout = 60000;
                List<string>? preferredLocales = null;

                // Create the session
                OpcSession = Session.Create(
                            config,
                            endpoint,
                            updateBeforeConnect,
                            checkDomain,
                            sessionName,
                            sessionTimeout,
                            user,
                            preferredLocales
                        ).Result;

                if (OpcSession != null && OpcSession.Connected)
                {
                    MessageBox.Show("OPC UA Client connected");
                    SubscribeToVariable(OpcSession, Slot1NodeId);
                }
                else throw new Exception("session could not connect");
            }
            catch
            {
                throw;
            }
        }

        private void ConnectDb()
        {
            // ipadress is localhost because postgres server is local, username is postgres and password is empty string
            postgresDbHandler = new("localhost", 5432, "SwitchboardTesting", "postgres", "");
            if (postgresDbHandler.TestConnection())
            {
                MessageBox.Show("DbCon working");
            }
            else
            {
                MessageBox.Show("DbCon failed");
            }
        }

        private void btnWriteToPlc_Click(object sender, EventArgs e)
        {
            if (OpcSession != null)
            {
                // Define the node IDs for the variables
                NodeId temperatureNodeId = new NodeId("ns=4;s=|var|WAGO 751-9401 Compact Controller 100.Application.PLC_PRG.Temperature");
                NodeId pressureNodeId = new NodeId("ns=4;s=|var|WAGO 751-9401 Compact Controller 100.Application.PLC_PRG.Pressure");

                WriteValueCollection nodesToWrite = new WriteValueCollection();
                WriteValue boolWriteVal = new WriteValue();
                boolWriteVal.NodeId = new NodeId("ns=4;s=|var|WAGO 751-9401 Compact Controller 100.Application.PLC_PRG.test");
                boolWriteVal.AttributeId = Attributes.Value;
                boolWriteVal.Value = new DataValue();
                boolWriteVal.Value.Value = flipBool;
                flipBool = !flipBool;
                nodesToWrite.Add(boolWriteVal);

                StatusCodeCollection? results = null;
                DiagnosticInfoCollection diagnosticInfos;
                MessageBox.Show("Writing nodes...");

                OpcSession.Write(null,
                                   nodesToWrite,
                                   out results,
                                   out diagnosticInfos);
                string writeResultString = "";
                // Display the results.
                foreach (StatusCode writeResult in results)
                {
                    writeResultString += writeResult.ToString();
                }
                MessageBox.Show(writeResultString);
                // Read the Temperature variable
                //DataValue temperatureValue = session.ReadValue(temperatureNodeId);
                //MessageBox.Show($"Temperature: {temperatureValue.Value}");

                //// Read the Pressure variable
                //DataValue pressureValue = session.ReadValue(pressureNodeId);
                //MessageBox.Show($"Pressure: {pressureValue.Value}");
            }
        }
        private static void SubscribeToVariable(Session session, string nodeId)
        {
            Subscription subscription = new Subscription(session.DefaultSubscription)
            {
                PublishingInterval = 100,  // Sends updates every 100ms
                KeepAliveCount = 10,
                LifetimeCount = 30,
                MaxNotificationsPerPublish = 10,
                PublishingEnabled = true,
                Priority = 0
            };

            MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                SamplingInterval = 100,  // Samples the variable every 100ms
                QueueSize = 10,
                DiscardOldest = true
            };
            monitoredItem.Notification += MonitoredItem_Notification;
            subscription.AddItem(monitoredItem);
            session.AddSubscription(subscription);
            subscription.Create();

            MessageBox.Show($"Subscribed to {nodeId} with 100ms interval.");
        }
        private static void MonitoredItem_Notification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            MessageBox.Show(item.ToString());
        }

        private void TryCompleteProcedure()
        {
            bool cancelCompletionOfTest = false;
            if (procedure != null)
            {
                foreach (var step in procedure.Steps)
                {
                    DialogResult result = DialogResult.OK;
                    // loop over requirements to see if they are completed
                    foreach (var requirement in step.Requirements)
                    {
                        bool isCompleted = requirement.Completed;
                        if (!isCompleted)
                        {
                            result = MessageBox.Show(
                                $"Step: {step.Name} \nRequirement: {requirement} \nis not completed. Is this correct?\n \nYes = requirement is not completed, write a comment\nNo = requirement is completed\ncancel = go trough and check manually",
                                "Confirm",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Exclamation
                            );

                            if (result == DialogResult.No)
                            {
                                requirement.Completed = true;
                            }
                            else if (result == DialogResult.Yes)
                            {
                                //check if a comment was made or not
                                if (requirement.Comment == null)
                                {
                                    //get a comment from the user on why the requirement was not completed
                                    CommentBox commentBox = new($"requirement: {requirement.ToString()}, was not completed");
                                    DialogResult commentResult = commentBox.ShowDialog();
                                    if (commentResult == DialogResult.OK)
                                    {
                                        string comment = commentBox.GetComment();
                                        requirement.Comment = comment;
                                    }
                                    commentBox.Dispose();
                                }
                            }
                        }
                        if (result == DialogResult.Cancel)
                        {
                            cancelCompletionOfTest = true;
                            break;
                        }
                    }
                    if (result == DialogResult.Cancel)
                    {
                        cancelCompletionOfTest = true;
                        break;

                    }
                }
                if (!cancelCompletionOfTest)
                {
                    //save test to db
                    int testerId = 1; //hardcoded testerId because missing tester id logic (login or other)
                    int switchboardId = Convert.ToInt32(cbbSwitchboards.SelectedValue);
                    int? testProcedureId = null;
                    if (postgresDbHandler != null)
                    {
                        testProcedureId = postgresDbHandler.GetLatestTestIdForSwitchboard(switchboardId);
                    }
                    if (testProcedureId != null && postgresDbHandler != null)
                    {
                        //use the returning execution id to save the results to the same execution row
                        int? executionId = null;
                        foreach (var step in procedure.Steps)
                        {
                            foreach (var requirement in step.Requirements)
                            {
                                executionId = postgresDbHandler.SaveTestResult(
                                    switchboardId,
                                    (int)testProcedureId,
                                    testerId, step.StepId,
                                    requirement.Value,
                                    requirement.Completed,
                                    requirement.Comment,
                                    executionId
                                    );
                            }
                        }
                        MessageBox.Show("test results were saved");
                    }
                }
            }
        }
        private void btnNextStep_Click(object sender, EventArgs e)
        {
            if (procedure != null)
            {
                if (selectedStepIdx < procedure.Steps.Count() - 1)
                {
                    selectedStepIdx += 1;
                }
                else
                {
                    UpdateRequirementsCompletions();
                    DialogResult result = MessageBox.Show("do you want to save the test?", "", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        TryCompleteProcedure();
                    }
                }
                UpdateProcedureInfo();
            }
        }

        private void btnPrevStep_Click(object sender, EventArgs e)
        {
            if (selectedStepIdx > 0)
            {
                selectedStepIdx -= 1;
            }
            UpdateProcedureInfo();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (OpcSession != null)
            {
                OpcSession.Close();
            }
        }
        //left/right arrow goes to prev/next to navigate faster
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            // Check if the left arrow key is pressed
            if (e.KeyCode == Keys.Left)
            {
                // Simulate clicking the left button
                btnPrevStep.PerformClick();
            }
            // Check if the right arrow key is pressed
            else if (e.KeyCode == Keys.Right)
            {
                // Simulate clicking the right button
                btnNextStep.PerformClick();
            }
        }

        private void cbbSwitchboards_SelectedIndexChanged(object sender, EventArgs e)
        {
            //only prompt to switch if procedure is not null
            if (procedure != null)
            {
                DialogResult result = DialogResult.OK;
                result = MessageBox.Show(
                                $"Do you want to switch switchboard? all test results will be lost",
                                "Confirm",
                                MessageBoxButtons.YesNo
                            );
                if (result == DialogResult.Yes)
                {
                    TryLoadTestProcedure();
                }
            }
            else
            {
                TryLoadTestProcedure();
            }
        }

        private void TryLoadTestProcedure()
        {
            // Make sure something is selected
            if (cbbSwitchboards.SelectedValue != null)
            {
                // Get the selected switchboard ID
                int? testId = null;
                int switchboardId = Convert.ToInt32(cbbSwitchboards.SelectedValue);
                if (postgresDbHandler != null)
                {
                    testId = postgresDbHandler.GetLatestTestIdForSwitchboard(switchboardId);
                }
                if (testId != null)
                {
                    LoadTestProcedure((int)testId);
                }
                else
                {
                    MessageBox.Show("the selected switchboard does not have a test procedure.");
                }
            }
        }
    }
}
