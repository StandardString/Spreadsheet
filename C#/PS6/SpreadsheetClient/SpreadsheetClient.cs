/*
 * Filename:  SpreadsheetClient.cs
 * Authors:   Owen Krafft, Dickson Chiu, Austin Nester, Bryan K. Smith
 * Course:    CS 3505
 * Date:      4/15/2013
 * 
 * Description:
 * 
 *      The client model that handles connection, disconnection, and messages.
 */

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
        public StringSocket socket; // The socket used to communicate with the server.
        // Register for this event to be notified when a line of text arrives.
        public event Action<String> IncomingLineEvent;

        /// <summary>
        /// Creates an unconnected client model.
        /// </summary>
        public SpreadsheetClient()
        {
            socket = null;
        }

        /// <summary>
        /// Connect to the server at the given hostname and port.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            try // Attempts to connect to the hostname on the provided port.
            {
                if (socket == null)
                {
                    TcpClient client = new TcpClient(hostname, port);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginReceive(LineReceived, null);
                }
            }
            catch (Exception e)
            {
                throw e; // If anything goes wrong, the exception is handled at a higher level.
            }
        }

        /// <summary>
        /// Sends a line of text to the server.
        /// </summary>
        /// <param name="line"></param>
        public void SendMessage(String line)
        {
            if (socket != null)
                socket.BeginSend(line, (e, p) => { }, null);
        }

        /// <summary>
        /// Deal with an incoming line of text.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="p"></param>
        private void LineReceived(String s, Exception e, object p)
        {
            if (IncomingLineEvent != null)
            {
                if (e != null) // If an exception was thrown...
                    IncomingLineEvent(e.ToString()); // Sends the exception forward.
                if (s != null) // Otherwise, if the string isn't empty...
                    IncomingLineEvent(s); // Sends the string.
            }

            if (socket != null) // If the socket is still active.
                socket.BeginReceive(LineReceived, null); // Begin receiving messages.
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        public void Disconnect()
        {
            // If the socket has not already been closed...
            if (socket != null)
            {
                socket.Close(); // Closes the socket.
                socket = null; // Removes the socket from the client.
            }
        }
    }
}