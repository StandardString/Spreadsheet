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

        public DebugForm()
        {
            InitializeComponent();
        }

        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            //
        }

        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
                this.Close();

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                run(CommandBox.Text);
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            cb(false);
        }

        private void run(string command)
        {
            if (command == "clear")
            {
                CommandBox.Clear();
                Dialog.Clear();
            }
            if (command == "close")
                this.Close();
        }

        public void addClientToServer(String message)
        {
            String s = "C => S  <<" + message + ">>";
            Dialog.Invoke(new Action(() => { Dialog.Text += s + "\r\n"; }));
        }

        public void addServerToClient(String message)
        {
            String s = "S => C <<" + message + ">>";
            Dialog.Invoke(new Action(() => { Dialog.Text += s + "\r\n"; }));
        }

        public void setCallback(Callback callback)
        {
            cb = callback;
        }
    }
}
