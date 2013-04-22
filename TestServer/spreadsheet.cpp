/*
 * Definitions for spreadsheet class.
 *
 * Created by Owen Krafft, Austin Nester, Dickson Chiu, and Bryan Smith
 * for CS 3505, April, 2013.
 */ 

#include <iostream>
#include <fstream>
#include "spreadsheet.h"
#include <boost/thread.hpp>


/*
 * Constructs a spreadsheet with no data in any of its cells.
 */
spreadsheet::spreadsheet()
{
   populate_cells();
}

/*
 * Constructs a spreadsheet with data in its cells
 * described by the file.
 */
spreadsheet::spreadsheet(std::string &filename)
{
   populate_cells(filename);
}

spreadsheet::~spreadsheet()
{

}


/*
 * Method definitions.
 */

/*
 * Populates the cells of the spreadsheet. The cells are guaranteed
 * to all exist in the std::map "cells" after this method finishes.
 * Empty cells will be mapped to "".
 *
 * Modifications to the spreadsheet are guaranteed not to begin before
 * the cells have been populated.
 */
void spreadsheet::populate_cells()
{
   boost::lock_guard<boost::mutex> lock(this->mutex);

   for (int i = 1; i < 100; i++)
   {
      for (int j = static_cast<int>('A'); j < static_cast<int>('Z'); j++)
      {
	 std::string cell = "";
	 cell += static_cast<char>(j);
	 cell += i;

	 //Creates and sets to empty string if DNE,
	 //does nothing if exists.
	 this->cells[cell];
      }
   }
}

/*
 * Populates the cells of the spreadsheet. The cells are guaranteed
 * to all exist in the std::map cells after this method finishes.
 * Empty cells will be mapped to "".
 *
 * file should be a reference to an ifstream for which ifstream.open()
 * has been called.
 *
 * Modifications to the spreadsheet are guaranteed not to begin before
 * the cells have been populated.
 */
void spreadsheet::populate_cells(std::string &filename)
{
   boost::lock_guard<boost::mutex> lock(this->mutex);
   std::ifstream file(filename.c_str());

   if (file.is_open())
   {
      while (file.good())
      {
	 std::string line;
	 getline(file, line);
	 std::string vals[2];

	 //Get cell name and value in val[0] and val[1].
	 std::string cell = line.substr(0, 3);
	 if (static_cast<char>(cell.at(cell.size()-1)) == ' ')
	    vals[0] = cell.erase(cell.size()-1, std::string::npos);
	 try
	 {
	    vals[1] = line.substr(4);
	 }
	 catch (std::exception& e)
	 {
	    vals[1] = "";
	 }

	 this->cells[vals[0]] = vals[1];
      }
   }

}


/*
 * Attempts a modification of the specified cell using the value
 * Returns true and increments the version number if it 
 * succeeded, returns false otherwise.
 *
 * This method is threadsafe.
 */
bool spreadsheet::attempt_modify(std::string &cell, std::string &value, int vnum)
{
   boost::lock_guard<boost::mutex> lock(this->mutex);

   if (vnum != this->version)
      return false;

   undo_cmd cmd = { cell, this->cells[cell] };
   this->undo_stack.push(cmd);
   this->cells[cell] = value;
   this->increment_version();

   return true;
}

/*
 * Attempts an undo of the last successful modification to the
 * spreadsheet. Returns true and increments the version number
 * if it succeeded, returns false otherwise.
 *
 * The pointers cellname and cellvalue will contain the
 * new cell values after the undo has completed.
 *
 * This method is threadsafe.
 */
bool spreadsheet::attempt_undo(int vnum, std::string *cellname, 
			       std::string *cellvalue)
{

   boost::lock_guard<boost::mutex> lock(this->mutex);

   if (vnum != this->version)
      return false;

   undo_cmd cmd = this->undo_stack.top();

   this->cells[cmd.cell] = cmd.value;
   *cellname = cmd.cell;
   *cellvalue = cmd.value;
   this->increment_version();

   this->undo_stack.pop();

   return true;
}


/*
 * Increments the version of the spreadsheet.
 */
void spreadsheet::increment_version()
{
   this->version++;
}

/*
 * Saves the current state of the spreadsheet and clears
 * the undo stack, such that changes before the save
 * may no longer be undone.
 *
 * The spreadsheet is guaranteed not to be modified while
 * the save operation is in progress.
 */
void spreadsheet::save(std::string filename)
{
   boost::lock_guard<boost::mutex> lock(this->mutex);

   std::ofstream file;
   file.open(filename.c_str());

   if (file.is_open())
   {
      for (std::map<std::string, std::string>::iterator it = this->cells.begin();
	   it != this->cells.end() && file.good(); it++)
      {
	 std::string cellname = it->first;
	 if (cellname.size() < 3)
	    cellname += " ";

	 file << ":";
	 file << it->second;
	 file << "\n";
      }
   }

   while (!this->undo_stack.empty())
      this->undo_stack.pop();
   

}

/*
 * Converts the current spreadsheet to an xml string.
 * This will block any changes to the spreadsheet until
 * the completion of the to_xml() function call.
 */
std::string spreadsheet::to_xml()
{
   boost::lock_guard<boost::mutex> lock(this->mutex);

   std::string result = "<?xml version=""1.0"" encoding=""utf-8""?>";
   result += "<spreadsheet>";

   for (std::map<std::string, std::string>::iterator it = this->cells.begin();
	   it != this->cells.end(); it++)
   {
      std::string cellname = it->first;
      std::string contents = it->second;

      if (contents == "")
	 continue;

      result += "<cell>";
      result += "<name>";
      result += cellname;
      result += "</name>";
      result += "<contents>";
      result += contents;
      result += "</contents>";
      result += "</cell>";
   }

   result += "</spreadsheet>";

   return result;

}
