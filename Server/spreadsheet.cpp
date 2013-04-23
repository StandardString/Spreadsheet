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
#include <boost/lexical_cast.hpp>


/*
 * Constructs a spreadsheet with data in its cells
 * described by the file. If the file does not exist,
 * the spreadsheet will be empty.
 */
spreadsheet::spreadsheet(std::string &file)
{
   filename = file;
   populate_cells();
   version = 0;
}

spreadsheet::~spreadsheet()
{

}



/*
 * Populates the cells of the spreadsheet.
 *
 * filename denotes the file in which the spreadsheet resides.
 *
 * Modifications to the spreadsheet are guaranteed not to begin before
 * the cells have been populated.
 */
void spreadsheet::populate_cells()
{
   boost::lock_guard<boost::mutex> lock(this->mutex);
   std::ifstream file(filename.c_str());
   std::cout << "Loading from file..." << std::endl;
   if (file.is_open())
   {
      while (file.good())
      {
	 std::string line;
	 std::getline(file, line);
	 if (line.length() > 3)
	 {
	    std::string cellname;
	    std::string cellvalue;
	    std::string cell;

	    //Get cell name and value in val[0] and val[1].
	    try {
	       cell = line.substr(0, 3);
	    }
	    catch (std::out_of_range& e)
	    {
	       continue;
	    }
	    if (static_cast<char>(cell.at(cell.size()-1)) == ' ')
	       cellname = cell.erase(cell.size()-1, std::string::npos);
	    else
	    {
	       cellname = cell;
	    }
	    try
	    {
	       cellvalue = line.substr(4);
	    }
	    catch (std::out_of_range& e)
	    {
	       cellvalue = "";
	    }

	    this->cells[cellname] = cellvalue;
	    std::cout << "Cell: " << cellname 
		      << "\nValue: " << cellvalue << std::endl;
	 }
      }
   }
   std::cout << "Exiting populate_cells() " << std::endl;
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
int spreadsheet::attempt_undo(int vnum, std::string *cellname, 
			       std::string *cellvalue)
{

   boost::lock_guard<boost::mutex> lock(this->mutex);

   if (vnum != this->version)
      return 0;

   if (this->undo_stack.empty())
     {
       return -1;
     }
   undo_cmd cmd = this->undo_stack.top();

   this->cells[cmd.cell] = cmd.value;
   *cellname = cmd.cell;
   *cellvalue = cmd.value;
   this->increment_version();

   this->undo_stack.pop();

   return 1;
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
void spreadsheet::save()
{
   boost::lock_guard<boost::mutex> lock(this->mutex);

   std::ofstream file;
   file.open(filename.c_str());

   std::cout << "Saving file..." << std::endl;
   if (file.is_open())
   {
      for (std::map<std::string, std::string>::iterator it = this->cells.begin();
	   it != this->cells.end() && file.good(); it++)
      {
	 if (it->second != "")
	 {
	    std::string cellname = it->first;
	    if (cellname.size() < 3)
	       cellname += " ";

	    file << cellname;
	    file << ":";
	    file << it->second;
	    file << "\n";
	 
	    std::cout << "Cell: " << cellname << "\nValue: " 
		      << it->second << std::endl;
	 }
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

   std::string result = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
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

int spreadsheet::get_version()
{
  return this->version;
}
