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
#include <boost/filesystem.hpp>
#include <map>
#include <boost/lexical_cast.hpp>
#include <boost/algorithm/string/predicate.hpp>
#include <boost/thread.hpp>
#include <fstream>
#include <istream>
#include <ostream>
#include <signal.h>
#include <stdlib.h>
#include <stdio.h>
#include "session.h"
typedef boost::shared_ptr<boost::asio::ip::tcp::socket> socket_ptr;
typedef boost::shared_ptr<session::session> session_ptr;
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
   boost::mutex mutex;
   boost::asio::streambuf b;
   typedef boost::shared_ptr<tcp_connection> pointer;

   static pointer create(boost::asio::io_service& io_service)
   {
      return pointer(new tcp_connection(io_service));
   }

   socket_ptr socket()
   {
      return socket_;
   }

   void start()
   {
   }

private:
   tcp_connection(boost::asio::io_service& io_service)
      : socket_(new tcp::socket(io_service))
   {
   }

   socket_ptr socket_;
 
 
};

static std::map<std::string, session_ptr> sessions;
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


   std::string incString;
   tcp::acceptor acceptor_;

   std::vector<std::string> commandVector;
public:



   tcp_server(boost::asio::io_service& io_service)
      : acceptor_(io_service, tcp::endpoint(tcp::v4(), 1984))
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

      acceptor_.async_accept((*new_connection->socket()),
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
	 boost::asio::async_read_until((*new_connection->socket()),
				       new_connection->b,
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

      std::istream is(&(new_connection->b));
      std::string line;
      std::getline(is, line);
      if (line == "" && commandVector.size() != 5)
	 return;
      incomingMessages(line, new_connection);
      if(!error)
      {
	 //Whenever the read runs into a new line character, the function starts reading again to accept any
	 //new messsages from the client.
	 boost::asio::async_read_until((*new_connection->socket()),
				       new_connection->b,
				       '\n',
				       boost::bind(&tcp_server::handle_read, this, new_connection,boost::asio::placeholders::error));
      }
   }


   //Handle write, just an empty call back function to be called when we use async_write
   void handle_write(const boost::system::error_code& error)
   {
      std::cout << "Handling write " << std::endl;
   }

   //This function splits incoming messages from the client into their respective necessary actions.
   void incomingMessages(std::string msg, tcp_connection::pointer nc)
   {
      boost::lock_guard<boost::mutex> lock(nc->mutex);
      std::string outString;
      bool error_condition_met = false;
      //Pushes the messsage into the vector, to keep track of how long each message is.
      commandVector.push_back(msg);
      //If the message begins with create, splits the message to get the name and the password out.
      //It then uses the name and password to create a new spreadsheet on the server.
      if(boost::starts_with(commandVector.front(), "CREATE") && commandVector.size() == 3 )
      {
	
	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 std::string rname = commandVector[1].substr(5);

	 if (!boost::starts_with(commandVector[1], "Name:"))
	 {
	    error_condition_met = true;
	 }

	 if (boost::filesystem::exists(name))
	 {
	    outString = "CREATE FAIL\nName:" + rname + "\nSpreadsheet already exists!\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));					   						 
	    commandVector.clear();
	    return;
	 }
	 //parse password
	 std::string password = commandVector[2].substr(9);

	 if (!boost::starts_with(commandVector[2], "Password:"))
	 {
	    error_condition_met = true;
	 }
       

	 if (!error_condition_met)
	 {
	    //set up file
	    std::string stringFileName = name + ".txt";
	    const char* fileName = stringFileName.c_str();
	    std::ofstream myfile (fileName);
	    if(myfile.is_open())
	    {
	       myfile << password + "\n";
	       myfile.close();
	    }
	    outString = "CREATE OK\nName:" + rname + "\nPassword:" + password + "\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, this,  boost::asio::placeholders::error));					   						 
	    commandVector.clear();
	    return;
	 }

      }
	  

     

      //If the message begins with join, splits the message and gets the name and password.
      //If the name and password match, puts the user in that session, and sends the xml to display the spreadsheet.
      else if(boost::starts_with(commandVector.front(), "JOIN") && commandVector.size() == 3)
      {
	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 std::string rname = commandVector[1].substr(5);
	 std::string pname;


	 if (!boost::starts_with(commandVector[1], "Name:"))
	 {
	    error_condition_met = true;
	 }
      

	 pname += name;
	 pname += ".txt";
	 if (!boost::filesystem::exists(pname) && !error_condition_met)
	 {
	    outString =  "JOIN FAIL\nName:" + rname + "\nSession does not exist!\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
												  this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }
	 //parse password
	 std::string password = commandVector[2].substr(9);
	 std::string session_password;

	 if (!boost::starts_with(commandVector[2], "Password:"))
	 {
	    error_condition_met = true;
	 }

	 std::ifstream myfile((name + ".txt").c_str());
	 if (myfile.is_open() && !error_condition_met)
	 {
	    std::getline(myfile, session_password);
	 }

	 if (session_password != password && !error_condition_met)
	 {
	    outString =  "JOIN FAIL\nName:" + rname + "\nSession password was incorrect!\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
												  this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }
	
      
	 std::cout << "NAME: " << name << std::endl;
	 std::cout << "PASSWORD: " << password << std::endl;
      
      
  
	 //Create the session if it does not exist already
	 if (sessions.find(name) == sessions.end())
	 {
	    session_ptr ssptr(new session(name));
	    sessions.insert(std::pair<std::string, session_ptr>(name, ssptr));
	    std::cout << "Inserted session into map" << std::endl;
	 }

	 sessions[name]->add_socket(nc->socket());
	 std::cout << "Added socket to session. " << std::endl;


	 std::string xml = sessions[name]->ss->to_xml();
	 outString = "JOIN OK\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(sessions[name]->ss->get_version()) +  
	    "\nLength:" + boost::lexical_cast<std::string>(xml.length()) +  "\n" + xml + "\n";

	 std::cout << outString << std::endl;
	 boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
											       this,  boost::asio::placeholders::error));			   

	 commandVector.clear();
	 return;
      }
    
      //If the message begins with change, takes out the name, version, cell, length of the xml? and the content of the cell.
      //This information is then used to change the spreadsheet, assuming the version is correct and the content doesn't cause any problems.
      else if(boost::starts_with(commandVector.front(), "CHANGE") && commandVector.size() == 6)
      {
	
	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 std::string rname = commandVector[1].substr(5);
	 if (!boost::starts_with(commandVector[1], "Name:"))
	 {
	    error_condition_met = true;
	 }
	 if (sessions.find(name) == sessions.end() && !error_condition_met)
	 {
	    outString = "CHANGE FAIL\nName:" + rname + "\nSession does not exist.\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }

	 if (!sessions[name]->contains_socket(nc->socket()) && !error_condition_met)
	 {
	    outString = "CHANGE FAIL\nName:" + rname + "\nYou do not have permission to edit this session.\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }


	 //parse version
	 std::string version = commandVector[2].substr(8);
	 if (!boost::starts_with(commandVector[2], "Version:"))
	 {
	    error_condition_met = true;
	 }
	 //parse cell
	 std::string cell = commandVector[3].substr(5);
	 if (!boost::starts_with(commandVector[3], "Cell:"))
	    error_condition_met = true;
	 //parse length
	 std::string length = commandVector[4].substr(7);
	 if (!boost::starts_with(commandVector[4], "Length:"))
	    error_condition_met = true;
	 //parse content
	 std::string content = "";
	 if(commandVector[5].length() != 0){
	    content  = commandVector[5];}
	 else{
	    content = "";}

      
	 std::cout << "NAME: " << name << std::endl;
	 std::cout << "VERSION: " << version << std::endl;
	 std::cout << "CELL: " << cell << std::endl;
	 std::cout << "LENGTH: " << length << std::endl;
	 std::cout << "CONTENT: " << content << std::endl;

	 if(!error_condition_met)
	 {
	    if (sessions[name]->ss->attempt_modify(cell, content, std::atoi(version.c_str())))
	    {
	       outString = "CHANGE OK\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(sessions[name]->ss->get_version()) + "\n";
	       std::cout << outString << std::endl;
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
												     this,  boost::asio::placeholders::error));
	       std::string broadcast_string = "UPDATE\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(sessions[name]->ss->get_version()) + "\nCell:" + cell + "\nLength:" + boost::lexical_cast<std::string>(content.length()) + "\n" + content + "\n";
	       sessions[name]->broadcast(broadcast_string, nc->socket());
	       commandVector.clear();
	       return;
	    }
	    else
	    {
	       outString = "CHANGE FAIL\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(sessions[name]->ss->get_version()) + "\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));
	       commandVector.clear();
	       return;
	    }
	 }													 
	
      }

      //If the message begins with undo, pulls out the name and version.
      //The undo is thern attempted and an appropriate message is returned to the client.
      else if(boost::starts_with(commandVector.front(), "UNDO") && commandVector.size() == 3)
      {

	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 if (!boost::starts_with(commandVector[1], "Name:"))
	    error_condition_met = true;
      
	 std::string rname = commandVector[1].substr(5);
	 std::cout << "UNDO command" << std::endl;
	 if (sessions.find(name) == sessions.end())
	 {
	    outString = "UNDO FAIL\nName:" + rname + "\nSession does not exist.\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }

	 if (!sessions[name]->contains_socket(nc->socket()))
	 {
	    outString = "UNDO FAIL\nName:" + rname + "\nYou do not have permission to edit this session.\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }


	 std::string version = commandVector[2].substr(8);
	 if (!boost::starts_with(commandVector[2], "Version:"))
	    error_condition_met = true;
	 std::string *cellname = new std::string();
	 std::string *cellvalue = new std::string();
	 std::cout << "NAME: " << name << std::endl;
	 std::cout<< "VERSION: " << version << std::endl;
	 std::cout << "SERVER VERSION: " << sessions[name]->ss->get_version() << std::endl;
	 if(!error_condition_met)
	 {
	    if (sessions[name]->ss->attempt_undo(std::atoi(version.c_str()),cellname, cellvalue) == 1)
	    {
	       std::string cname = *cellname;
	       std::string cval = *cellvalue;
	       int out_version = sessions[name]->ss->get_version();
	       outString = "UNDO OK\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(out_version) + "\nCell:" + cname + "\nLength:" + boost::lexical_cast<std::string>(cval.length())+ "\n" + cval + "\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));


	       std::string broadcast_string = "UPDATE\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(out_version) + "\nCell:" + cname + "\nLength:" + boost::lexical_cast<std::string>(cval.length()) + "\n" + cval + "\n";
	       sessions[name]->broadcast(broadcast_string, nc->socket());
	       commandVector.clear();
	       return;
	    }
	    else if (sessions[name]->ss->attempt_undo(std::atoi(version.c_str()),cellname, cellvalue) == -1)
	    {
	       int out_version = sessions[name]->ss->get_version();
	       outString = "UNDO END\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(out_version) + "\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));
	       commandVector.clear();
	       return;

	    }
	    else
	    {
	       int out_version = sessions[name]->ss->get_version();
	       outString = "UNDO WAIT\nName:" + rname + "\nVersion:" + boost::lexical_cast<std::string>(out_version) + "\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));
	       commandVector.clear();
	       return;
	    }
	 }

	
	

	 delete cellname;
	 delete cellvalue;

	
      }

      //If the message begins with save, pulls out the name 
      else if(boost::starts_with(commandVector.front(), "SAVE") && commandVector.size() == 2)
      {

	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 if (!boost::starts_with(commandVector[1], "Name:"))
	    error_condition_met = true;
	 std::string rname = commandVector[1].substr(5);

	 //Fail conditions: session does not exist or user is not in session
	 if (!error_condition_met)
	 {
	    if (sessions.find(name) == sessions.end())
	    {
	       outString = "SAVE FAIL\nName:" + rname + "\nSession does not exist.\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));
	       commandVector.clear();
	       return;
	    }

	    if (!sessions[name]->contains_socket(nc->socket()))
	    {
	       outString = "SAVE FAIL\nName:" + rname + "\nYou do not have permission to edit this session.\n";
	       boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														       this,  boost::asio::placeholders::error));
	       commandVector.clear();
	       return;
	    }

	    //If we got here, everything is good. Save the spreadsheet.
	    std::cout << "NAME: " << name << std::endl;

	
	    sessions[name]->ss->save();
	    outString = "SAVE OK\nName:" + rname + "\n";
	    boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString, outString.size()), boost::bind(&tcp_server::handle_write, 
														    this,  boost::asio::placeholders::error));
	    commandVector.clear();
	    return;
	 }													 
      }
    
      //If the message begins with leave, disconnects the user from the session.
      else if(boost::starts_with(commandVector.front(), "LEAVE") && commandVector.size() == 2)
      {

	 //parse name
	 std::string name = commandVector[1].substr(5) + ".ss";
	 if (!boost::starts_with(commandVector[1], "Name:"))
	    error_condition_met = true;
	 std::cout << "NAME: " << name << std::endl;
	 if (!error_condition_met)
	 {
	    if (sessions.find(name) == sessions.end())
	    {
	       commandVector.clear();
	       return;
	    }

	    if (!sessions[name]->contains_socket(nc->socket()))
	    {
	       commandVector.clear();
	  
	       return;
	    }

      

	    sessions[name]->remove_socket(nc->socket());

	    if (sessions[name]->users->empty())
	    {
	       sessions[name]->ss->save();
	       sessions.erase(name);
	    }
      


	    commandVector.clear();
	    return;
	 }
      }

      //Error handling
      else if (commandVector.size() > 6 || (!boost::starts_with(commandVector.front(), "CHANGE") && 
					    !boost::starts_with(commandVector.front(), "UNDO") && 
					    !boost::starts_with(commandVector.front(), "LEAVE") &&
					    !boost::starts_with(commandVector.front(), "CREATE") &&
					    !boost::starts_with(commandVector.front(), "JOIN") &&
					    !boost::starts_with(commandVector.front(), "SAVE")))
      {
	 outString = "ERROR\n";
	 boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
											       this,  boost::asio::placeholders::error));
	 commandVector.clear();
	 return;

      }

      if (error_condition_met)
      {
	 outString = "ERROR\n";
	 boost::asio::async_write((*nc->socket()), boost::asio::buffer(outString), boost::bind(&tcp_server::handle_write, 
											       this,  boost::asio::placeholders::error));
	 commandVector.clear();
	 return;
      }
    
   }
 
};


/*
 * Handles CTRL-C event.
 */
void int_handler(int s)
{
   printf("\nServer was interrupted. Saving all active sessions.\n");
   for (std::map<std::string, session_ptr>::iterator it = sessions.begin(); it != sessions.end(); it++)
   {
      it->second->ss->save();
   }
   printf("Saves successful. Exiting.\n");
   exit(0);
}


int main()
{
   try
   {
      struct sigaction sigIntHandler;
      sigIntHandler.sa_handler = int_handler;
      sigemptyset(&sigIntHandler.sa_mask);
      sigIntHandler.sa_flags = 0;

      sigaction(SIGINT, &sigIntHandler, NULL);
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
