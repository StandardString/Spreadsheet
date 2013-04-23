/*
 * Filename:  Form2.cs
 * Authors:   Owen Krafft, Dickson Chiu, Austin Nester, Bryan K. Smith
 * Course:    CS 3505
 * Date:      4/15/2013
 * 
 * Description:
 * 
 *      A customizable connection form for the spreadsheet client.
 */ 
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

        /// <summary>
        /// Constructor for the connection form.
        /// </summary>
        public Form2()
        {
            InitializeComponent();

            GoButton.Enabled = false; // Disables the primary button by default.
        }

        /// <summary>
        /// Handles the event where the primary button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoButton_Click(object sender, EventArgs e)
        {
            // Stores the content of the text boxes in strings.
            String input1 = NameBox.Text.ToString();
            String input2 = PasswordBox.Text.ToString();
            cb(input1, input2); // Sends the strings through the callback.

            this.Close(); // Closes the form.
        }

        /// <summary>
        /// Handles the event where the cancel button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Closes the form.
        }

        /// <summary>
        /// Handles the event where the text inside the name box changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            // Checks to see if both text boxes contain something.
            checkBoxContent();
        }

        /// <summary>
        /// Handles the event where a key is pressed while the name box has focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            // If the enter or return key is pressed...
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) 
                && GoButton.Enabled)
                GoButton_Click(sender, e); // Run as button click.
        }

        /// <summary>
        /// Handles the event where the text inside the password box changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordBox_TextChanged(object sender, EventArgs e)
        {
            // Checks to see if both text boxes contain something.
            checkBoxContent();
        }

        /// <summary>
        /// Handles the event where a key is pressed while the password box has focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            // If the enter or return key is pressed.
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                && GoButton.Enabled)
                GoButton_Click(sender, e); // Run as button click.
        }

        /// <summary>
        /// A helper method that checks to see if both text fields have content.
        /// </summary>
        private void checkBoxContent()
        {
            // Trims any whitespaces from the box contents.
            String name = NameBox.Text.Trim();
            String password = PasswordBox.Text.Trim();
            // If the boxes both contain strings...
            if (name != "" && password != "")
                GoButton.Enabled = true; // Enable the primary button.
            else
                GoButton.Enabled = false; // Disable the primary button.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void setMessage(String message)
        {
            MessageLabel.Text = message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void setLabels(String first, String second)
        {
            NameLabel.Text = first;
            PassLabel.Text = second;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void setButtonText(String first, String second)
        {
            GoButton.Text = first;
            CancelButton.Text = second;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void setDefaultInput(String first, String second)
        {
            NameBox.Text = first;
            PasswordBox.Text = second;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}