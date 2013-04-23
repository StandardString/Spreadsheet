/*
 * Created by Owen Krafft, Austin Nester, Dickson Chiu, and Bryan Smith
 * for CS 3505, April, 2013.
 */
#include <iostream>
#include <fstream>
#include "spreadsheet.h"
#include "session.h"
#include <boost/asio.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>

typedef boost::shared_ptr<boost::asio::ip::tcp::socket> socket_ptr;

/*
 * Constructs an empty session. The underlying spreadsheet
 * is initialized with empty cells.
 */
session::session() : ss(new spreadsheet())
{
   session::users = new std::set<socket_ptr>();
   ss = new spreadsheet();
}


/*
 * Constructs an empty session from the saved spreadsheet file
 * denoted by filename.
 */
session::session(std::string &filename) : ss(new spreadsheet(filename))
{
   session::users = new std::set<socket_ptr>();
}

/*
 * Deletes a session. Any users still connected can no longer interact
 * with this session. Deletion of user sockets relegated to other methods,
 * as users may still be connected to other sessions.
 */
session::~session()
{
   delete session::users;
   delete ss;
}

/*
 * Adds a socket to this session.
 */
void session::add_socket(socket_ptr user)
{
   session::users->insert(user);
}

/*
 * Removes a socket from this session.
 */
void session::remove_socket(socket_ptr user)
{
   session::users->erase(user);
}

/*
 * Returns true if this session contains the socket.
 */
bool session::contains_socket(socket_ptr user)
{
  for (std::set<socket_ptr>::iterator it = users->begin();
       it != users->end(); it++)
    {
       if ((*it) == user)
	  return true;
    }
  
  return false;
}

void handlesessionwrite(const boost::system::error_code &err, size_t size)
{

}

/*
 * Broadcasts the supplied message to every user in the session.
 */
void session::broadcast(std::string &msg)
{

   char *data = new char[msg.size()];
   for (int i = 0; i < msg.size(); i++)
   {
      data[i] = msg[i];

   }

   void *d = data;
   for (std::set<socket_ptr>::iterator it = session::users->begin(); 
	it != session::users->end(); it++)
   {
      
      socket_ptr s = *it;
      boost::asio::async_write(*s, boost::asio::buffer(d, msg.size()), &handlesessionwrite);
   }
}
