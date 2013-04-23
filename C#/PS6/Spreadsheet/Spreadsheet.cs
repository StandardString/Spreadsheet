// Author:  Bryan K. Smith
// Class:   CS 3500
// Date:    9/25/2012
// Version: 1.2.118
//
// Revision History:
//           1.1.00 - 10/1/2012 - Created new methods to match the updated specification of AbstractSpreadsheet.
//           1.2.00 - 10/17/2012 - Modified acceptible variable formats and added GUI element.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using SpreadsheetUtilities;

namespace SS
{
    /// <summary>
    /// A Spreadsheet object that extends an AbstractSpreadsheet.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        /// <summary>
        /// Member Variables 
        ///  - A dependency graph of cell names.
        ///  - A dictionary of cell names and their content (as a cell object).
        /// </summary>
        private DependencyGraph dg;
        private Dictionary<string, Cell> cells;

        /// <summary>
        /// Constructor #1
        ///  - Initializes a new spreadsheet object that imposes no extra validity conditions, normalizes cell
        ///  names to themself, and has version "default."
        /// </summary>
        public Spreadsheet():base(s => true, s => s, "default")
        {
            dg = new DependencyGraph();
            cells = new Dictionary<string, Cell>();

            Changed = false; // Modifies the status of changed.
        }

        /// <summary>
        /// Constructor #2
        ///  - Initializes a new spreadsheet object that imposes extra validty conditions, normalizes every cell
        ///  name to a particular format, and allows the caller to set a version.
        /// </summary>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            dg = new DependencyGraph();
            cells = new Dictionary<string, Cell>();

            Changed = false; // Modifies the status of changed.
        }

        /// <summary>
        /// Constructor #3
        ///  - Initializes a new spreadsheet object that "loads" a spreadsheet from a saved file based on a path, imposes
        ///  extra validity conditions, normalizes every cell name to a particular format, and allows the caller to set
        ///  a version. If the version of the current spreadsheet does not match the version of a saved file, an exception
        ///  is thrown. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(string filePath, Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            dg = new DependencyGraph();
            cells = new Dictionary<string, Cell>();

            // Stores the version information of a saved spreadsheet and sets the current spreadsheet to its contents.
            string temp = GetSavedVersion(filePath);  
            //if (temp != Version)
            //    throw new SpreadsheetReadWriteException("Saved file version does not match the current spreadsheet.");

            Changed = false; // Modifies the status of changed.
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file. Sets the contents of the current
        /// spreadsheet to the cell contents of the spreadsheet saved in the named file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            // Initializes a variable to store version information.
            string v = string.Empty;
            string n = string.Empty;
            string c = string.Empty;

            try // Attempts to read from a saved file and set the contents of the current spreadsheet to the those of the saved file.
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    v = reader["version"];  // Stores the version information in the temporary variable v.
                                    break;

                                case "cell":
                                    break;

                                case "name":
                                    reader.Read();
                                    n = reader.ReadString();
                                    break;

                                case "contents":
                                    reader.Read();
                                    c = reader.ReadString();
                                    SetContentsOfCell(n, c);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)  // If an exception is caught, a message is generated based on the type of exception thrown.
            {
                string message = string.Empty;

                if (e is InvalidNameException)
                    message = "A cell name was 'null' or was otherwise invalid.";
                else if (e is ArgumentNullException)
                    message = "Attempted to place 'null' contents into a cell.";
                else if (e is FormulaFormatException)
                    message = "Attempted to place an incorrectly formatted formula into a cell.";
                else if (e is CircularException)
                    message = "Setting the contents of a cell to a formula created a circular dependency.";
                else  // If some other unexpected exception was thrown.
                    message = e.Message.ToString();
                throw new SpreadsheetReadWriteException(message);
            }
            
            return v;  // Return the version information.
        }

        /// <summary>
        /// Writes the contents of the spreadsheet to a named file using an XML format with the following
        /// structure:
        /// 
        /// <spreadsheet version="default">
        ///         <cell name="A1" contents="20"/>
        /// </spreadsheet>
        /// 
        /// If there are any problems opening, writing, or closing the file, the method throws a
        /// SpreadsheetReadWriteException.
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            if (filename == null)
                throw new SpreadsheetReadWriteException("Attempted to save to a 'null' file location.");
          
            // Initializes an enumerable containing the names of all non-empty cells in the spreadsheet.
            IEnumerable<string> CellNames = GetNamesOfAllNonemptyCells();

            // Creates an XML writer and adjusts the settings to include indentations.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";

            try // Attempts to write the cell contents of the spreadsheet in XML format and catches any exceptions thrown.
            {
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument(); // Starts the creation of the XML document.
                    writer.WriteStartElement("spreadsheet"); // Writes the "spreadsheet" element.
                    //writer.WriteAttributeString("version", Version); // Writes and closes the "version" attribute of "spreadsheet."

                    foreach (string name in CellNames)    // For each cell in the enumeration,
                    {  
                        writer.WriteStartElement("cell");  // Writes the "cell" element.

                        writer.WriteElementString("name", name); // Writes and closes the "name" attribute of "cell."
                        if (GetCellContents(name) is string)       // Writes and closes the "contents" attribute of "cell" for a string.
                            writer.WriteElementString("contents", GetCellContents(name).ToString());
                        if (GetCellContents(name) is double)       // Writes and closes the "contents" attribute of "cell" for a double.
                            writer.WriteElementString("contents", GetCellContents(name).ToString());
                        if (GetCellContents(name) is Formula)      // Writes and closes the "contents" attribute of "cell" for a formula.
                            writer.WriteElementString("contents", "=" + GetCellContents(name).ToString());

                        writer.WriteEndElement();  // Closes the "cell" element.
                    }

                    writer.WriteEndElement();   // Closes the "spreadsheet" element.
                    writer.WriteEndDocument();  // Finishes writing the document.
                }
            }
            catch(Exception e)
            {
                string message = string.Empty;
                if (e is System.IO.DirectoryNotFoundException)
                    message = ("Could not find a part of the path '" + filename + "'. ");
                else
                    message = "Some terrible error has occurred while attempting to save the spreadsheet.";
                throw new SpreadsheetReadWriteException(message);
            }

            Changed = false;   // Modifies the status of changed.
        }

        /// <summary>
        /// Enumerates the names of all non-empty cells in the spreadsheet.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            // Returns the IEnumerabe of dictionary keys. Keys containing "empty" cells are removed from the dictionary,
            // so non-empty cells are never included.
            return cells.Keys;
        }

        /// <summary>
        /// Returns the "value" of the cell, which is either a double, a string, or a FormulaError.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            if (!Syntax(name) || !IsValid(name))  // Checks the validity of the cell name.
                throw new InvalidNameException();

            if (cells.ContainsKey(name))      // If the key exists,
                return cells[name].value;     // Returns the value of the cell by name.
            else
                return string.Empty;          // Otherwise, returns an empty string.
        }

        /// <summary>
        /// Returns the "content" of the cell, which is either a double, a string, or a formula.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if (!Syntax(name) || !IsValid(name))  // Checks the validity of the cell name.
                throw new InvalidNameException();

            if (cells.ContainsKey(name))      // If the key exists,
                return cells[name].content;   // Returns the content of the cell by name.
            else
                return string.Empty;          // Otherwise, returns an empty string.
        }

        /// <summary>
        /// Sets the contents of a named cell to a specific type, depending on the input. This method returns
        /// a set containing the name of the cell, and any direct/indirect dependents of it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            name = Normalize(name);                  // Normalizes the cell name.
            if (content == null)                     // Checks content for a null value.
                throw new ArgumentNullException();
            if (!Syntax(name) || !IsValid(name))     // Checks cell name validity.
                throw new InvalidNameException();

            Changed = true;  // Marks the spreadsheet as changed.

            // Attempts to parse the string as a double.
            double result;
            if (Double.TryParse(content, out result))
            {
                return SetCellContents(name, result);   // Calls the SetCell method specific to doubles.
            }

            // Attempts to parse the string as a formula equation.
            if (content.StartsWith("="))
            {
                string equation = content.Substring(1); // Creates a substring of the characters that come after "="
                Formula f = new Formula(equation, IsValid, Normalize);  // Creates a new formula.
                return SetCellContents(name, f);        // Calls the SetCell method specific to formulae.
            }

            // Otherwise, stores the base content string.
            return SetCellContents(name, content);      // Calls the SetCell method specific to strings.
        }

        /// <summary>
        /// Sets the contents of a specific cell to a number. The method removes any previous dependees
        /// of the cell and returns a set containing the name of the cell, and any direct/indirect
        /// dependents of it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            // Stores the number in the named cell, replacing any previous content.
            cells[name] = new Cell(number);

            // Updates the spreadsheet dependencies by removing the dependees of the named cell.
            foreach (string dependee in dg.GetDependees(name))
                dg.RemoveDependency(dependee, name);

            // Stores an enumeration of the cells affected by the change.
            IEnumerable<string> ToBeUpdated = GetCellsToRecalculate(name);
            foreach (string cell in ToBeUpdated)
            {
                RecalculateCell(cell); // Recalculates the value of each cell that needs to be updated.
            }

            // Returns an enumeration of the named cell and any cells that depend on it directly or indirectly.
            return new HashSet<string>(ToBeUpdated);
        }

        /// <summary>
        /// Sets the contents of a specific cell to a string. The method removes any previous dependees
        /// of the cell and returns a set containing the name of the cell, and any direct/indirect
        /// dependents of it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text != string.Empty)             // If the text is not an empty string,
                cells[name] = new Cell(text);     // the string replaces the content of the named cell.
            else
                cells.Remove(name);               // Otherwise the named cell is removed from the dictionary.

            // Updates the spreadsheet dependencies by removing the dependees of the named cell.
            foreach (string dependee in dg.GetDependees(name))
                dg.RemoveDependency(dependee, name);

            // Stores an enumeration of the cells affected by the change.
            IEnumerable<string> ToBeUpdated = GetCellsToRecalculate(name);
            foreach (string cell in ToBeUpdated)
            {
                RecalculateCell(cell);   // Recalculates the value of each cell that needs to be updated.
            }

            // Returns an enumeration of the named cell and any cells that depend on it directly or indirectly.
            return new HashSet<string>(ToBeUpdated);
        }

        /// <summary>
        /// Sets the contents of a specific cell to a formula. The method replaces any previous dependees
        /// of the cell and returns a set containing the name of the cell, and any direct/indirect
        /// dependents of it.
        /// 
        /// If the formula creates a circular dependency in the spreadsheet, the content of the named cell 
        /// remains the same, but the script throws an exception. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formula"></param>
        /// <returns></returns>
        protected override ISet<string> SetCellContents(string name, SpreadsheetUtilities.Formula formula)
        {
            // Creates temporary variables to preserve the unchanged status of the dependency graph and the content
            // of the named cell, to be restored in the case of a circular dependency.
            IEnumerable<string> oldDependees = dg.GetDependees(name);
            Cell OldCell = new Cell(string.Empty);
            if (cells.ContainsKey(name))
                OldCell = cells[name];

            cells[name] = new Cell(formula, lookup);    // The formula replaces the content of the named cell.

            IEnumerable<string> temp = formula.GetVariables();  // Enumerates the variables of the input formula.
            dg.ReplaceDependees(name, temp);                    // Replaces the dependees of the named cell with the variables.

            // Attempts to catch a circular dependency created by formula stored in the named cell.
            // If no exception is thrown, the method simply returns the enumerated set.
            IEnumerable<string> ToBeUpdated;
            try
            {
                ToBeUpdated = GetCellsToRecalculate(name);
            }
            catch(CircularException c)
            {
                // Restores the previous content of the named cell.
                // If the cell was originally empty, it is removed from the dictionary.
                if (OldCell.content == string.Empty)
                    cells.Remove(name);
                else
                    cells[name] = OldCell;

                // Restores the previous state of the spreadsheet dependencies
                // and throws the exception.
                dg.ReplaceDependees(name, oldDependees);
                throw c;
            }

            // Stores an enumeration of the cells affected by the change.
            foreach (string cell in ToBeUpdated)
            {
                RecalculateCell(cell);   // Recalculates the value of each cell that needs to be updated.
            }

            // Returns an enumeration of the named cell and any cells that depend on it directly or indirectly.
            return new HashSet<string>(ToBeUpdated);
        }

        /// <summary>
        /// Returns an enumeration of all the cells in the spreadsheet that depend on the named cell.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dg.GetDependents(name);
        }

        /// <summary>
        /// Returns true if the spreadsheet has been modified or changed since initialization or saving,
        /// otherwise, returns false.
        /// </summary>
        public override bool Changed { get; protected set; }

        /// <summary>
        /// A method that checks the validity of a cell name string. A string is a cell name if 
        /// and only if it consists of one or more letters, followed by a non-zero digit, followed by 
        /// zero or more digits.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Syntax(string name)
        {
            if (name == null || !Regex.IsMatch(name, "^[a-zA-Z]+[0-9][0-9]*$"))
                return false;
            return true;
        }

        /// <summary>
        /// Method used to determine the value of a named string (for use in formula evaluations containing
        /// variables). If the value of the named cell is not a double, the method throws an exception.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private double lookup(string s)
        {
            object temp = GetCellValue(s);   // Stores the value of a named cell as a temporary object.
            if (temp is double)              // If the temporary object is a double,
                return (double)temp;         // return the temporary object.
            else
                throw new ArgumentException("The referenced cell has no value."); // Otherwise, throw an exception.
        }

        /// <summary>
        /// A helper method that recalculates the value of a named cell.
        /// </summary>
        /// <param name="name"></param>
        private void RecalculateCell(string name)
        {
            name = Normalize(name);
            object temp;                       // Creates a temporary object for storage.
            if (cells.ContainsKey(name))       // If the spreadsheet contains the named cell,
            {
                temp = cells[name].content;    // the content of the named cell is stored in temp.

                if (temp is Formula)           // If the temporary object is a formula,
                {
                    Formula f = (Formula)temp; // Initialize a new formula with a cast of temp.
                    if (f != null)
                        cells[name].value = f.Evaluate(lookup);  // Evaluate the formula.
                }
                else
                    cells[name].value = temp;  // Otherwise store the content as the named cell's value.
            }
        }

        /// <summary>
        /// Defines a private class Cell for use in the spreadsheet. A cell only recognizes its content
        /// and value; names are handled by the spreadsheet's dictionary. The content of a cell should
        /// be a double, a string, or a formula. The value should be a double, FormulaError, or a string.
        /// </summary>
        private class Cell
        {
            public object content;
            public object value;

            /// <summary>
            /// Cell Constructor
            /// </summary>
            /// <param name="obj"></param>
            public Cell(object obj)
            {
                if (obj is double)          // Content as double.
                {
                    content = (double)obj;
                    value = content;        // Value as double.
                }
                if (obj is string)
                {
                    content = (string)obj;  // Content as string.
                    value = content;        // Value as string.
                }
            }
            
            /// <summary>
            /// An alternate constructor that handles formula objects.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="lookup"></param>
            public Cell(object obj, Func<string, double> lookup)
            {
                try
                {
                    if (obj is Formula)
                    {
                        Formula f = (Formula)obj;
                        if (f != null)
                            content = f;         // Content as formula.
                        value = f.Evaluate(lookup); // Value as string, double, or FormulaError.
                    }
                }
                catch (Exception c)
                {
                    throw c;
                }
            }
        }
    }
}