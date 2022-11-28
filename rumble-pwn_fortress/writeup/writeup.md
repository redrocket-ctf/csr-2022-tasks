# pwn_fortress - Write Up

## Strings
Searching for strings in the binary, we find `save.pwn` - the name of the save-file:
![](/writeup/01_strings.png)

This string is only used in one function that looks a lot like a main-function:

```c
undefined8 FUN_00101230(int argc,char **argv)
{
  //...
  undefined game_state [8];
  long local_140;
  uint local_30;
  long local_20;
  //...
  res = access(s_save.pwn_00109020,0); //Save-file exists?
  if (res == 0) {
    res = load_game(s_save.pwn_00109020,game_state); //Attempt to load game
    if (res == 0) {   
      //...
      reset_game(game_state);
    }
  }
  else {
    //...
    reset_game(game_state);
  }
  if (('\0' < *(char *)(local_140 + 8)) && (local_30 < 0x100)) { // (1)
    if (1 < argc) { //Process process args
      switch(*argv[1]) {
      case 'a':
        move_player(game_state,&DAT_00109014);
        break;
      case 'd':
        move_player(game_state,&DAT_0010901c);
        break;
      case 's':
        move_player(game_state,&DAT_00109018);
        break;
      case 'w':
        move_player(game_state,&DAT_00109010);
        break;
      case 'x':
        goto switchD_001013c9_caseD_78;
      }
    }
    FUN_00103ff0(game_state);
    save_game(s_save.pwn_00109020,game_state);
    if (*(char *)(local_140 + 8) < '\x01') { // (2)
      remove_save(s_save.pwn_00109020);
      //...
    }
    if (255 < local_30) { // (3)
      remove_save(s_save.pwn_00109020);
      //...
    }
    if (str_buff != '\0') {
      __printf_chk(1,&str_buff);
    }
    //...
  }
  //...
}
```

Reasoning:
* The `load_game` method indirectly contains a bunch of file operations (fopen, fread) to read data from `save.pwn`. Hence the name.
* `load_game` takes two arguments: a filename and a pointer to where data is written. Since it reads data from `save.pwn`, the pointer points to our gamedata.
* The `switch` processes the direction-input by calling the same method (`move_player`) with different arguments for the individual directions. The `DAT_001090XX` references contain pairs of shorts that hold 1 or 0xFFFF (-1).
* `remove_save` overwrites the save-state with zeros.

From playing the game we know that the save-state is removed when:
* the player dies
* the "time" ran out

Knowing this we can take a closer look at (2) and (3):
* (2) evaluates whether `*(char*)(local_140 + 8)<= 0`
* (3) evaluates whether `local_30 > 255`
* (1) is a combination of both of the above

However we find that neither `local_140` nor `local_30` are ever used in the method. This must mean that Ghidra's decompilation and type-detection didn't work quite right and that `game_state` is not 8 but 