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
        public delegate void Callback(String name, String password);
        private Callback cb;

        public Form2()
        {
            InitializeComponent();

            GoButton.Enabled = false;
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            String input1 = NameBox.Text.ToString();
            String input2 = PasswordBox.Text.ToString();
            cb(input1, input2);

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            checkBoxContent();
        }

        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) 
                && GoButton.Enabled)
                GoButton_Click(sender, e);
        }

        private void PasswordBox_TextChanged(object sender, EventArgs e)
        {
            checkBoxContent();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                && GoButton.Enabled)
                GoButton_Click(sender, e);
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
            MessageLabel.Text = message;
        }

        public void setLabels(String first, String second)
        {
            NameLabel.Text = first;
            PassLabel.Text = second;
        }

        public void setButtonText(String first, String second)
        {
            GoButton.Text = first;
            CancelButton.Text = second;
        }

        public void setDefaultInput(String first, String second)
        {
            NameBox.Text = first;
            PasswordBox.Text = second;
        }

        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}