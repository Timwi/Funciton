﻿
  ╔═════════════════════════════════════════════╗  ╔════════════╤═══════════════════════╗
  ║  Fibonacci sequence (whole integer range)   ║  ║  Lucas     │ Lucas(n) = Fibo(n−1)  ║
  ╟─────────────────────────────────────────────╢  ║  sequence  │          + Fibo(n+1)  ║
  ║  Fibo(n) = n < 0 ? Fibo(n+2) − Fibo(n+1) :  ║  ╚════════════╧═══════════════════════╝
  ║                    n < 2 ? n :              ║                ╓───────╖
  ║                    Fibo(n-2) + Fibo(n-1)    ║                ║ Lucas ║
  ╚═════════════════════════════════════════════╝                ╙───┬───╜
                                                                     │
                 ╓──────╖                                ╔═══╗ ┌───╖ │ ┌───╖  ╔═══╗
                 ║ Fibo ║                                ║ 1 ╟─┤ − ╟─┴─┤ + ╟──╢ 1 ║
                 ╙──┬───╜                                ╚═══╝ ╘═╤═╝   ╘═╤═╝  ╚═══╝
          ┌─────────┴─────────┐                             ┌────┴─╖   ┌─┴────╖
          │           ┌───────┴──────┐                      │ Fibo ║   │ Fibo ║
        ┌─┴─╖   ┌───╖ │ ┌───╖  ╔═══╗ │                      ╘════╤═╝   ╘═╤════╝
      ┌─┤ · ╟───┤ + ╟─┴─┤ + ╟──╢ 1 ║ │                           │ ┌───╖ │
      │ ╘═╤═╝   ╘═╤═╝   ╘═╤═╝  ╚═══╝ │                           └─┤ + ╟─┘
      │   │  ┌────┴─╖ ┌───┴──╖       │                             ╘═╤═╝
      │   │  │ Fibo ║ │ Fibo ║ ┌─────┴───┐                           │
      │   │  ╘════╤═╝ ╘═╤════╝ │      ┌──┴──┐
      │   └┐      │   ┌─┴─╖  ┌─┴─╖  ┌─┴─╖   │
    ╔═╧═╗  └┐     │   │ − ╟──┤ ? ╟──┤ ≤ ║   │
    ║ 2 ║   └┐    │   ╘═╤═╝  ╘═╤═╝  ╘═╤═╝   │
    ╚═╤═╝    └┐   └─────┘      │    ╔═╧═╗   │
      │ ┌───╖ │ ┌───╖  ╔════╗  │    ║ 0 ║   │
      └─┤ − ╟─┴─┤ + ╟──╢ -1 ║  │    ╚═══╝   │
        ╘═╤═╝   ╘═╤═╝  ╚════╝  │            │
     ┌────┴─╖ ┌───┴──╖         │            │
     │ Fibo ║ │ Fibo ║         │            │
     ╘════╤═╝ ╘═╤════╝         │            │
          │   ┌─┴─╖          ┌─┴─╖        ┌─┴─╖
          │   │ + ╟──────────┤ ? ╟────────┤ > ║
          │   ╘═╤═╝          ╘═╤═╝        ╘═╤═╝
          └─────┘              │          ╔═╧═╗
                                          ║ 2 ║
                                          ╚═══╝


