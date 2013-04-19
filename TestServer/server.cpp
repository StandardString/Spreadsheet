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
    std::cout << line << std::endl;
    if(!error)
      {
	boost::asio::async_read_until(new_connection->socket(),
				    b,
				    '\n',
				      boost::bind(&tcp_server::handle_read, this, new_connection,boost::asio::placeholders::error));
      }
  }

  tcp::acceptor acceptor_;
  boost::asio::streambuf b;
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
