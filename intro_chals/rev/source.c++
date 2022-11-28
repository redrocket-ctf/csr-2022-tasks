#include <iostream>
#include <string>
//171136
void door_lock( int pass ) {
    if( (pass*2)>>8 == 1337) {
        std::cout << "CSR{" << pass%37 << "_submarines_" << ((pass<<2)*67-7) << "_solved_n1c3!}" << std::endl;
    }
}

void print_dives( std::string name ) {
    std::string admin = "Jeremy";
    std::string diver1 = "Simon";
    std::string diver2 = "Adminiman";

    if( name == diver1) {
        std::cout << "Your dive count is: 81\n";
    } else if (name == diver2)
    {
        std::cout << "Welcome instructor!\n";
        std::cout << "Your dive count is: 410\n";
    } else if (name == admin) {
        int pass;
        std::cout << "Your dive count is: 0\n";
        std::cout << "To show today's drydock report, please enter passcode:\n";
        std::cin >> pass;
        door_lock(pass);
    } else {
        std::cout << "No diving recore of diver " << name << " found!\n";
    }
    
}

int main( int argc, const char* argv[] ) {
    std::string input;

    std::cout << "| >>> REEF RANGERS Dive Panel <<< |" << std::endl;
    std::cout << "| ------------------------------- |" << std::endl;
    std::cout << "|    Please provide Diver Name:   |" << std::endl;
    std::cin >> input;
    print_dives(input);
}