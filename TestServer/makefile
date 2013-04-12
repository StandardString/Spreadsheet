
all: session.o server.o

server.o: server.cpp
	g++ server.cpp -lboost_system -pthread

session.o: session.cpp
	g++ session.cpp -lboost_system -pthread