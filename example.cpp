#include <iostream>
#include <fstream>

main()
{
  std::string thingy;
  std::cout << "Enter a thingy" << std::endl;
  std::cin >> thingy;
  thingy += ".txt";
  const char* fileName = thingy.c_str();
  std::ofstream myfile (fileName);
  if(myfile.is_open())
    {
      myfile << "DAVID BOWIE\n";
      myfile << thingy + "\n";
      myfile << "<?xml version=\"1.0\" encoding=\"utf-8\"?><spreadsheet version=\"ps6\"></spreadsheet>";
      myfile.close();
    }
  std::cout << "MEOW MEOW MEOW MEOW MEOW" << std::endl;
}
