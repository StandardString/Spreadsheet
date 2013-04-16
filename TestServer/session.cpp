

#include <iostream>
#include "session.h"
#include <boost/enable_shared_from_this.hpp>

/*
 * Constructs an empty session.
 */
session::session()
{
   session::users = new std::set<boost::asio::ip::tcp::socket*>();
}

/*
 * Deletes a session. Any users still connected can no longer interact
 * with this session. Deletion of user sockets relegated to other methods,
 * as users may still be connected to other sessions.
 */
session::~session()
{
   delete session::users;
}

/*
 * Adds a socket to this session.
 */
void session::add_socket(boost::asio::ip::tcp::socket *user)
{
   session::users->insert(user);
}

/*
 * Removes a socket from this session.
 */
void session::remove_socket(boost::asio::ip::tcp::socket *user)
{
   session::users->erase(user);
}

/*
 * Returns true if this session contains the socket.
 */
bool session::contains_socket(boost::asio::ip::tcp::socket *user)
{
   return (session::users->find(user) != session::users->end());
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
   for (std::set<boost::asio::ip::tcp::socket*>::iterator it = session::users->begin(); 
	it != session::users->end(); it++)
   {
      
      boost::asio::ip::tcp::socket &s = (*(*it));
      //Fill in sending a message to everyone. Fuck if I can figure this out. ~Owen.
      boost::asio::async_write(s, boost::asio::buffer(d, msg.size()), &handlesessionwrite);
   }
}
