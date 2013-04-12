

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

/*
 * Broadcasts the supplied message to every user in the session.
 */
void session::broadcast(std::string &msg)
{
   for (std::set<boost::asio::ip::tcp::socket*>::iterator it = session::users->begin(); 
	it != session::users->end(); it++)
   {

      //Fill in sending a message to everyone. Fuck if I can figure this out. ~Owen.
   }
}
