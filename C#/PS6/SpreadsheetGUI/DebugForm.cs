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
        Form1 context;

        public DebugForm(Form1 form)
        {
            InitializeComponent();

            context = form;
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            context.setDebugging(false);
        }

        public void addClientToServer(String message)
        {
            String s = "C => S  <<" + message + ">>";
            Dialog.Invoke(new Action(() => { Dialog.Text += s + "\r\n"; }));
        }

        public void addServerToClient(String message)
        {
            String s = "S => S <<" + message + ">>";
            Dialog.Invoke(new Action(() => { Dialog.Text += s + "\r\n"; }));
        }

        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D)
                this.Close();
        }
    }
}
