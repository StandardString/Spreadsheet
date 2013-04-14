using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using CustomNetworking;

namespace SS
{
    public class SpreadsheetClient
    {
        private StringSocket socket;
        public event Action<String> IncomingLineEvent;

        /// <summary>
        /// Creates a not yet connected client.
        /// </summary>
        public SpreadsheetClient()
        {
            socket = null;
        }

        /// <summary>
        /// Connects to the server at the given hostname and port, and with
        /// the given name.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        public void Connect(string hostname, int port, String name)
        {
            if (socket == null)
            {
                TcpClient client = new TcpClient(hostname, port);
                socket = new StringSocket(client.Client, UTF8Encoding.Default);
                socket.BeginSend(name + "\n", (e, p) => {}, null);
                socket.BeginReceive(LineReceived, null);
            }
        }

        /// <summary>
        /// Sends a line of text to the server.
        /// </summary>
        /// <param name="line"></param>
        public void SendMessage(String line)
        {
            if (socket != null)
                socket.BeginSend(line + "\n", (e, p) => { }, null);
        }

        /// <summary>
        /// Deals with an arriving line of text.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void LineReceived(String s, Exception e, object p)
        {
            if (IncomingLineEvent != null)
            {
                if (s != null)
                    IncomingLineEvent(s);
            }

            if (socket != null)
                socket.BeginReceive(LineReceived, null);
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
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
