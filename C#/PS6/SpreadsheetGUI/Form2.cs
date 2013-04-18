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
        public delegate void Callback(String name, String password);
        private Callback cb;

        public Form2(Form1 primaryForm)
        {
            InitializeComponent();
            context = primaryForm;

            NameBox.Text = "name";
            PasswordBox.Text = "password";
            GoButton.Enabled = true;
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            String name = NameBox.Text.ToString();
            String password = PasswordBox.Text.ToString();
            cb(name, password);

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UsernameBox_TextChanged(object sender, EventArgs e)
        {
            checkBoxContent();
        }

        private void PasswordBox_TextChanged(object sender, EventArgs e)
        {
            checkBoxContent();
        }

        private void checkBoxContent()
        {
            String name = NameBox.Text.Trim();
            String password = PasswordBox.Text.Trim();
            if (name != "" && password != "")
                GoButton.Enabled = true;
            else
                GoButton.Enabled = false;
        }

        public void setMessage(String message)
        {
            String s = "Please enter the name and password of the spreadsheet you wish to ";
            MessageLabel.Text = s + message + ".";
        }

        public void setButtonText(String text)
        {
            GoButton.Text = text;
        }

        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}
