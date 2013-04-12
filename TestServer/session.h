
#ifndef SESSION_H
#define SESSION_H

#include <iostream>
#include <set>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>

/*
 * Defines a spreadsheet session. Sessions consist of all users
 * currently editing a specific spreadsheet.
 *
 * Sessions are mapped by name in server.cpp
 */
class session
{
public:
   session();
   ~session();
   std::set<boost::asio::ip::tcp::socket*> *users;
   
   void add_socket(boost::asio::ip::tcp::socket *user);
   void remove_socket(boost::asio::ip::tcp::socket *user);
   bool contains_socket(boost::asio::ip::tcp::socket *user);
   void broadcast(std::string &msg);

   void handle_write(const boost::system::error_code& /*error*/,
      size_t /*bytes_transferred*/)
  {
  }
};

#endif
