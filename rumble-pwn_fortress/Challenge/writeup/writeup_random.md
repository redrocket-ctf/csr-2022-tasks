# pwn_fortress - WriteUp

## 1. Observing the game
Playing the game we notice that a save-state is created:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ xxd save.pwn
00000000: c35b 307c fdca ff3e 6b9f d760 a02b bf22  .[0|...>k..`.+."
00000010: c21c a222 8095 19d6 6fe2 6186 f199 f787  ..."....o.a.....
00000020: 6371 41b4 221e ec86 9cba 9100 70d4 8145  cqA.".......p..E
00000030: da8d 03ba ef66 5599 a8f0 b4a2 3504 61e6  .....fU.....5.a.
00000040: c7d6 33c6 4f76 23be c26f 62c3 962f dfda  ..3.Ov#..ob../..
00000050: c31c a321 8595 18b2 1ae2 6054 f076 f617  ...!......`T.v..
00000060: 62f7 4049 2361 c586 9dfa 9090 710f 80c0  b.@I#a......q...
00000070: db8d 02b7 e866 5489 a913 b532 3482 60e6  .....fT....24.`.
00000080: c242 319a fc5a feb8 6a62 d61f a22b bec2  .B1..Z..jb...+..
00000090: c389 a370 81e1 18d6 6eef 6068 f042 f602  ...p....n.`h.B..
000000a0: 62e9 40e1 201e ed6e 9d58 903c 71a0 8045  b.@. ..n.X.<q..E
```

However, we also observe that all contents of the file change when another save-state is created - even if we did not do anything in-game:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ diff -u <(xxd 1save.pwn) <(xxd 2save.pwn)
--- /dev/fd/63  2022-04-06 10:36:39.310547267 +0000
+++ /dev/fd/62  2022-04-06 10:36:39.310547267 +0000
@@ -1,11 +1,11 @@
-00000000: c35b 307c fdca ff3e 6b9f d760 a02b bf22  .[0|...>k..`.+."
-00000010: c21c a222 8095 19d6 6fe2 6186 f199 f787  ..."....o.a.....
-00000020: 6371 41b4 221e ec86 9cba 9100 70d4 8145  cqA.".......p..E
-00000030: da8d 03ba ef66 5599 a8f0 b4a2 3504 61e6  .....fU.....5.a.
-00000040: c7d6 33c6 4f76 23be c26f 62c3 962f dfda  ..3.Ov#..ob../..
-00000050: c31c a321 8595 18b2 1ae2 6054 f076 f617  ...!......`T.v..
-00000060: 62f7 4049 2361 c586 9dfa 9090 710f 80c0  b.@I#a......q...
-00000070: db8d 02b7 e866 5489 a913 b532 3482 60e6  .....fT....24.`.
-00000080: c242 319a fc5a feb8 6a62 d61f a22b bec2  .B1..Z..jb...+..
-00000090: c389 a370 81e1 18d6 6eef 6068 f042 f602  ...p....n.`h.B..
-000000a0: 62e9 40e1 201e ed6e 9d58 903c 71a0 8045  b.@. ..n.X.<q..E
+00000000: e5d7 3e52 8c28 9a00 74e8 5f35 0f58 ccff  ..>R.(..t._5.X..
+00000010: 6fe0 63d5 30e6 42fd f211 68fb 57cb b1d3  o.c.0.B...h.W...
+00000020: f17f c40b db61 6f45 d5ab 1372 2ccd 7053  .....aoE...r,.pS
+00000030: ea77 d210 5789 2e9a 6fda d43a 4d0c 1a15  .w..W...o..:M...
+00000040: d1a0 ec42 1b3e 98c2 1a32 8a0d 4154 d7f4  ...B.>...2..AT..
+00000050: 6ee0 62d6 35e6 4399 8711 6929 5624 b043  n.b.5.C...i)V$.C
+00000060: f0f9 c5f6 da1e 4645 d4eb 12e2 2d16 71d6  ......FE....-.q.
+00000070: eb77 d31d 5089 2f8a 6e39 d5aa 4c8a 1b15  .w..P./.n9..L...
+00000080: e4ce 3fb4 8db8 9b86 7515 5e4a 0d58 cd1f  ..?.....u.^J.X..
+00000090: 6e75 6287 3192 43fd f31c 6915 5610 b056  nub.1.C...i.V..V
+000000a0: f0e7 c55e d961 6ead d449 124e 2db9 7153  ...^.an..I.N-.qS
```

This data appears to be random, at least we can't make sense of this now.

## 2. Strings
Proceeding with static analysis, we run `strings` against the binary and find some promising output:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ strings pwn_fortress.out                                                                        
...
fopen
ftell
...
fseek
stdout
memcpy
fclose
malloc
fwrite
fread
...
/dev/urandom
...
[###############
...
save.pwn
...
```

From this we can draw the following conclusions:
* The game performs a bunch of file-operations on the save-state `save.pwn`
* The game utilizes `/dev/urandom`

## 3. Patching

Let's try and make the game use zero-bytes instead of random data and see what happens.
1. First we get the offset of the string `/dev/urandom`:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ strings -t d pwn_fortress.out | grep /dev/urandom
  24583 /dev/urandom
```
2. Then we overwrite it with `/dev/zero` (taking the NULL-terminator into account!)
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ printf "/dev/zero\x00" > zero.bin
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ xxd zero.bin
00000000: 2f64 6576 2f7a 6572 6f00                 /dev/zero.
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ cp pwn_fortress.out pwn_fortress_zero.out
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ dd if=zero.bin of=pwn_fortress_zero.out obs=1 seek=24583 conv=notrunc
0+1 records in
10+0 records out
10 bytes copied, 0.000112304 s, 89.0 kB/s
```
3. Validate the result:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ strings -t d pwn_fortress_zero.out | grep /dev/zero
  24583 /dev/zero
  ```
4. After running the modified binary we now see a very different save-state:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ ./pwn_fortress_zero.out

 Welcome to pwn_fortress! +---------------+
                          |               |
 Controls:  w - up        |   #           |
            a - left      |               |
            s - down      +------+ +------+
            d - right            |=|
                               [Start]
 Goal: Reach the flag! HF & GL
  - Zat
Time left: [################### ]
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ xxd save.pwn
00000000: 0000 0000 0000 0000 0000 0000 0000 0000  ................
00000010: 0000 0000 0000 0000 0000 0000 0000 0000  ................
00000020: 0000 0000 0000 0000 0000 0000 0000 0000  ................
00000030: 0000 0000 0000 0000 0000 0000 0000 0000  ................
00000040: de00 0000 4ba3 4f67 0100 0103 0300 011e  ....K.Og........
00000050: 0100 0103 0500 0164 7500 01d2 01ef 0190  .......du.......
00000060: 0186 01fd 017f 2900 0140 0190 01db 0185  ......)..@......
00000070: 0100 010d 0700 0110 01e3 0190 0186 0100  ................
00000080: 0119 01e6 0190 0186 01fd 017f 0200 01e0  ................
00000090: 0195 0152 0174 0100 010d 01ee 01db 0185  ...R.t..........
000000a0: 0198 0155 0200 01e8 01e2 013c 0174 0100  ...U.......<.t..
```

The first 64 bytes are all zeroes and the rest of the data doesn't look nearly as random as it did before!

## Reversing the save-state
Now we can use a variation of the memory scanning approach by creating a bunch of save-states and comparing them.
1. Run the game once, copy state (`save.pwn1`)
2. Run the game again and move right, copy state (`save.pwn2`)
3. Run the game again and move left, copy state (`save.pwn3`)
4. Compare the three files

```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ diff -u <(xxd save.pwn1) <(xxd save.pwn2)
--- /dev/fd/63  2022-04-06 11:44:50.693638672 +0000
+++ /dev/fd/62  2022-04-06 11:44:50.693638672 +0000
@@ -2,10 +2,11 @@
 00000010: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000020: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000030: 0000 0000 0000 0000 0000 0000 0000 0000  ................
-00000040: de00 0000 d386 d6b3 0100 0101 0300 011e  ................
+00000040: de00 0000 048c 5d47 0100 0102 0300 011f  ......]G........
 00000050: 0100 0103 0500 0164 7500 01c8 01ef 0153  .......du......S
 00000060: 01a0 01ff 017f 2900 0140 0160 01b0 01c6  ......)..@.`....
 00000070: 0100 010d 0700 0190 01c5 0153 01a0 0100  ...........S....
 00000080: 01a9 01c8 0153 01a0 01ff 017f 0200 01e0  .....S..........
 00000090: 01d5 0156 01b7 0100 010d 01be 01b0 01c6  ...V............
 000000a0: 01be 0155 0200 01e8 0122 0141 01b7 0100  ...U.....".A....
+000000b0: 0000

mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ diff -u <(xxd save.pwn2) <(xxd save.pwn3)
--- /dev/fd/63  2022-04-06 11:45:00.953502025 +0000
+++ /dev/fd/62  2022-04-06 11:45:00.953502025 +0000
@@ -2,7 +2,7 @@
 00000010: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000020: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000030: 0000 0000 0000 0000 0000 0000 0000 0000  ................
-00000040: de00 0000 048c 5d47 0100 0102 0300 011f  ......]G........
+00000040: de00 0000 c5ff 10cd 0100 0103 0300 011e  ................
 00000050: 0100 0103 0500 0164 7500 01c8 01ef 0153  .......du......S
 00000060: 01a0 01ff 017f 2900 0140 0160 01b0 01c6  ......)..@.`....
 00000070: 0100 010d 0700 0190 01c5 0153 01a0 0100  ...........S....

mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ diff -u <(xxd save.pwn1) <(xxd save.pwn3)
--- /dev/fd/63  2022-04-06 11:45:04.321457181 +0000
+++ /dev/fd/62  2022-04-06 11:45:04.321457181 +0000
@@ -2,10 +2,11 @@
 00000010: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000020: 0000 0000 0000 0000 0000 0000 0000 0000  ................
 00000030: 0000 0000 0000 0000 0000 0000 0000 0000  ................
-00000040: de00 0000 d386 d6b3 0100 0101 0300 011e  ................
+00000040: de00 0000 c5ff 10cd 0100 0103 0300 011e  ................
 00000050: 0100 0103 0500 0164 7500 01c8 01ef 0153  .......du......S
 00000060: 01a0 01ff 017f 2900 0140 0160 01b0 01c6  ......)..@.`....
 00000070: 0100 010d 0700 0190 01c5 0153 01a0 0100  ...........S....
 00000080: 01a9 01c8 0153 01a0 01ff 017f 0200 01e0  .....S..........
 00000090: 01d5 0156 01b7 0100 010d 01be 01b0 01c6  ...V............
 000000a0: 01be 0155 0200 01e8 0122 0141 01b7 0100  ...U.....".A....
+000000b0: 0000
```

We can observe a couple of things here:
1. There are 4 bytes at 44 that changed when we moved right (`save.pwn1` -> `save.pwn2`) but reverted to their original value when we moved back (`save.pwn2` -> `save.pwn3`)
2. There is a single byte at 4b that was incremented every time we ran the game
2. Another single byte at 4f was incremented when we moved right (`save.pwn1` -> `save.pwn2`) and decremented when we moved left (`save.pwn2` -> `save.pwn3`)

The byte at 4f directly corresponds to the action we took: it might just hold our X coordinate.

Let's try patching the value at 4f and examine the results:
```
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ printf "\x00" > zero1.bin
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ dd if=zero1.bin of=save.pwn seek=79 obs=1 conv=notrunc
0+1 records in
1+0 records out
1 byte copied, 0.00012292 s, 8.1 kB/s
mo@dev-ubuntu-server:~/projects/pwn_fortress/bin/x64/Release$ ./pwn_fortress_zero.out
[Corrupt save-file, using new]

 Welcome to pwn_fortress! +---------------+
                          |               |
 Controls:  w - up        |   #           |
            a - left      |               |
            s - down      +------+ +------+
            d - right            |=|
                               [Start]
 Goal: Reach the flag! HF & GL
  - Zat
Time left: [################### ]
```

The game says "corrupt save-file" and discards the save-state. This implies that the game evaluates save-states. To do this it can do some of the following:
1. Perform validation: We might try a "valid" value instead of setting the X coordinate to zero. This could be the value in `save.pwn1` at 4f. **Spoiler: that won't work.**
2. Signing: We might try and find & forge a signature in the save-state. **Spoiler: the game does not sign the save-state.**
3. Checksums: We might identify a checksum and re-calculate it. **Spoiler This is it!**

## Checksums
The four-byte value at 44 could be a simple CRC32 checksum. Let's try and validate that. But what data does it specifically check? Well, the save-state is structured as follows:
* 64 bytes of zeros (presumably encryption key)
* Some static 4 bytes that don't change
* 4 bytes of CRC32 checksum
* Variable number of bytes of potential gamestate

So let's extract all but the first 72 bytes of `save.pwn1` and calculate its CRC32:
```
```