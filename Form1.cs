using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Text.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Bachelor_Testing_V1
{
    public partial class Form1 : Form
    {
        Session session;
        bool flipBool = false;
        string Slot1NodeId = "ns=4;s=|var|WAGO 751-9401 Compact Controller 100.Application.PLC_PRG.Slot1";
        PostgresDbHandler postgresDbHandler;
        private TestProcedure procedure;
        int selectedStepIdx;

        public Form1()
        {
            InitializeComponent();
            SetTabStopRecursive(this);
            clbRequirements.TabStop = true;
            clbRequirements.Focus();
            ConnectDb();
            try
            {
                ConnectOpcUa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("opc ua failed to connect: ", ex.Message);
            }
            LoadTestProcedure();
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

        private void LoadTestProcedure()
        {
            selectedStepIdx = 0;
            string json = postgresDbHandler.GetTestProcedureJsonById(3); //TODO get the test procedure based on the switchboard in question
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            procedure = JsonSerializer.Deserialize<TestProcedure>(json, options);

            UpdateProcedureInfo();
        }

        private void UpdateProcedureInfo()
        {
            lsbSafetyReq.DataSource = null;
            lsbSafetyReq.DataSource = procedure.SafetyRequirements;
            var selectedStep = procedure.Steps[selectedStepIdx];
            lblStepName.Text = $"Step {selectedStepIdx + 1}: {selectedStep.Name}";
            rtbDescription.Text = selectedStep.Description;
            rtbEquipment.Text = "- " + string.Join("\n- ", selectedStep.EquipmentNeeded);
            int i = 0;
            foreach (var item in clbRequirements.Items)
            {
                bool isChecked = clbRequirements.GetItemChecked(i);
                if (isChecked)
                {
                    selectedStep.Requirements[i].Completed = true;
                }
                else
                {
                    selectedStep.Requirements[i].Completed = false;
                }
                i ++;
            }
            clbRequirements.Items.Clear();
            foreach (Requirement requirement in selectedStep.Requirements)
            {
                int index = clbRequirements.Items.Add(requirement.Value);
                if (requirement.Completed)
                {
                    clbRequirements.SetItemChecked(index, true);
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
                session = Session.Create(
                            config,
                            endpoint,
                            updateBeforeConnect,
                            checkDomain,
                            sessionName,
                            sessionTimeout,
                            user,
                            preferredLocales
                        ).Result;

                if (session != null && session.Connected)
                {
                    MessageBox.Show("OPC UA Client connected");
                    SubscribeToVariable(session, Slot1NodeId);
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
            postgresDbHandler = new("127.0.0.1", 5432, "SwitchboardTesting", "postgres", "");
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
            if (session != null)
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

                session.Write(null,
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

        private void btnNextStep_Click(object sender, EventArgs e)
        {
            if (selectedStepIdx < procedure.Steps.Count() - 1)
            {
                selectedStepIdx += 1;
            }
            else
            {
                MessageBox.Show("Are you sure you are done");
            }
            UpdateProcedureInfo();
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
            if (session != null)
            {
                session.Close();
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
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
    }
}
