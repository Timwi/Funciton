**[Funciton](http://esolangs.org/wiki/Funciton)** (pronounced: /ˈfʌŋkɪtɒn/) is a two-dimensional, declarative, functional, esoteric programming language.

### Usage

* `FuncitonInterpreter.exe sourcefile(s) [-t[name] [-t[name]] ...] [-s<string>|-i<integer>]`

    > Executes (interprets) a Funciton program.

* `FuncitonInterpreter.exe sourcefile(s) -c<file>`

    > Compiles a Funciton program to an exe.

* `FuncitonInterpreter.exe sourcefile(s) -a[name] [-a[name] ...]`

    > Displays a debug analysis.

### Options

* For interpreting Funciton programs:

    * `-t[name]`: Displays a debug trace of any number of functions and/or the main program during execution. For example, `-t+ -t× -t` traces the `+` and `×` functions and the main program.
    * `-s<string>`: Specifies a string to use as standard input (actual stdin is ignored). Cannot be used with `-i`.
    * `-i<integer>`: Makes the stdin box return this integer (even if it is not a valid string) (actual stdin is ignored). Cannot be used with `-s`.

* For compiling Funciton programs:

    * `-c<filename>`: Compiles the Funciton program to an exe and writes the output to the specified filename.

* Miscellaneous:

    * `-a[name]`: Displays analyses of any number of functions and/or the main program. For example, `-a+ -a× -a` analyzes the `+` and `×` functions and the main program. (The program is not executed.)

### Notes on the compiler

There is a known issue with compiling code that makes use of [lambda expressions](http://esolangs.org/wiki/Funciton#Lambda_expressions).
