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
        /// 
        /// </summary>
        public DebugForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
                this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
                this.Close();

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                run(CommandBox.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cb(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        private void run(string command)
        {
            if (command == "clear")
            {
                CommandBox.Clear();
                Dialog.Clear();
                addCommand("clear");
            }
            else if (command == "exit")
                this.Close();
            else
            {
                CommandBox.Clear();
                addCommand("Command not recognized.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void addClientToServer(String message)
        {
            String s = "C => S  << " + message + " >>";
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n";
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret();
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void addServerToClient(String message)
        {
            String s = "S => C  << " + message + " >>";
            Dialog.Invoke(new Action(() => { 
                Dialog.Text += s + "\r\n";
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret();
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void addCommand(String message)
        {
            String s = "Cmd      << " + message + " >>";
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n";
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret();
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void addMessage(String message)
        {
            String s = "Status   << " + message + " >>";
            Dialog.Invoke(new Action(() =>
            {
                Dialog.Text += s + "\r\n";
                Dialog.SelectionStart = Dialog.Text.Length;
                Dialog.ScrollToCaret();
            }));
        }

        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}