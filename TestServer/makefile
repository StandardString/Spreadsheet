all: session.o

session.o: session.cpp spreadsheet.cpp
	g++ session.cpp spreadsheet.cpp -lboost_system -pthread
