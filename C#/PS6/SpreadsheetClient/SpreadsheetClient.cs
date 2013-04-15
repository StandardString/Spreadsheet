using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CustomNetworking;

namespace SS
{
    public class SpreadsheetClient
    {
        public StringSocket socket;
        public event Action<String> IncomingLineEvent;

        public SpreadsheetClient()
        {
            socket = null;
        }

        public void Connect(string hostname, int port, String name)
        {
            try
            {
                if (socket == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginSend(name + "\n", (e, p) => { }, null);
                    socket.BeginReceive(LineReceived, null);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SendMessage(String line)
        {
            if (socket != null)
                socket.BeginSend(line + "\n", (e, p) => { }, null);
        }

        private void LineReceived(String s, Exception e, object p)
        {
            if (IncomingLineEvent != null)
            {
                if (e != null)
                    IncomingLineEvent(e.ToString());
                else if (s != null)
                    IncomingLineEvent(s);
            }

            if (socket != null)
                socket.BeginReceive(LineReceived, null);
        }

        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }
}
