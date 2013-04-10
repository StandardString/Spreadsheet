#include <iostream>
#include <boost/array.hpp>
#include <boost/asio.hpp>

using boost::asio::ip::tcp;

int main()
{
  try
    {
      boost::asio::io_service io_service;

      tcp::acceptor acceptor(io_service, tcp::endpoint(tcp::v4(), 3333));
      
      tcp::socket socket(io_service);
      acceptor.accept(socket);
      

      boost::asio::write(socket, boost::asio::buffer("MESSAGE"));

    }
  catch(std::exception& e)
    {
      std::cerr << e.what() << std::endl;
    }
  return 0;

}
