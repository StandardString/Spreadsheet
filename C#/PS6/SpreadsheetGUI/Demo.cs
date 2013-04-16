// Author:  Bryan K. Smith
// Class:   CS 3500
// Date:    10/17/2012
// Version: 1.3.007
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
        private SS.AbstractSpreadsheet ss;
        private SS.SpreadsheetClient model;
        private bool beingEdited = false;
        private String IPAddress = "155.98.111.51";
        private int port = 1984;

        private SS.Form2 connectForm;

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

            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;
        }

        /// <summary>
        /// Alternate constructor for the spreadsheet form that takes in a filepath string.
        /// </summary>
        public Form1(string filename)
        {
            InitializeComponent();

            int row, col;   // Establishes variables to store row and column information.
            string value;   // Establishes a variable to contain the cell value.
            try
            {
                ss = new Spreadsheet(filename, s => true, s => s.ToUpper(), "ps6");  // Initializes a new class level spreadsheet object from the filepath.
                IEnumerable<string> ToUpdate = ss.GetNamesOfAllNonemptyCells();   // Initializes a IEnumberable to store all non-empty cells from the spreadsheet. 
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
                MessageBox.Show("Could not read file from disk. \n Original error: " + c.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Registers the event where the selection changes.
            spreadsheetPanel1.SelectionChanged += displaySelection;
            spreadsheetPanel1.SetSelection(2, 3);  // Selects C4 by default.

            model = new SpreadsheetClient();
            model.IncomingLineEvent += MessageReceived;
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
        }

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
        /// A method that handles the event where the text inside the content box is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentBox_TextChanged(object sender, EventArgs e)
        {
            int row, col; // Establishes variables to store row and column information.
            String value;
            IEnumerable<string> ToBeUpdated; // Creates an enumerable to store the cells of the spreadsheet to be updated.

            ErrorBox.Clear(); // Clears any error message displayed in the special error box.

            try // Attempts to change the contents and value of the cell within the spreadsheet GUI.
            {
                beingEdited = true;

                // Sets the contents and values of the logic cells and stores the returned set in the enumerable.
                ToBeUpdated = ss.SetContentsOfCell(NameBox.Text, ContentBox.Text.ToUpper());
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
                ErrorBox.Text = report;
            }
        }

        private void MessageReceived(String line)
        {
            if (!line.StartsWith("System.Net"))
                ErrorBox.Invoke(new Action(() => { ErrorBox.Text = line; }));
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
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) // If the delete or return keys are pressed...
            {
                if (ContentBox.Text != "" && beingEdited == false)  // And the cell is not currently being edited...
                    ContentBox.Clear();                             // Clears the cell contents.
            }
        }

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

        /// <summary>
        /// Handles the "About" menu item listed under Help.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Creates a message to display in the "About" message box.
            string message = "Spreadsheet Application - v1.3.007";
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
            message += "\n     window.";
            message += "\n";
            message += "\nDELETING";
            message += "\n  -  To delete while you're typing the contents of a cell, simply press either the";
            message += "\n     backspace or delete keys. Pressing delete or backspace after selecting a cell";
            message += "\n     will delete the entire contents of the cell.";
            MessageBox.Show(message, "How To");
        }

        private void toExistingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            connectForm = new Form2(this);
            connectForm.Show();
        }

        private void leaveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorBox.Text = "Disconnected from server.";
            model.Disconnect();
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

        public void connect(String user, String pass)
        {
            ErrorBox.Text = "Attempting to connect to server...";

            try 
            { 
                model.Connect(IPAddress, port, "Bob Kessler");
                ErrorBox.Text = "Established connection with the host server.";
            }
            catch (Exception e) 
            {
                ErrorBox.Text = "Failed to connect to host server: ";
                ErrorBox.Text += e.Message.ToString();
            }
        }
    }
}
