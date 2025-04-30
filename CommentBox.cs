using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bachelor_Testing_V1
{
    public partial class CommentBox : Form
    {
        string? comment;
        public CommentBox(string requirement)
        {
            InitializeComponent();
            lblRequirement.Text = requirement;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            comment = rtbComment.Text;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        public string GetComment()
        {
            return rtbComment.Text;
        }
    }
}
