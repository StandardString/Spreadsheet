/* Author:      Bryan K. Smith
 * Modified By: Owen Krafft, Dickson Chiu, Austin Nester, and Bryan K. Smith for CS 3505
 * Class:       CS 3500
 * Date:        10/17/2012
 * Version:     1.3.025
 *
 * Revision History:
 *           1.1.00 - 10/1/2012 - Created new methods to match the updated specification of AbstractSpreadsheet.
 *           1.2.00 - 10/17/2012 - Modified acceptible variable formats and added GUI element.
 *           1.3.00 - 4/15/2013 - Added sockets and connection support for online spreadsheets.
 */
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
        private bool quickType = false;

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
            // from the server, and registers an IncomingLineEvent.
            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;

            // Sets the initial menu item visibility.
            createSessionToolStripMenuItem.Enabled = false;
            joinExistingToolStripMenuItem.Enabled = false;
            saveSessionToolStripMenuItem.Enabled = false;
            undoLastToolStripMenuItem.Enabled = false;
            leaveSessionToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = false;

            ContentBox.Focus(); // Gives the content box control focus.
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
            // from the server, and registers an IncomingLineEvent.
            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;

            // Sets the initial menu item visibility.
            createSessionToolStripMenuItem.Enabled = false;
            joinExistingToolStripMenuItem.Enabled = false;
            saveSessionToolStripMenuItem.Enabled = false;
            undoLastToolStripMenuItem.Enabled = false;
            leaveSessionToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = false;

            ContentBox.Focus(); // Gives the content box control focus.
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

        private void enableQuickTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (quickType)
                quickType = false;
            else
                quickType = true;
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
            connectForm = new Form2(); // Creates a new customizable connection window.
            // Sets necessary attributes of the connection window.
            connectForm.setMessage("Please enter the IP address and port you wish to connect to.");
            connectForm.setLabels("Address:", "Port:");
            connectForm.setButtonText("Connect", "Cancel");
            connectForm.setDefaultInput("lab1-20.eng.utah.edu", "1984");
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
            connectForm = new Form2(); // Creates a new customizable connection window.
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
            connectForm = new Form2(); // Creates a new customizable connection window.
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

            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }
            // If the client is in debug mode, it sends the message to the debug window.
            if (debugging) debugForm.addClientToServer(message);
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

            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }
            // If the client is in debug mode, it also sends the message to the debug window.
            if (debugging) debugForm.addClientToServer(message);
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
            try { model.SendMessage(message); } // Attempts to send the message.
            catch (Exception ex) { ErrorBox.Text = ex.Message.ToString(); }
            // If the client is in debug mode, the message is also sent to the debug window.
            if (debugging) debugForm.addClientToServer(message);

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
            disconnect(); // Disconnects the client from the host server.
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
            beingEdited = true; // Marks that the cell is currently being edited.

            if (!connected) // If the client is not connected to the server...
                updateCells(NameBox.Text, ContentBox.Text.ToUpper()); // Cells are updated on every keypress.
            else if (connected && quickType)
            {
                String message = "CHANGE\n";
                message += "Name:" + sessionName + "\n";
                message += "Version:" + version + "\n";
                message += "Cell:" + NameBox.Text + "\n";
                message += "Length:" + ContentBox.Text.Length + "\n";
                message += ContentBox.Text.ToUpper() + "\n";

                if (debugging) debugForm.addClientToServer(message);
                model.SendMessage(message);
            }
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
            if (connected && e.KeyCode == Keys.Return) // If the return key is pressed and the client is connected.
            {
                String oldContent;
                spreadsheetPanel1.GetSelection(out col, out row);
                spreadsheetPanel1.GetValue(col, row, out oldContent);      // Gets the value of the currently selected cell.

                String result;
                result = updateCells(NameBox.Text, ContentBox.Text.ToUpper());
                updateBoxes(col, row);
                if (result != "Cells were successfully updated.")
                {
                    // Revert to old value and throw exception to error box.
                    updateCells(NameBox.Text, oldContent);
                    updateBoxes(col, row);

                    ErrorBox.Text = result;
                }
                else
                {
                    String message = "CHANGE\n"; // Generates a new outgoing message string.
                    message += "Name:" + sessionName + "\n"; // Appends the session name,
                    message += "Version:" + version + "\n"; // ... the version number,
                    message += "Cell:" + NameBox.Text + "\n"; // ... the name of the modified cell,
                    message += "Length:" + ContentBox.Text.Length + "\n"; // ... the length in characters of the contents,
                    message += ContentBox.Text.ToUpper() + "\n"; // ... and the contents of the modified cell.

                    model.SendMessage(message); // Sends the message donw the socket.
                    // If debugging is enabled, the message is also sent to the debug window.
                    if (debugging) debugForm.addClientToServer(message);
                } 
            }
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) // If the delete or back keys are pressed...
            {
                if (ContentBox.Text != "" && beingEdited == false)  // And the cell is not currently being edited...
                {
                    ContentBox.Clear();                             // Clears the cell contents.
                    String message = "CHANGE\n"; // Generates an outgoing message.
                    message += "Name:" + sessionName + "\n"; // Appends the session name,
                    message += "Version:" + version + "\n"; // ... the version number.
                    message += "Cell:" + NameBox.Text + "\n"; // ... the name of the modified cell...
                    message += "Length:" + ContentBox.Text.Length + "\n"; // ... the length, in characters, of the contents,
                    message += ContentBox.Text.ToUpper() + "\n"; // ... and the content of the modified cell.

                    model.SendMessage(message); // Sends the message down the socket.
                    // If debugging is enabled, the message is also sent to the debug window.
                    if (debugging) debugForm.addClientToServer(message);
                }
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
            String oldName; // Storage for the name of the current cell.
            // If the enter or return keys are pressed while the client is in the name box...
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                NameBox.Text = NameBox.Text.ToUpper(); // Capitalizes the characters of the new cell in the name box.
                spreadsheetPanel1.GetSelection(out col, out row); // Retrieves the current selection.
                char column = (char)(col + 65); // Generates a column number, based on the letter of the cell name.
                oldName = column + (row + 1).ToString(); // And stores the name of the current cell.

                col = ConvertLetterToNumber(NameBox.Text[0]); // Converts the letter of the new cell into a column number.
                // Attempts to parse the rest of the cell name as a 32-bit integer.
                if (Int32.TryParse(NameBox.Text.ToString().Substring(1), out row))
                {
                    if (row < 100 && row > 0) // If the number is between 1 and 99...
                    {
                        spreadsheetPanel1.SetSelection(col, row - 1); // The selection is updated.

                        updateBoxes(col, row - 1); // Name, Content, Value, and Error boxes are updated accordingly,
                        ContentBox.Focus(); // ... and focus is returned to the content box.
                    }
                    else // Otherwise, if the row is out of range...
                    {
                        ErrorBox.Text = "Invalid cell name."; // Prints an error.
                        NameBox.Text = oldName; // Recovers the old cell name,
                        ContentBox.Focus(); // .. and returns focus to the content box.
                    }
                }
                else // Otherwise, if the parse failed...
                {
                    ErrorBox.Text = "Invalid cell name."; // An error is printed,
                    NameBox.Text = oldName; // ... the old cell name is recovered,
                    ContentBox.Focus(); // .. and focus is returned to the content box.
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
        private string updateCells(String cellName, string content)
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

                return "Cells were successfully updated.";
            }
            catch (Exception c) // Catches any exceptions thrown and displays a specific message in the error box.
            {
                string report = c.Message;
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = report; }));
                return report;
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

            // If the client is operating in debug mode, a prepatory message is sent to the debug window.
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

                model.SendMessage(message); // Sends the message down the socket.
                // If the client is in debugging mode, the message is also sent to the debug window.
                if (debugging) debugForm.addClientToServer(message);
            }
            catch (Exception e)
            {
                ErrorBox.Text = e.Message.ToString();
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

                model.SendMessage(message); // Sends the message down the socket.
                // If the client is in debug mode, the message is also sent to the debug window.
                if (debugging) debugForm.addClientToServer(message);
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

                clearSpreadsheet(); // Clears the spreadsheet visually and logically.

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
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = ex.Message.ToString(); }));
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
                MessageBox.Show("Could not open session spreadsheet: " + c.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                leaveSessionToolStripMenuItem_Click(this, new EventArgs());
            }
        }

        /// <summary>
        /// A helper method that clears the cells of a spreadsheet.
        /// </summary>
        private void clearSpreadsheet()
        {
            // Initializes a new, blank spreadsheet.
            ss = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
            spreadsheetPanel1.Clear(); // Clears the current display.
            int col, row; // Gets the new selection and updates boxes accordingly.
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
            ErrorBox.Invoke(new Action(() => { ErrorBox.Clear(); })); // Clears the error box.
            if (line.StartsWith("System.Net")) // If the incoming line starts with "System.Net"...
            {
                disconnect(); // The client received a closing message from the server and disconnects.
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "The server closed unexpectedly."; }));
                SessionBox.Invoke(new Action(() => { SessionBox.Text = "Not connected to the host server."; }));
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
            else // Otherwise, the line is considered valid. 
            {
                // If the client has debugging enabled, the incoming line is sent to the debugging window.
                if (debugging) debugForm.addServerToClient(line);

                // If the incoming line begins with a command, the line array is cleared.
                if (line.StartsWith("CREATE") || line.StartsWith("JOIN") || line.StartsWith("CHANGE")
                    || line.StartsWith("UNDO") || line.StartsWith("UPDATE") || line.StartsWith("SAVE")
                    || line.StartsWith("ERROR"))
                    lines.Clear();

                lines.Add(line); // Adds the incoming line to the lines array.
                String first = lines[0]; // Stores the first line in a more intuitive variable.

                // If first line starts with "CREATE OK" and number of lines in the array is 3...
                if (first.StartsWith("CREATE OK") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.

                    if (debugging) debugForm.addMessage(name + " successfully created.");
                    SessionBox.Invoke(new Action(() => { SessionBox.Text = name + " session successfully created."; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("CREATE FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    String message = lines[2];           // Retrieves the error message from the third line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " failed to create: " + message; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("JOIN OK") && lines.Count() == 5)
                {
                    sessionName = lines[1].Substring(5); // Retrieves and stores the session name.
                    version = lines[2].Substring(8); // Retrieves and stores the version name.
                    int result; // Creates an integer to store the parsing result,
                    int.TryParse(lines[3].Substring(7), out result); // ... and attempts to parse the xml length.
                    String xml = lines[4].Substring(0, result); // Retrieves the xml from the final line.

                    String path = "temp.ss"; // Creates a file path string.
                    File.Create(path).Dispose(); // Creates a new empty file at the path.
                    using (FileStream fs = new FileStream(path,
                        FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        TextWriter tw = new StreamWriter(path, true); // Sets the file as writable,
                        tw.WriteLine(xml); // ... and writes the xml string to the file.
                        tw.Close(); // Closes the file.
                    }
                    loadSpreadsheet(path);// Loads the file into the spreadsheet.
                    File.Delete(path); // Deletes the file.

                    this.Invoke(new Action(() =>
                    {
                        // Modifies the visibility of the dropdown menu items under Server.
                        createSessionToolStripMenuItem.Enabled = false;
                        joinExistingToolStripMenuItem.Enabled = false;
                        undoLastToolStripMenuItem.Enabled = true;
                        saveSessionToolStripMenuItem.Enabled = true;
                        leaveSessionToolStripMenuItem.Enabled = true;
                    }));

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("JOIN FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    String message = lines[2]; // Retrieves the error message from the third line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Failed to join " + name + ": " + message; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("CHANGE OK") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the name of the session from the second line.
                    version = lines[2].Substring(8); // Retrieves and stores the version number from the third line.

                    updateCells(NameBox.Text, ContentBox.Text.ToUpper()); // Updates the spreadsheet cells.

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage(name + " was successfully modified.");

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("CHANGE FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the name of the session from the second line.
                    String message = lines[2]; // Retrieves the error message from the third line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " was unable to be modified: " + message; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("UNDO OK") && lines.Count() == 6)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    version = lines[2].Substring(8); // Retrieves and stores the version number from the third line.
                    String cellName = lines[3].Substring(5); // Retrieves the cell name from the fourth line.
                    int result; // Creates an integer to store the result of parsing.
                    int.TryParse(lines[4].Substring(7), out result); // Attempts to parse the content length as an integer.
                    String content = lines[5].Substring(0, result); // Retrieves the cell content from the sixth line.

                    updateCells(cellName, content); // Updates the cells in the spreadsheet.

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage("The last action of " + name + " was successfully undid.");

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("UNDO END") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "There are no unsaved changes on " + name + "."; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("UNDO WAIT") && lines.Count() == 3)
                {
                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Your version is out of date, please wait for an update."; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("UNDO FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    String message = lines[2]; // Retrieves the error message from the third line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "The last action of " + name + " could not be undone: " + message; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("UPDATE") && lines.Count() == 6)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    version = lines[2].Substring(8); // Retrieves and stores the version number from the third line.
                    String cellName = lines[3].Substring(5); // Retrieves the cell name from the fourth line.
                    int result; // Creates an integer to store the parsing result.
                    int.TryParse(lines[4].Substring(7), out result); // Attempts to parse the fifth line as a number.
                    String content = lines[5].Substring(0, result); // Retrieves the cell content from the sixth line.

                    updateCells(cellName, content.ToUpper()); // Updates the cells in the spreadsheet.

                    SessionBox.Invoke(new Action(() => { SessionBox.Text = "Connected To:  \"" + sessionName + "\" Version: " + version; }));
                    if (debugging) debugForm.addMessage("Spreadsheet was successfully updated.");

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("SAVE OK") && lines.Count() == 2)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.

                    if (debugging) debugForm.addMessage(name + " was successfully saved.");

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("SAVE FAIL") && lines.Count() == 3)
                {
                    String name = lines[1].Substring(5); // Retrieves the session name from the second line.
                    String message = lines[2]; // Retrieves the error message from the third line.

                    // Prints the error to the client window.
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = name + " could not be saved: " + message; }));

                    lines.Clear(); // Clears the array of command lines.
                }
                else if (first.StartsWith("ERROR") && lines.Count() == 1)
                {
                    ErrorBox.Invoke(new Action(() => { ErrorBox.Text = "Sum terbible erlor has accrued."; }));

                    lines.Clear(); // Clears the array of command lines.
                }
            }
        }
    }
}