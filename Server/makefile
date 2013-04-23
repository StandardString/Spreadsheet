all: server

server: session.cpp server.cpp spreadsheet.cpp
	g++ -o server server.cpp session.cpp spreadsheet.cpp -lboost_system -lboost_filesystem -pthread


clean:
	rm -rf *.ss
	rm -rf *.txt