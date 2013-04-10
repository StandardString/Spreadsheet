all: both

server:
	g++ -o server server.cpp -lboost_system -pthread
client:
	g++ -o client client.cpp -lboost_system -pthread
both:
	g++ -o server server.cpp -lboost_system -pthread
	g++ -o client client.cpp -lboost_system -pthread

clean: 
	rm -f *.o client server