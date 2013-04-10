#include <iostream>
#include <boost/array.hpp>
#include <boost/asio.hpp>

using boost::asio::ip::tcp;
int main(int argc, char* argv[])
{
  try
    {
      boost::asio::io_service io_service;

      tcp::resolver resolver(io_service);
      tcp::resolver::query query(argv[1], "serv");
      tcp::resolver::iterator endpoint_iterator = resolver.resolve(query);


      tcp::socket socket(io_service);
      boost::asio::connect(socket, endpoint_iterator);




    }
  catch(std::exception& e)
    {
      std::cout << "SOMETHING HORRIBLE HAPPENED" << std::endl;
      std::cerr << e.what() << std::endl;

    }

}
