#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <unistd.h>
#include <string.h>

// gcc source.c -fno-stack-protector -no-pie -o pwn

void ignore_me_kill_timer(int sig) {
    if(sig == SIGALRM)
        _exit(0);
}

void ignore_me() {
    setvbuf(stdout, NULL, _IONBF, 0);
    setvbuf(stderr, NULL, _IONBF, 0);
    setvbuf(stdin, NULL, _IONBF, 0);

    signal(SIGALRM, ignore_me_kill_timer);
    alarm(30);
}

int check_fo2( int fo2 ) {
    char name[100];
    if( fo2 == 0 ) {
        printf("WARNING: Diving without oxygen in mixture will lead to death!\n");
        printf("A minimum Oxygen percentage of 21%% is advised\n");
    } else if ( fo2 < 0 ) {
        printf("Negative number not possible. Please input your email for us to reach out to you:\n");
        scanf("%s", name);
    }

    return abs(fo2);
}

void print_flag() {
    char buf[100];
    FILE *flag = fopen("./flag.txt", "r");
    if( flag == NULL ) {
        printf("file does not exist\n");
        return;
    }
    fscanf(flag, "%99s", buf);
    printf("%s", buf);
}

void deep_dive() {
    printf("| ---------------------------------------- |\n");
    printf("| >>> S.C.U.B.A. Diving PO2 calculator <<< |\n");
    printf("| ---------------------------------------- |\n");
    printf("|          Partial Pressure of O2          |\n");
    printf("| ________________________________________ |\n\n");

    int depth, fo2;
    printf("Percentage of Oxygen in mix: ");
    scanf("%d", &fo2);
    fo2 = check_fo2(fo2);
    printf("Estimated depth to calculate: ");
    scanf("%d", &depth);
    float po2 = ((float) fo2 / 100.0) * ((float) abs(depth) / 10. + 1.0);
    printf("Calulated PO2: %.2f\n", po2);


}

void main() {
    #ifndef SKIPPER
    printf("!!!\nUsing a self compiled version may lead to incorrect offsets or the challenge not working at all!\n!!!\n");
    #endif

    ignore_me();
    deep_dive();
}
