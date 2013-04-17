/*
 * Created by Owen Krafft, Austin Nester, Dickson Chiu, and Bryan Smith
 * for CS 3505, April, 2013.
 */

#ifndef SPREADSHEET_H
#define SPREADSHEET_H

#include <iostream>
#include <map>
#include <stack>
#include <fstream>
#include <boost/thread.hpp>

/*
 * Defines a spreadsheet which tracks
 * data held in cells. This is a server
 * backend class and thus it does not do
 * cell verification.
 *
 * Spreadsheets can receive requests to modify
 * their contents. These requests must contain
 * a version number matching the internal version
 * number of the spreadsheet. If they do not match,
 * the spreadsheet will not update.
 */



class spreadsheet
{
  public:
   struct undo_cmd { std::string cell; std::string value; };
  private:
   std::map< std::string, std::string > cells;
   std::stack< undo_cmd > undo_stack;
   int version;
   boost::mutex mutex;
   void populate_cells(std::ifstream &file);
   void populate_cells();

  public:
   spreadsheet();
   spreadsheet(std::ifstream &file);
   ~spreadsheet();

   bool attempt_modify(std::string &cell, std::string &value, int vnum);
   bool attempt_undo(int vnum);
   void increment_version();

   void save(std::string filename);

};

#endif
