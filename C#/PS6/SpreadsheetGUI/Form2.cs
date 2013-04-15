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
    }
}
