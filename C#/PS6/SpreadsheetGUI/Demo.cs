// Author:  Bryan K. Smith
// Class:   CS 3500
// Date:    10/17/2012
// Version: 1.3.025
//
// Revision History:
//           1.1.00 - 10/1/2012 - Created new methods to match the updated specification of AbstractSpreadsheet.
//           1.2.00 - 10/17/2012 - Modified acceptible variable formats and added GUI element.
//           1.3.00 - 4/15/2013 - Added sockets and connection support.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SS
{
    /// <summary>
    /// A SpreadsheetPanel object that extends a Form.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Member Variables
        ///  - A spreadsheet file.
        ///  - A boolean that keeps track of whether or not a cell is being edited.
        /// </summary>
        private SS.AbstractSpreadsheet ss;     // The logic spreadsheet.
        private SS.SpreadsheetClient model;    // The spreadsheet communication model.
        private bool beingEdited = false;
        private bool connected = false;        // The connection status of the model.
        private bool debugging = false;

        private String sessionName = "Test";   // The name of the current session.
        private String version = "-1";         // The model's version of the session.

        // Storage for command lines received from the server.
        private List<String> lines = new List<String>();

        // A separate form used for connection input.
        private SS.Form2 connectForm;
        // A separate form used for message debugging.
        private SS.DebugForm debugForm;

        /// <summary>
        /// Constructor for the spreadsheet form.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Initializes a class level spreadsheet that normalizes to upper and has version "ps6."
            ss = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");

            // Registers the event where the selection changes.
            spreadsheetPanel1.SelectionChanged += displaySelection;
            spreadsheetPanel1.SetSelection(2, 3);  // Selects C4 by default.

            // Creates a new client model that will handle messages to and
            // from the server.
            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;

            // Sets the initial menu item visibility.
            createSessionToolStripMenuItem.Enabled = false;
            joinExistingToolStripMenuItem.Enabled = false;
            saveSessionToolStripMenuItem.Enabled = false;
            undoLastToolStripMenuItem.Enabled = false;
            leaveSessionToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = false;

            ContentBox.Focus();
        }

        /// <summary>
        /// Alternate constructor for the spreadsheet form that takes in a filepath string.
        /// </summary>
        public Form1(string filename)
        {
            InitializeComponent();

            // Loads a spreadsheet from a file at the specified pathname.
            loadSpreadsheet(filename);

            // Registers the event where the selection changes.
            spreadsheetPanel1.SelectionChanged += displaySelection;
            spreadsheetPanel1.SetSelection(2, 3);  // Selects C4 by default.

            // Creates a new client model that will handle messages to and
            // from the server.
            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;

            // Sets the initial menu item visibility.
            createSessionToolStripMenuItem.Enabled = false;
            joinExistingToolStripMenuItem.Enabled = false;
            saveSessionToolStripMenuItem.Enabled = false;
            undoLastToolStripMenuItem.Enabled = false;
            leaveSessionToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = false;

            ContentBox.Focus();
        }

        /// <summary>
        /// Moves the current selection to the cell clicked on with the mouse.
        /// </summary>
        /// <param name="sp"></param>
        private void displaySelection(SpreadsheetPanel sp)
        {
            int row, col;
            sp.GetSelection(out col, out row);
            // Retrieves the coordinates of the current selection and updates the display boxes.
            updateBoxes(col, row);
            // Sets focus to the cell content box.
            ContentBox.Focus();
            ContentBox.SelectionStart = ContentBox.Text.Length;
        }

        /*
         * FORM METHODS FOR MENU ITEMS
         */

        /// <summary>
        /// Handles the "New" menu item listed under File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Tell the application context to run the form on the same
            // thread as the other forms.
            DemoApplicationContext.getAppContext().RunForm(new Form1());
        }

        /// <summary>
        /// Handles the "Open" menu item listed under File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // Attempts to read a file from an input filepath name.
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // After the user has pushed the "OK" button, creates a new spreadsheet window with the alternate constructor.
                    Form f = new Form1(openFileDialog1.FileName);
                    DemoApplicationContext.getAppContext().RunForm(f);
                }
            }
            catch (Exception c)
            {
                string result = string.Empty;           // Initializes a string to store the resulting exception message.
                if (c is SpreadsheetReadWriteException)
                    result = c.Message;                 // Handles the case where a SpreadsheetReadWriteException was thrown.
                else                                    // Handles all other cases.
                    result = c.Message;
                MessageBox.Show("Could not read file from disk. \n Original error: " + c.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        /// <summary>
        /// Handles the "Save" menu item listed under File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // Attempts to save a file with an input filepath name.
            {
                if (saveFileDialog1.FileName == "")  // If the filepath has not been set...
                {
                    saveAsToolStripMenuItem_Click(sender, e); // Run as "Save As"
                }
                else // Otherwise, save the file using the prexisting filepath.
                {
                    ss.Save(saveFileDialog1.FileName);
                }
            }
            catch (Exception c)  // Catchs any exceptions thrown while attempting to save the file and displays a specific message.
            {
                MessageBox.Show("Could not save file to disk. \n Original error: " + c.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Handles the "Save As" menu item listed under File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // Attempts to save a file with an input filepath name.
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ss.Save(saveFileDialog1.FileName); // Uses the filepath provided by the user to save the file.
                }
            }
            catch (Exception c) // Catchs any exceptions thrown while attempting to save the file and displays a specific message.
            {
                MessageBox.Show("Could not save file to disk. \n Original error: " + c.Message, "Save As", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Handles the "Close" menu item listed under File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close(); // Run as a form-closing action. *See below.*
        }

        /// <summary>
        /// Handles the "About" menu item listed under Help.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Creates a message to display in the "About" message box.
            string message = "Spreadsheet Application - v1.3.025";
            message += "\nLast Revision: 4/15/2013";
            message += "\n";
            message += "\nWritten by Bryan K. Smith for CS 3500";
            message += "\nModified by Bryan K. Smith for CS 3505";
            MessageBox.Show(message, "About");
        }

        /// <summary>
        /// Handles the "How To" menu item listed under Help.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void howToUseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Creates a message to display in the "How To" message box.
            string message = "How To Use the Program:";
            message += "\n";
            message += "\nNAVIGATION";
            message += "\n  -  You can navigate around the cells with the arrow keys, or by clicking on a cell";
            message += "\n     with the mouse.";
            message += "\n";
            message += "\nINPUTTING";
            message += "\n  -  You can enter cell formula values by typing on the keyboard after selecting a";
            message += "\n     cell or click directly on the formula (FX) box at the top of the application";
            message += "\n     window. If the application is connected to an online session, the enter key";
            message += "\n     must be used to accept values typed into the cell or formula box.";
            message += "\n";
            message += "\nDELETING";
            message += "\n  -  To delete while you're typing the contents of a cell, simply press either the";
            message += "\n     backspace or delete keys. Pressing delete or backspace after selecting a cell";
            message += "\n     will delete the entire contents of the cell.";
            message += "\n";
            message += "\nCONNECTION";
            message += "\n  -  To be addressed.";
            MessageBox.Show(message, "How To");
        }

        /// <summary>
        /// Handles the "Debug" menu item listed under Help.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!debugging) // If the client is not already debugging...
            {
                debugForm = new DebugForm(); // Create a new debugging window.
                debugForm.setCallback(setDebugging); // Set the callback.
                debugForm.Show(); // Show the window.

                setDebugging(true); // Change the state of debugging.
            }
        }

        /// <summary>
        /// Handles the "Connect" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectForm = new Form2(); // Creates a new connection window.
            // Sets necessary attributes of the connection window.
            connectForm.setMessage("Please enter the IP address and port you wish to connect to.");
            connectForm.setLabels("Address:", "Port:");
            connectForm.setButtonText("Connect", "Cancel");
            connectForm.setDefaultInput("lab1-27.eng.utah.edu", "1992");
            connectForm.setCallback(connect);
            connectForm.Show(); // Shows the connection window.
        }

        /// <summary>
        /// Handles the "Create Session" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectForm = new Form2(); // Creates a new connection window.
            // Sets the necessary attributes of the connection window.
            connectForm.setMessage("Please enter the name and password of the session you wish to create.");
            connectForm.setLabels("Name:", "Password:");
            connectForm.setButtonText("Create", "Cancel");
            connectForm.setDefaultInput("", "");
            connectForm.setCallback(create);
            connectForm.Show(); // Shows the connection window.
        }

        /// <summary>
        /// Handles the "Join Session" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void joinExistingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectForm = new Form2(); // Creates a new connection window.
            // Sets the necessary attributes of the connection window.
            connectForm.setMessage("Please enter the name and password of the session you wish to join.");
            connectForm.setLabels("Name:", "Password:");
            connectForm.setButtonText("Join", "Cancel");
            connectForm.setDefaultInput("", "");
            connectForm.setCallback(join);
            connectForm.Show(); // Shows the connection window.
        }

        /// <summary>
        /// Handles the "Save Session" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            String message = "SAVE\n"; // Prepares an outgoing string.
            message += "Name:" + sessionName + "\n"; // Appends the session name.

            // If the client is in debug mode, it sends the message to the debug window.
            if (debugging) debugForm.addClientToServer(message);
            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }
        }

        /// <summary>
        /// Handles the "Undo Last" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            String message = "UNDO\n"; // Prepares an outgoing string.
            message += "Name:" + sessionName + "\n"; // Appends the session name.
            message += "Version:" + version + "\n"; // Appends the session version.

            // If the client is in debug mode, it sends the message to the debug window.
            if (debugging) debugForm.addClientToServer(message);
            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }
        }

        /// <summary>
        /// Handles the "Leave Session" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leaveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            // Prepares an outgoing message.
            String message = "LEAVE\nName:" + sessionName + "\n";
            // If the client is in debug mode, the message is sent to the debug window.
            if (debugging) debugForm.addClientToServer(message);
            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }

            SessionBox.Text = "Not connected to any sessions.";
            if (debugging) debugForm.addMessage("You have successfully left the session.");
            sessionName = ""; // Resets the session name.
            version = "-1";
            clearSpreadsheet();

            // Modifies menu item visibility under the Server tab.
            createSessionToolStripMenuItem.Enabled = true;
            joinExistingToolStripMenuItem.Enabled = true;
            saveSessionToolStripMenuItem.Enabled = false;
            undoLastToolStripMenuItem.Enabled = false;
            leaveSessionToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Handles the "Disconnect" menu item listed under Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (debugging) debugForm.addMessage("Attempting to disconnect from the host server");
            disconnect();
            if (debugging) debugForm.addMessage("Successfully disconnected from the host server.");
        }

        /// <summary>
        /// Handles the event where the user closes the spreadsheet form window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ss.Changed == true) // If the spreadsheet has been altered since the last time it was created or saved,
            {
                DialogResult result = new DialogResult(); // Creates a message box that asks the user if they want to save before closing.
                result = MessageBox.Show("Do you want to save the spreadsheet before closing?", "Close", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result.ToString().Equals("Yes"))      // If the response is yes, runs "Save As."
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                    model.Disconnect();
                    e.Cancel = false;
                }
                else if (result.ToString().Equals("No"))  // If the response is no, closes the window.
                {
                    model.Disconnect();
                    e.Cancel = false;
                }
                else                                      // Otherwise the window does nothing and the prompt closes.
                    e.Cancel = true;
            }
            else   // If the spreadsheet hasn't been altered, the window closes.
            {
                model.Disconnect();
                e.Cancel = false;
            }
        }

        /*
         * TEXTBOX METHODS
         */

        /// <summary>
        /// A method that handles the event where the text inside the content box is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentBox_TextChanged(object sender, EventArgs e)
        {
            beingEdited = true;

            if (!connected)
                updateCells(NameBox.Text, ContentBox.Text.ToUpper());
        }

        /// <summary>
        /// A method that handles the event where a key is pressed down while the focus is on the content box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentBox_KeyDown(object sender, KeyEventArgs e)
        {
            int row, col; // Establishes variables to store row and column information.

            if (e.KeyCode == Keys.Left)  // If the left arrow key is pressed, moves the selection 1 column to the left.
            {
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col > 0) // If possible.
                    spreadsheetPanel1.SetSelection(col - 1, row);
                spreadsheetPanel1.GetSelection(out col, out row);

                updateBoxes(col, row); // Updates the display text boxes.
            }
            if (e.KeyCode == Keys.Right) // If the right arrow key is pressed, moves the selection 1 column to the right.
            {
                spreadsheetPanel1.GetSelection(out col, out row);
                if (col < 100) // If possible.
                    spreadsheetPanel1.SetSelection(col + 1, row);
                spreadsheetPanel1.GetSelection(out col, out row);

                updateBoxes(col, row); // Updates the display text boxes.
            }
            if (e.KeyCode == Keys.Up) // If the up arrow key is pressed, moves the selection 1 row upwards.
            {
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row > 0) // If possible.
                    spreadsheetPanel1.SetSelection(col, row - 1);
                spreadsheetPanel1.GetSelection(out col, out row);

                updateBoxes(col, row); // Updates the display text boxes.
            }
            if (e.KeyCode == Keys.Down) // If the down arrow key is pressed, moves the selection 1 row downwards.
            {
                spreadsheetPanel1.GetSelection(out col, out row);
                if (row < 100) // If possible.
                    spreadsheetPanel1.SetSelection(col, row + 1);
                spreadsheetPanel1.GetSelection(out col, out row);

                updateBoxes(col, row); // Updates the display text boxes.
            }
            if (connected && e.KeyCode == Keys.Return)
            {
                String message = "CHANGE\n";
                message += "Name:" + sessionName + "\n";
                message += "Version:" + version + "\n";
                message += "Cell:" + NameBox.Text + "\n";
                message += "Length:" + ContentBox.Text.Length + "\n";
                //if (ContentBox.Text.ToUpper() == "")
                //    message += " \n";
                //else
                message += ContentBox.Text.ToUpper() + "\n";

                if (debugging) debugForm.addClientToServer(message);
                model.SendMessage(message);
            }
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) // If the delete or return keys are pressed...
            {
                if (ContentBox.Text != "" && beingEdited == false)  // And the cell is not currently being edited...
                    ContentBox.Clear();                             // Clears the cell contents.
            }
        }

        /// <summary>
        /// A helper function that enables the user to select keys using the Name textbox.
        /// Goes against the specification designated by the instructor, as the name textbox was
        /// supposed to be implemented as a read-only display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            int row, col;
            String oldName;
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                NameBox.Text = NameBox.Text.ToUpper();
                spreadsheetPanel1.GetSelection(out col, out row);
                char column = (char)(col + 65);
                oldName = column + (row + 1).ToString();

                col = ConvertLetterToNumber(NameBox.Text[0]);
                if (Int32.TryParse(NameBox.Text.ToString().Substring(1), out row))
                {
                    if (row < 100 && row > 0)
                    {
                        spreadsheetPanel1.SetSelection(col, row - 1);

                        updateBoxes(col, row - 1);
                        ContentBox.Focus();
                    }
                    else
                    {
                        ErrorBox.Text = "THAT'S NOT A CELL NAME, DUMBASS! JESUS!";

                        NameBox.Text = oldName;
                        ContentBox.Focus();
                    }
                }
                else
                {
                    ErrorBox.Text = "THAT'S NOT A CELL NAME, DUMBASS! JESUS!";

                    NameBox.Text = oldName;
                    ContentBox.Focus();
                }
            }
        }

        /*
         * HELPER METHODS
         */ 

        /// <summary>
        /// A helper method that converts the a letter character into a number.
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        private int ConvertLetterToNumber(char letter)
        {
            string list = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int number = -1;
            for (int i = 0; i < 26; i++) // Iterates through the alphabet.
            {
                if (list[i] == letter)   // If the character at the index matches the input letter, 
                    number = i;          // the index is stored in a temporary variable
            }
            return number;  // Returns the number.
        }

        /// <summary>
        /// A helper method that updates the text boxes of the GUI. 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void updateBoxes(int col, int row)
        {
            // Converts the column and row into a cell name.
            string name;
            char column = (char)(col + 65);
            name = column + (row + 1).ToString();
            NameBox.Text = name; // Modifies the name box to display the cell name of the current selection.

            // If the contents do not match the value, the content is treated as a Formula/FormulaError object.
            if (ss.GetCellContents(NameBox.Text) != ss.GetCellValue(NameBox.Text))
                ContentBox.Text = "=" + ss.GetCellContents(NameBox.Text).ToString(); // Appends an "=" to the beginning of the content string.
            else // Otherwise, the content box is updated to display the content of the current selection.
                ContentBox.Text = ss.GetCellContents(NameBox.Text).ToString();
            ValueBox.Text = ss.GetCellValue(NameBox.Text).ToString(); // Updates the value displayed for the current selection.

            ErrorBox.Clear();

            beingEdited = false; // Records that the current selection is no longer being edited.
        }

        /// <summary>
        /// A helper method that updates the logic and display for the spreadsheet cells.
        /// </summary>
        /// <param name="cellName"></param>
        /// <param name="content"></param>
        private void updateCells(String cellName, string content)
        {
            // Update the cell and junk.
            int row, col; // Establishes variables to store row and column information.
            String value;
            IEnumerable<string> ToBeUpdated; // Creates an enumerable to store the cells of the spreadsheet to be updated.
            try // Attempts to change the contents and value of the cell within the spreadsheet GUI.
            {
                // Sets the contents and values of the logic cells and stores the returned set in the enumerable.
                ToBeUpdated = ss.SetContentsOfCell(cellName, content.ToUpper());
                spreadsheetPanel1.GetSelection(out col, out row);     // Retrieves the column and row information of the current selection.
                spreadsheetPanel1.GetValue(col, row, out value);      // Gets the value of the currently selected cell.
                spreadsheetPanel1.SetValue(col, row, ss.GetCellValue(NameBox.Text).ToString()); // Sets the value of the currently selection.
                // Updates the text of the box that displays the current cell value.
                ValueBox.Invoke(new Action(() => { ValueBox.Text = ss.GetCellValue(NameBox.Text).ToString(); }));

                foreach (string cell in ToBeUpdated)  // Updates each of the cells that were directly or indirectly dependent on the current selection.
                {
                    col = ConvertLetterToNumber(cell[0]);            // Convert the letter into a column coordinate.
                    Int32.TryParse(cell.Substring(1), out row);      // Attempts to parse the rest of the cell name as the row.
                    spreadsheetPanel1.GetValue(col, row, out value); // Changes the GUI cell to reflect the changes to the spreadsheet logic.
                    spreadsheetPanel1.SetValue(col, row - 1, ss.GetCellValue(cell).ToString());
                }
            }
            catch (Exception c) // Catches any exceptions thrown and displays a specific message in the error box.
            {
                string report = c.Message;
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = report; }));
            }
        }

        /// <summary>
        /// A helper method that connects the server to the specified
        /// IP address along the specified port.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        private void connect(String IP, String port)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            int result; // Attempts to parse the port string as an integer.
            if (!int.TryParse(port, out result)) 
            {
                ErrorBox.Text = "Unable to resolve port number.";
                return;
            }

            if (debugging) debugForm.addMessage("Attempting to establish connection to host server");
            try // Attempts to connect the client to the IP address along the port.
            {
                model.Connect(IP, result);
                if (debugging) debugForm.addMessage("Established connection with the host server.");
                SessionBox.Text = "Connected to the host server.";

                connected = true; // Changes the state of the spreadsheet.

                // Modifies menu item visibility under the Server tab.
                connectToolStripMenuItem.Enabled = false;
                createSessionToolStripMenuItem.Enabled = true;
                joinExistingToolStripMenuItem.Enabled = true;
                disconnectToolStripMenuItem.Enabled = true;
            }
            catch (Exception ex) // Otherwise, if some exception was caught.
            {
                ErrorBox.Text = ex.Message.ToString();
            }
        }

        /// <summary>
        /// A helper method that prepares a CREATE message and sends it
        /// to the server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        private void create(String name, String pass)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            try
            {
                String message = "CREATE\n"; // Prepares an outgoing message.
                message += "Name:" + name + "\n"; // Appends the session name.
                message += "Password:" + pass + "\n"; // Appends the password.

                // If the client is in debugging mode, send the message to the debug
                // window.
                if (debugging) debugForm.addClientToServer(message);
                model.SendMessage(message); // Sends the message down the socket.
            }
            catch (Exception e)
            {
                ErrorBox.Text = "In create: " + e.Message.ToString();
            }
        }

        /// <summary>
        /// A helper method that prepares a JOIN message and sends it to
        /// the server.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        private void join(String name, String pass)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            try 
            {
                String message = "JOIN\n"; // Prepares an outgoing message.
                message += "Name:" + name + "\n"; // Appends the session name.
                message += "Password:" + pass + "\n"; // Appends the password.

                // If the client is in debugging mode, send the message to the debug
                // window.
                if (debugging) debugForm.addClientToServer(message);
                model.SendMessage(message); // Sends the message down the socket.
            }
            catch (Exception e) 
            {
                ErrorBox.Text = "In join: " + e.Message.ToString();
            }
        }

        /// <summary>
        /// A helper method that disconnects the client model.
        /// </summary>
        private void disconnect()
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            try // Attempts to disconnect the client.
            {
                model.Disconnect(); // Closes the socket on the client's end.
                SessionBox.Text = "Not connected to the host server.";

                clearSpreadsheet();

                connected = false; // Changes the state of the spreadsheet.

                // Modifies menu item visibility under the Server tab.
                connectToolStripMenuItem.Enabled = true;
                createSessionToolStripMenuItem.Enabled = false;
                joinExistingToolStripMenuItem.Enabled = false;
                saveSessionToolStripMenuItem.Enabled = false;
                undoLastToolStripMenuItem.Enabled = false;
                leaveSessionToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = false;
            }
            catch (Exception ex) // Otherwise, if an exception was caught...
            {
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "In Disconnect: " + ex.Message.ToString(); }));
            }
        }

        /// <summary>
        /// A helper method that loads a spreadsheet from file.
        /// </summary>
        /// <param name="filename"></param>
        private void loadSpreadsheet(String filename)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            int row, col; // Establishes variables to store row and column information.
            string value; // Establishes a variable to contain the cell value.
            try
            {
                // Initializes a new class level spreadsheet object from the filepath.
                ss = new Spreadsheet(filename, s => true, s => s.ToUpper(), "ps6");
                // Initializes a IEnumberable to store all non-empty cells from the spreadsheet. 
                IEnumerable<string> ToUpdate = ss.GetNamesOfAllNonemptyCells();
                foreach (string cell in ToUpdate)                    // For each cell in the enumerable...
                {
                    col = ConvertLetterToNumber(cell[0]);            // Convert the letter into a column coordinate.
                    Int32.TryParse(cell.Substring(1), out row);      // Attempts to parse the rest of the cell name as the row.
                    spreadsheetPanel1.GetValue(col, row, out value); // Changes the GUI cell to reflect the changes to the spreadsheet logic.
                    spreadsheetPanel1.SetValue(col, row - 1, ss.GetCellValue(cell).ToString());
                }
            }
            catch (Exception c)  // Catches any exceptions that are thrown and creates a message box to display it.
            {
                MessageBox.Show("Could not read file from disk. \nOriginal error: " + c.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// A helper method that clears the cells of a spreadsheet.
        /// </summary>
        private void clearSpreadsheet()
        {
            // Initializes a new, blank spreadsheet.
            ss = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
            spreadsheetPanel1.Clear();
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            updateBoxes(col, row);
        }

        /// <summary>
        /// A helper mehtod that changes the debugging state of the spreadsheet.
        /// </summary>
        /// <param name="state"></param>
        private void setDebugging(bool state)
        {
            debugging = state;
        }

        /// <summary>
        /// Perfoms an action on the client based on the string
        /// received.
        /// </summary>
        /// <param name="line"></param>
        private void MessageReceived(String line)
        {
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); }));
            if (line.StartsWith("System.Net"))
            {
                disconnect();
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "The server closed unexpectedly."; }));
                this.Invoke(new Action(() =>
                {
                    // Modifies menu item visibility under the Server tab.
                    connectToolStripMenuItem.Enabled = true;
                    createSessionToolStripMenuItem.Enabled = false;
                    joinExistingToolStripMenuItem.Enabled = false;
                    saveSessionToolStripMenuItem.Enabled = false;
                    leaveSessionToolStripMenuItem.Enabled = false;
                    disconnectToolStripMenuItem.Enabled = false;
                }));
            }
            else
            {
                if (debugging) debugForm.addServerToClient(line);

                if (line.StartsWith("CREATE") || line.StartsWith("JOIN") || line.StartsWith("CHANGE")
                    || line.StartsWith("UNDO") || line.StartsWith("UPDATE") || line.StartsWith("SAVE")
                    || line.StartsWith("ERROR"))
                    lines.Clear();

                lines.Add(line);
                String first = lines[0];

                // If first line starts with "SOME TAG" and lines.size() is command size
                if (first.StartsWith("CREATE OK") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5);

                    if (debugging) debugForm.addMessage(name + " successfully created.");
                }
                else if (first.StartsWith("CREATE FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5);
                    String message = lines[1];

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " failed to create: " + message; }));
                }
                else if (first.StartsWith("JOIN OK") && lines.Count() == 5)
                {
                    sessionName = lines[1].Substring(5);
                    version = lines[2].Substring(8);
                    int result;
                    int.TryParse(lines[3].Substring(7), out result);
                    String xml = lines[4].Substring(0, result);

                    // Save xml into a temporary file.
                    String path = "temp.ss";
                    File.Create(path).Dispose();
                    using (FileStream fs = new FileStream(path,
                        FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        TextWriter tw = new StreamWriter(path, true);
                        tw.WriteLine(xml);
                        tw.Close();
                    }
                    // Load the file into the spreadsheet.
                    loadSpreadsheet(path);
                    // Delete the file.
                    File.Delete(path);

                    this.Invoke(new Action(() =>
                    {
                        createSessionToolStripMenuItem.Enabled = false;
                        joinExistingToolStripMenuItem.Enabled = false;
                        undoLastToolStripMenuItem.Enabled = true;
                        saveSessionToolStripMenuItem.Enabled = true;
                        leaveSessionToolStripMenuItem.Enabled = true;
                    }));

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));

                    lines.Clear();
                }
                else if (first.StartsWith("JOIN FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5);
                    String message = lines[2];

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Failed to join " + name + ": " + message; }));

                    lines.Clear();
                }
                else if (first.StartsWith("CHANGE OK") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5);
                    version = lines[2].Substring(8);


                    updateCells(NameBox.Text, ContentBox.Text.ToUpper());

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage(name + " was successfully modified.");

                    lines.Clear();
                }
                else if (first.StartsWith("CHANGE FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5);
                    String message = lines[2];

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " was unable to be modified: " + message; }));

                    lines.Clear();
                }
                else if (first.StartsWith("UNDO OK") && lines.Count() == 6)
                {
                    // Do something.
                    String name = lines[1].Substring(5);
                    version = lines[2].Substring(8);
                    String cellName = lines[3].Substring(5);
                    int result;
                    int.TryParse(lines[4].Substring(7), out result);
                    String content = lines[5].Substring(0, result);

                    updateCells(cellName, content);

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage("The last action of " + name + " was successfully undid.");

                    lines.Clear();
                }
                else if (first.StartsWith("UNDO END") && lines.Count() == 3)
                {
                    // Do something.
                    String name = lines[1].Substring(5);

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "There are no unsaved changes on " + name + "."; }));

                    lines.Clear();
                }
                else if (first.StartsWith("UNDO WAIT") && lines.Count() == 3)
                {
                    // Do something.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Your version is out of date, please wait for an update."; }));

                    lines.Clear();
                }
                else if (first.StartsWith("UNDO FAIL") && lines.Count() == 3)
                {
                    // Do something.
                    String name = lines[1].Substring(5);
                    String message = lines[2];

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " was unable to be undid: " + message; }));

                    lines.Clear();
                }
                else if (first.StartsWith("UPDATE") && lines.Count() == 6)
                {
                    // Do something.
                    String name = lines[1].Substring(5);
                    version = lines[2].Substring(8);
                    String cellName = lines[3].Substring(5);
                    int result;
                    int.TryParse(lines[4].Substring(7), out result);
                    String content = lines[5].Substring(0, result);

                    // Update the cell and junk.
                    int row, col; // Establishes variables to store row and column information.
                    String value;
                    IEnumerable<string> ToBeUpdated; // Creates an enumerable to store the cells of the spreadsheet to be updated.
                    try // Attempts to change the contents and value of the cell within the spreadsheet GUI.
                    {
                        // Sets the contents and values of the logic cells and stores the returned set in the enumerable.
                        ToBeUpdated = ss.SetContentsOfCell(cellName, content.ToUpper());
                        spreadsheetPanel1.GetSelection(out col, out row);     // Retrieves the column and row information of the current selection.
                        spreadsheetPanel1.GetValue(col, row, out value);      // Gets the value of the currently selected cell.
                        spreadsheetPanel1.SetValue(col, row, ss.GetCellValue(NameBox.Text).ToString()); // Sets the value of the currently selection.
                        ValueBox.Text = ss.GetCellValue(NameBox.Text).ToString(); // Updates the text of the box that displays the current cell value.

                        foreach (string cell in ToBeUpdated)  // Updates each of the cells that were directly or indirectly dependent on the current selection.
                        {
                            col = ConvertLetterToNumber(cell[0]);            // Convert the letter into a column coordinate.
                            Int32.TryParse(cell.Substring(1), out row);      // Attempts to parse the rest of the cell name as the row.
                            spreadsheetPanel1.GetValue(col, row, out value); // Changes the GUI cell to reflect the changes to the spreadsheet logic.
                            spreadsheetPanel1.SetValue(col, row - 1, ss.GetCellValue(cell).ToString());
                        }
                    }
                    catch (Exception c) // Catches any exceptions thrown and displays a specific message in the error box.
                    {
                        string report = c.Message;
                        ErrorBox.Invoke(new Action(() => { ErrorBox.Text = report; }));
                    }

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage("Spreadsheet was successfully updated.");

                    lines.Clear();
                }
                else if (first.StartsWith("SAVE OK") && lines.Count() == 2)
                {
                    // Do something.
                    String name = lines[1].Substring(5);

                    if (debugging) debugForm.addMessage(name + " was successfully saved.");

                    lines.Clear();
                }
                else if (first.StartsWith("SAVE FAIL") && lines.Count() == 3)
                {
                    // Do something.
                    String name = lines[1].Substring(5);
                    String message = lines[2];

                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " was unable to be saved: " + message; }));

                    lines.Clear();
                }
                else if (first.StartsWith("ERROR") && lines.Count() == 1)
                {
                    // Do something.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Sum terbible erlor has accrued."; }));

                    lines.Clear();
                }
            }
        }
    }
}