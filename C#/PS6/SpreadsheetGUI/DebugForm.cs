/* Authors:  Owen Krafft, Dickson Chiu, Austin Nester, Bryan K. Smith
 * Class:    CS 3505
 * Date:     4/19/2013
 *
 * Description:
 * 
 *      A simple debugging window for tracking messages between the spreadsheet
 * client and the host server.
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
    public partial class DebugForm : Form
    {
        public delegate void Callback(bool boolean);
        private Callback cb;

        /// <summary>
        /// The default constructor for the debug window.
        /// </summary>
        public DebugForm()
        {
            InitializeComponent();
            CommandBox.Focus();
        }

        /// <summary>
        /// A method that handles the event where a key is pressed down while the focus is on the dialog box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
                this.Close();
        }

        /// <summary>
        /// A method that handles the event where a key is pressed down while the focus is on the command box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            // If the Control and D keys are pressed together...
            if (e.Control && e.KeyCode == Keys.D)
                this.Close(); // The form is closed.

            // If enter or return is pressed...
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                run(CommandBox.Text); // Attempts to run the string in the box as a command.
        }

        /// <summary>
        /// Handles the event where the user closes the debug form window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cb(false); // Sets the debugging state of the spreadsheet.
        }

        /// <summary>
        /// A helper method that attempts to run the input string as a command in the window.
        /// </summary>
        /// <param name="command"></param>
        private void run(string command)
        {
            if (command == "clear")
            {
                CommandBox.Clear(); // Clears the command box text.
                Dialog.Clear(); // Clears the dialog box text.
                addCommand("clear"); // Prints a message to the dialog box.
            }
            else if (command == "exit")
                this.Close(); // Closes the debug form.
            else
            {
                CommandBox.Clear(); // Clears the command box.
                // Prints a message to the dialog box.
                addCommand("Command not recognized.");
            }
        }

        /// <summary>
        /// A helper method that formats a "client-to-server" message for the dialog window.
        /// </summary>
        /// <param name="message"></param>
        public void addClientToServer(String message)
        {
            // Generates the message string format.
            String s = "C => S  << " + message + " >>"; 
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n"; // Adds the message to the dialog box.
                // Moves the text entry point to the end of the line.
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret(); // Scrolls the dialog down to the last line of the box.
            }));
        }

        /// <summary>
        /// A helper method that formats a "server-to-client" message for the debug window.
        /// </summary>
        /// <param name="message"></param>
        public void addServerToClient(String message)
        {
            // Generates the message string format.
            String s = "S => C  << " + message + " >>";
            Dialog.Invoke(new Action(() => {
                Dialog.Text += s + "\r\n"; // Adds the message to the dialog box.
                // Moves the text entry point to the end of the line.
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret(); // Scrolls the dialog down to the last line of the box.
            }));
        }

        /// <summary>
        /// A helper method that formats a "status" message for the debug window.
        /// </summary>
        /// <param name="message"></param>
        public void addCommand(String message)
        {
            // Generates the message string format.
            String s = "Cmd      << " + message + " >>";
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n"; // Adds the message to the dialog box.
                // Moves the text entry point to the end of the line.
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret(); // Scrolls the dialog down to the last line of the box.
            }));
        }

        /// <summary>
        /// A helper method that formats a "status" message for the debug window.
        /// </summary>
        /// <param name="message"></param>
        public void addMessage(String message)
        {
            // Generates the message string format.
            String s = "Status   << " + message + " >>";
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n"; // Adds the message to the dialog box.
                // Moves the text entry point to the end of the line.
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret(); // Scrolls the dialog down to the last line of the box.
            }));
        }

        /// <summary>
        /// A helper method that sets the callback method for the debug window.
        /// </summary>
        /// <param name="callback"></param>
        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}