//
// client.cpp
// ~~~~~~~~~~
//DICKSON IS A HARLOT.
// Copyright (c) 2003-2008 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#include <iostream>
#include <boost/array.hpp>
#include <boost/asio.hpp>

using boost::asio::ip::tcp;

int main(int argc, char* argv[])
{
  try
  {
    if (argc != 2)
    {
      std::cerr << "Usage: client <host>" << std::endl;
      return 1;
    }

    boost::asio::io_service io_service;
    
    tcp::socket socket(io_service);

    socket.close();
  
    socket.connect(tcp::endpoint(tcp::v4(), 1984));//(tcp::endpoint(boost::asio::ip::address::from_string("155.98.111.56"),1984));

    boost::array<char, 128> buf;

    size_t len = socket.read_some(boost::asio::buffer(buf));

     std::cout.write(buf.data(), len);
  
  }
  catch (std::exception& e)
  {
    std::cerr << e.what() << std::endl;
  }

  return 0;
}
