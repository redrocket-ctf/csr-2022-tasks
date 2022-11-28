#!/usr/bin/env python3

import random
import string
from secret import FLAG

NUMBER_OF_GAMES = 100
THRESHOLD = 5
SIZE_OF_WORDLIST = 12000

rng = random.SystemRandom()


def game(secret_word, wordlist):
    print("[start game]")
    chars = set(secret_word)

    for guess_count in range(1, 7):
        guess = input("> ")[:5]
        assert(len(guess) == 5)
        assert(guess in wordlist)

        if guess == secret_word:
            print("[guess correct]> ")
            break

        result = ""
        for i in range(5):
            if guess[i] == secret_word[i]:
                result += "+"
            elif guess[i] in chars:
                result += "*"
            else:
                result += "-"

        print(result)
    else:
        print("[number of guesses exceeded]> ")

    return guess_count


def main():
    wordlist = set()
    while len(wordlist) < SIZE_OF_WORDLIST:
        word = ''.join(rng.choices(string.ascii_uppercase, weights=[7.58, 1.96, 3.16, 4.98, 18.93, 1.49, 3.02, 4.98, 8.02, 0.24, 1.32, 3.60, 2.55, 12.53, 2.24, 0.67, 0.02, 6.89, 8.42, 7.79, 3.83, 0.84, 1.78, 0.05, 0.05, 1.21], k=5))
        wordlist.add(word)

    print("Here is your personal wordlist:")
    print("[begin of wordlist]")
    for word in wordlist:
        print(word)
    print("[end of wordlist]")

    guesses = []
    for _ in range(NUMBER_OF_GAMES):
        secret_word = rng.choice(list(wordlist))
        guesses.append(game(secret_word, wordlist))

    average_guesses = sum(guesses) / NUMBER_OF_GAMES
    print(f'avg.: {average_guesses}')
    if average_guesses < THRESHOLD:
        print(FLAG)


if __name__ == '__main__':
    main()
