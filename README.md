**[Funciton](http://esolangs.org/wiki/Funciton)** (pronounced: /ˈfʌŋkɪtɒn/) is a two-dimensional, declarative, functional, esoteric programming language. Its syntax consists of Unicode box-drawing characters which combine to form boxes and lines. With the arbitrary-size integer as its only datatype, Funciton is Turing-complete (capable of all the same computations as any other programming paradigm) despite providing only 6 built-in syntax elements.

[![justforfunnoreally.dev badge](https://img.shields.io/badge/justforfunnoreally-dev-9ff)](https://justforfunnoreally.dev)

To get an idea of what the language looks like, here is the factorial function. However, note that each box used here is a call to another function that is itself written in Funciton.

```
                   ╓───╖
                   ║ ! ║
                   ╙─┬─╜   ┌───╖  ╔═══╗
               ┌─────┴─────┤ > ╟──╢ 2 ║
 ╔════╗  ┌───╖ │           ╘═╤═╝  ╚═══╝
 ║ −1 ╟──┤ + ╟─┴─┐           │
 ╚════╝  ╘═╤═╝   │           │
         ┌─┴─╖   │    ╔═══╗  │
         │ ! ║   │    ║ 1 ║  │
         ╘═╤═╝   │    ╚═╤═╝  │
           │   ┌─┴─╖  ┌─┴─╖  │
           │   │ × ╟──┤ ? ╟──┘
           │   ╘═╤═╝  ╘═╤═╝
           └─────┘      │
```

If you prefer to learn visually: [**Learn Funciton** video playlist (YouTube)](https://www.youtube.com/playlist?list=PLkG32PHxWoJaetjKUMVRONWLgRHQVjmtc)

If you prefer to read: [**Funciton** on the esolangs wiki](https://esolangs.org/wiki/Funciton)

## Usage

* `Funciton.exe sourcefile(s) [-t[name] [-t[name]] ...] [-s<string>|-i<integer>]`

    * Executes (interprets) a Funciton program.

* `Funciton.exe sourcefile(s) -c<file>`

    * Compiles a Funciton program to an exe.

* `Funciton.exe sourcefile(s) -a[name] [-a[name] ...]`

    * Displays a debug analysis.

## Options

* **Interpreting Funciton programs:**

    * `-t[name]`
        * Displays a debug trace of any number of functions and/or the main program during execution. For example, `-t+ -t× -t` traces the `+` and `×` functions and the main program.
    * `-s<string>`
        * Specifies a string to use as standard input (actual stdin is ignored). Cannot be used with `-i`.
    * `-i<integer>`
        * Makes the stdin box return this integer (even if it is not a valid string) (actual stdin is ignored). Cannot be used with `-s`.

* **Compiling Funciton programs:**

    * `-c<filename>`
        * Compiles the Funciton program to an exe and writes the output to the specified filename.

* **Miscellaneous:**

    * `-a[name]`
        * Displays analyses of any number of functions and/or the main program. For example, `-a+ -a× -a` analyzes the `+` and `×` functions and the main program. (The program is not executed.)

## Notes on the compiler

There is a known issue with compiling code that makes use of [lambda expressions](http://esolangs.org/wiki/Funciton#Lambda_expressions).
