/*
 * Created by Owen Krafft, Austin Nester, Dickson Chiu, and Bryan Smith
 * for CS 3505, April, 2013.
 */
#ifndef SESSION_H
#define SESSION_H


#include <iostream>
#include <set>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/enable_shared_from_this.hpp>
#include <boost/asio.hpp>
#include "spreadsheet.h"

/*
 * Defines a spreadsheet session. Sessions consist of all users
 * currently editing a specific spreadsheet.
 *
 * Sessions are mapped by name in server.cpp
 */
typedef boost::shared_ptr<boost::asio::ip::tcp::socket> socket_ptr;
class session
{
public:
   session(std::string &filename);
   session();
   ~session();
   std::set<socket_ptr> *users;
   spreadsheet ss;
   
   void add_socket(socket_ptr user);
   void remove_socket(socket_ptr user);
   bool contains_socket(socket_ptr user);
   void broadcast(std::string &msg);
};

#endif
