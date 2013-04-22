/*
 * Created by Owen Krafft, Austin Nester, Dickson Chiu, and Bryan Smith
 * for CS 3505, April, 2013.
 */

/* server.cpp
 * ~~~~~~~~~~
 * The server that receives and sends messages.  When the client sends a message to the server,
 * the server does the correct thing and sends a message back depending on if the command worked,
 * failed, or there is another message to send.
 *
 * Originally based off of the boost daytime tutorial Copyright (c) 2003-2008 Christopher M. Kohlhoff (chris at kohlhoff dot com)
 */

#include <iostream>
#include <string>
#include <boost/bind.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/asio.hpp>
#include <map>
#include <boost/algorithm/string/predicate.hpp>
#include <fstream>
#include "session.h"

using boost::asio::ip::tcp;

/**
 * tcp_connection class
 *~~~~~~~~~~~~~~~~~~~~~
 * Used as a wrapper for the client socket, any information that 
 * needs to be saved for each user is saved as a tcp_connection
 * and stored in the correct session.
 *
 * socket_ : A client socket stored within tcp_connection for use by the server.
 * 
 * 
 *
 **/
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

/*
 *tcp_server class
 *~~~~~~~~~~~~~~~~
 * This class does all of the work as far as redirecting and ressponding to messages
 * sent by the client.  Using the boost libraries, this was a simple
 * task to complete, but took quite a bit of research to master.
 *
 *
 * incString : Incoming message from the client
 * outString : Outgoing message to be sent to the client
 * acceptor_ : Accepts tcp sockets on this ip addresss and port.
 * b : A streambuffer to get what came from the client 
 * commandVector: A vector to store the different parts of the incoming message so it can be handled and the correct command will be executed.
 */
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
    : acceptor_(io_service, tcp::endpoint(tcp::v4(), 1992))
  {
    start_accept();
  }

 

private:
  //Accepts a client, sends to the handle_accept function, which will then come back here to accept new sockets after
  //the first client has been stored.
  void start_accept()
  {
    std::cout << "START" << std::endl;
    tcp_connection::pointer new_connection =
      tcp_connection::create(acceptor_.io_service());

    acceptor_.async_accept(new_connection->socket(),
			   boost::bind(&tcp_server::handle_accept, this, new_connection,
				       boost::asio::placeholders::error));
    
  }

  //Takes a socket and begins listening on it, putting it into it's own function handle_read.
  void handle_accept(tcp_connection::pointer new_connection,
		     const boost::system::error_code& error)
  {    
    if (!error)
      {    
      
	//START ASYNC READ ON THAT CONNECTION
	//Reads the socket until it encounters a new line character, at which point it goes to the 
	//handle_read function and sets up continued listening for that socket.
	boost::asio::async_read_until(new_connection->socket(),
				      b,
				      '\n',
				      boost::bind(&tcp_server::handle_read, this, new_connection,
						  boost::asio::placeholders::error));

	start_accept();//START ACCEPTING NEW CLIENTS
      }
    else if (error)
      throw boost::system::system_error(error);
  }

  //Takes in a new connection, and uses that socket to readconstantly. 
  void handle_read(tcp_connection::pointer new_connection, const boost::system::error_code& error)
  {
   
    std::istream is(&b);
    std::string line;
    std::getline(is, line);
    incomingMessages(line, new_connection);
    if(!error)
      {
	//Whenever the read runs into a new line character, the function starts reading again to accept any
	//new messsages from the client.
	boost::asio::async_read_until(new_connection->socket(),
				      b,
				      '\n',
				      boost::bind(&tcp_server::handle_read, this, new_connection,boost::asio::placeholders::error));
      }
  }


  //Handle write, just an empty call back function to be called when we use async_write
  void handle_write(const boost::system::error_code& error)
  {
  }

  //This function splits incoming messages from the client into their respective necessary actions.
  void incomingMessages(std::string msg, tcp_connection::pointer nc)
  {
    //Clears the command vector if we receive another command.
    if(boost::starts_with(msg, "CREATE") || boost::starts_with(msg, "JOIN") || boost::starts_with(msg, "CHANGE") || boost::starts_with(msg, "UNDO") || boost::starts_with(msg, "SAVE") || boost::starts_with(msg, "LEAVE"))
      {
	commandVector.clear();
      }
    //Pushes the messsage into the vector, to keep track of how long each message is.
    commandVector.push_back(msg);

    //If the message begins with create, splits the message to get the name and the password out.
    //It then uses the name and password to create a new spreadsheet on the server.
    if(boost::starts_with(commandVector.front(), "CREATE") && commandVector.size() == 3 )
      {
	bool pass  = true;
	//parse name
	std::string name = commandVector[1].substr(5);
	//parse password
	std::string password = commandVector[2].substr(9);
	//set up file
	std::string stringFileName = name + ".txt";
	const char* fileName = stringFileName.c_str();
    std::string emptyxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"ps6\"></spreadsheet>";
    std::ofstream myfile (fileName);
    if(myfile.is_open())
    {
        myfile << password + "\n";
        myfile << emptyxml + "\n";
        myfile.close();
    }
      outString = "CREATE OK\nName:" + name + "\nPassword:" + password + "\n";
      boost::asio::async_write(nc->socket(), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, this,
													   boost::asio::placeholders::error));
      // outString = "CREATE FAIL\nName:" + name + "\nSpreadsheet creation failed!\n";
       

  }

  //If the message begins with join, splits the message and gets the name and password.
  //If the name and password match, puts the user in that session, and sends the xml to display the spreadsheet.
  if(boost::starts_with(commandVector.front(), "JOIN") && commandVector.size() == 3)
    {
      //parse name
      std::string name = commandVector[1].substr(5);
      //parse password
      std::string password = commandVector[2].substr(9);
      std::string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"ps6\"></spreadsheet>";
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
    
  //If the message begins with change, takes out the name, version, cell, length of the xml? and the content of the cell.
  //This information is then used to change the spreadsheet, assuming the version is correct and the content doesn't cause any problems.
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
      std::string content = "";
      if(commandVector[5].length() != 0){
	content  = commandVector[5];}
      else{
	content = "null";}

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

  //If the message begins with undo, pulls out the name and version.
  //The undo is thern attempted and an appropriate message is returned to the client.
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

  //If the message begins with save, pulls out the name 
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
    
  //If the message begins with leave, disconnects the user from the session.
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
