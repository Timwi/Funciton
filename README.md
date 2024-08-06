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

The source code for this project has [moved to Codeberg](https://codeberg.org/Timwi/Funciton).
