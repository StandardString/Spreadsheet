// server.cpp
// ~~~~~~~~~~
//
// Copyright (c) 2003-2008 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#include <iostream>
#include <string>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/asio.hpp>
#include <map>
#include <boost/algorithm/string/predicate.hpp>
#include "session.h"

using boost::asio::ip::tcp;

class tcp_connection
{
public:
  typedef boost::shared_ptr<tcp_connection> pointer;

  static pointer create(boost::asio::io_service& io_service)
  {
    return pointer(new tcp_connection(io_service));
  }

  tcp::socket& socket()
  {
    return socket_;
  }

  void start()
  {
  }

private:
  tcp_connection(boost::asio::io_service& io_service)
    : socket_(io_service)
  {
  }

  tcp::socket socket_;
 
};

class tcp_server
{

  //std::map<std::string, session> sessions;
  std::string incString;
  std::string outString;
  tcp::acceptor acceptor_;
  boost::asio::streambuf b;
  std::vector<std::string> commandVector;
public:
  tcp_server(boost::asio::io_service& io_service)
    : acceptor_(io_service, tcp::endpoint(tcp::v4(), 1984))
  {

    start_accept();
  }

 

private:
  void start_accept()
  {
    std::cout << "START" << std::endl;
    tcp_connection::pointer new_connection =
      tcp_connection::create(acceptor_.io_service());

    acceptor_.async_accept(new_connection->socket(),
        boost::bind(&tcp_server::handle_accept, this, new_connection,
          boost::asio::placeholders::error));
    
  }

  void handle_accept(tcp_connection::pointer new_connection,
      const boost::system::error_code& error)
  {    
    if (!error)
    {    
      
      //START ASYNC READ ON THAT CONNECTION
      boost::asio::async_read_until(new_connection->socket(),
				    b,
				    '\n',
				    boost::bind(&tcp_server::handle_read, this, new_connection,
				    boost::asio::placeholders::error));

      start_accept();//START ACCEPTING NEW CLIENTS
    }
    else if (error){
      std::cerr << "in error"  << std::endl;
      throw boost::system::system_error(error);
    }
  }

  void handle_read(tcp_connection::pointer new_connection, const boost::system::error_code& error)
  {
   
    std::istream is(&b);
    std::string line;
    std::getline(is, line);
    incomingMessages(line, new_connection);
    if(!error)
      {
	boost::asio::async_read_until(new_connection->socket(),
				    b,
				    '\n',
				      boost::bind(&tcp_server::handle_read, this, new_connection,boost::asio::placeholders::error));
      }
  }



  void handle_write(const boost::system::error_code& error)
  {

  }
  void incomingMessages(std::string msg, tcp_connection::pointer nc)
  {
    if(boost::starts_with(msg, "CREATE") || boost::starts_with(msg, "JOIN") || boost::starts_with(msg, "CHANGE") || boost::starts_with(msg, "UNDO") || boost::starts_with(msg, "SAVE") || boost::starts_with(msg, "LEAVE"))
      {
	commandVector.clear();
      }

    commandVector.push_back(msg);

    if(boost::starts_with(commandVector.front(), "CREATE") && commandVector.size() == 3 )
    {
      bool pass  = true;
      //parse name
      std::string name = commandVector[1].substr(5);
      //parse password
      std::string password = commandVector[2].substr(9);

      std::cout << "NAME: " << name << std::endl;
      std::cout << "PASSWORD: " << password << std::endl;
      if(pass){
	outString = "CREATE OK\nName:" + name + "\nPassword:" + password + "\n";
	boost::asio::async_write(nc->socket(), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, this,
            boost::asio::placeholders::error));
      }
      
      else
	outString = "CREATE FAIL\nName:" + name + "\nSpreadsheet creation failed!\n";
       

    }

    if(boost::starts_with(commandVector.front(), "JOIN") && commandVector.size() == 3)
      {
	//parse name
	std::string name = commandVector[1].substr(5);
	//parse password
	std::string password = commandVector[2].substr(9);
	std::string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"ps6\"><cell><name>B1</name><contents>1</contents></cell><cell><name>C1</name><contents>=A1+B1</contents></cell><cell><name>A1</name><contents>1</contents></cell></spreadsheet>";
	bool pass = true;
	std::cout << "NAME: " << name << std::endl;
	std::cout << "PASSWORD: " << password << std::endl;
	if(pass)
	  {
	  outString = "JOIN OK\nName:" + name + "\nVersion:5\nLength:5\n" + xml + "\n";
	  std::cout << outString << std::endl;
	 boost::asio::async_write(nc->socket(), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, this,
            boost::asio::placeholders::error));
	  //sessions[name] = new session();
	  // sessions[name].add_socket(nc->socket());
	  // sessions[name].broadcast(outString);
	  }
	else
	  outString =  "JOIN FAIL\nName:" + name + "\nJoin failed!\n";
      }

    if(boost::starts_with(commandVector.front(), "CHANGE") && commandVector.size() == 6)
      {
	//parse name
	std::string name = commandVector[1].substr(5, commandVector[1].length());
	//parse version
	std::string version = commandVector[2].substr(8, commandVector[2].length());
	//parse cell
	std::string cell = commandVector[3].substr(5, commandVector[3].length());
	//parse length
	std::string length = commandVector[4].substr(7, commandVector[4].length());
	//parse content
	std::string content = commandVector[5].substr(7, commandVector[5].length());

	bool pass = false;
	std::cout << "NAME: " << name << std::endl;
	std::cout << "VERSION: " << version << std::endl;
	std::cout << "CELL: " << cell << std::endl;
	std::cout << "LENGTH: " << length << std::endl;
	std::cout << "CONTENT: " << content << std::endl;
	if(pass)
	  outString = "CHANGE OK\nName:" + name + "\nVersion:5\n";
	else
	  outString = "CHANGE FAIL\nName:" + name + "\nVersion:5\n";

	
      }

    if(boost::starts_with(commandVector.front(), "UNDO") && commandVector.size() == 3)
      {
	//parse name
	std::string name = commandVector[1].substr(5, commandVector[1].length());
	//parse password
	std::string password = commandVector[2].substr(9, commandVector[2].length());
	bool pass = false;
	std::cout << "NAME: " << name << std::endl;
	std::cout << "PASSWORD: " << password << std::endl;

	if(pass){
	  outString = "UNDO OK\nName:" + name + "\nVersion:5\n";

	  outString = "UNDO END\nName:" + name + "\nVersion:5\n";

	  outString = "UNDO WAIT\nName:" + name + "\nVersion:5\n";
	}
	 else
	   outString = "UNDO FAIL\nName:" + name + "\nUNDO FAILED!\n";
      }

    if(boost::starts_with(commandVector.front(), "SAVE") && commandVector.size() == 2)
      {
	//parse name
	std::string name = commandVector[1].substr(5, commandVector[1].length());
	bool pass = false;
	std::cout << "NAME: " << name << std::endl;
	if(pass)
	  outString = "SAVE OK\nName:" + name + "\n";
	else
	  outString = "SAVE FAIL\nName:" + name + "\nSave failed!\n";
      }
    
    if(boost::starts_with(commandVector.front(), "LEAVE") && commandVector.size() == 2)
      {
	//parse name
	std::string name = commandVector[1].substr(5, commandVector[1].length());
	
	std::cout << "NAME: " << name << std::endl;
      }
  }
 
};





int main()
{
  try
  {
    boost::asio::io_service io_service;
    tcp_server server(io_service);
    io_service.run();
  }
  catch (std::exception& e)
  {
    std::cerr << e.what() << std::endl;
  }

  return 0;
}
