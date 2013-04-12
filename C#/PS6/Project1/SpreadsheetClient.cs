/* Author:  Bryan K. Smith
 * Class:   CS 3505
 * Date:    4/11/2013
 * Version: 1.3.00
 *
 * Revision History:
 * 
 *      - 1.3.00 - Added sockets.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;

namespace SS
{
    class SpreadsheetClient
    {
        public StringSocket socket;
        public event Action<String> IncomingLineEvent;
        public String received;

        public SpreadsheetClient()
        {
            socket = null;
        }

        public void Connect(string hostname, int port, String name)
        {
        }

        public void Disconnect()
        {
        }

    }
}
