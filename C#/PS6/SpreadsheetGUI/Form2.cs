using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SS
{
    public partial class Form2 : Form
    {
        Form1 context;

        public Form2(Form1 primaryForm)
        {
            InitializeComponent();

            this.context = primaryForm;
            ConnectButton.Enabled = false;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            String username = UsernameBox.Text.ToString();
            String password = PasswordBox.Text.ToString();
            context.connect(username, password);

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UsernameBox_TextChanged(object sender, EventArgs e)
        {
            String username = UsernameBox.Text.Trim();
            String password = PasswordBox.Text.Trim();
            if (username != "" && password != "")
                ConnectButton.Enabled = true;
            else
                ConnectButton.Enabled = false;
        }

        private void PasswordBox_TextChanged(object sender, EventArgs e)
        {
            String username = UsernameBox.Text.Trim();
            String password = PasswordBox.Text.Trim();
            if (username != "" && password != "")
                ConnectButton.Enabled = true;
            else
                ConnectButton.Enabled = false;
        }
    }
}
