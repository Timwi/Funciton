﻿
                 ╔╤════════════╗             ╔╤══════════════════════╗
                 ║│  division  ║             ║│  remainder (modulo)  ║
                 ╚╧════════════╝             ╚╧══════════════════════╝
 
            ┌───────────┐                   ┌───────────┐
            │  ╓───╖  ┌─┴──╖     ╔═══╗      │  ╓───╖  ┌─┴──╖
            └──╢ ÷ ╟──┤ ÷% ╟──┬──╢ 0 ║      └──╢ % ╟──┤ ÷% ╟─────────┐
               ╙───╜  ╘═╤══╝  │  ╚═══╝         ╙───╜  ╘═╤══╝  ╔═══╗ ┌┴┐
                        └──┬──┘                         └──┬──╢ 0 ║ └┬┘
                          ┌┴┐                              │  ╚═══╝  │
                          └┬┘                              └────┬────┘
                           │                                    │
 
   ┌──────────────────────────────────┐                    ╔═════════════════════════════════════╗
   │                   ╓────╖       ┌─┴─╖                  ║  division and modulo                ║
   ├───────────────────╢ ÷% ║       │ | ║                  ╟─────────────────────────────────────╢
   │                   ╙──┬─╜       ╘═╤═╝                  ║  ÷%(a, b) =                         ║
   │  ┌───╖  ╔═══╗  ┌───╖ │ ┌───╖  ┌──┴──╖     ┌────┐      ║      let (q, r) = ÷%p(|a|, |b|);    ║
   └──┤ > ╟──╢ 0 ╟──┤ < ╟─┴─┤ | ╟──┤ ÷%p ╟─────┤    │      ║      a<0 ^ b<0 ? (~q, ~r) : (q, r)  ║
      ╘═╤═╝  ╚═══╝  ╘═╤═╝   ╘═══╝  ╘══╤══╝   ┌─┴─╖  │      ╚═════════════════════════════════════╝
     ┌──┴─────────┬───┴──┐          ┌─┴──┐   │ ~ ║  │           ╔═══════════════════════════╗
    ┌┴┐           │     ┌┴┐  ─┐   ┌─┴─╖  │   ╘═╤═╝  │           ║        a                  ║
    └┬┘         ┌─┴─╖   └┬┘   │   │ ~ ║  │   ┌─┴─╖  │           ║        ↓                  ║
     └────┬─────┤ · ╟────┘  ┌─┴─╖ ╘═╤═╝  │ ┌─┤ ? ╟──┘           ║      ┌─┴──╖     a ÷ b     ║
          │     ╘═╤═╝    ┌──┤ ? ╟───┘    │ │ ╘═╤═╝              ║  b → ┤ ÷% ╟ → (Quotient)  ║
          └───┬───┘      │  ╘═╤═╝        │     │                ║      ╘═╤══╝               ║
              └──────────┤    └──────────┘     │                ║        ↓                  ║
                         └─────────────────────┘                ║      a % b                ║
                                                                ║   (Remainder)             ║
                           ╔═══╗  ┌────╖                        ╚═══════════════════════════╝
                           ║ 1 ╟──┤ >> ╟────┐
                           ╚═══╝  ╘═╤══╝    │
                                 ┌──┴──╖  ┌─┴─╖
                ╓┬────╖      ┌───┤ ÷%p ╟──┤ · ╟──┐
                ╟┘÷%p ╟──────┤   ╘══╤══╝  ╘═╤═╝  │
                ╙──┬──╜    ┌─┴─╖  ┌─┴─╖     │    │
 ┌─────────────────┴───────┤ · ╟──┤ · ╟─────┘    │
 │                         ╘═╤═╝  ╘═╤═╝          │
 │ ╔═══╗           ┌────╖  ┌─┴─╖  ┌─┴─╖          │
 │ ║ 1 ╟───────────┤ << ╟──┤ · ╟──┤ · ╟──────────┘         ╔══════════════════════════════════════════╗
 │ ╚═╤═╝ ┌┐ ┌───╖  ╘═╤══╝  ╘═╤═╝  ╘═╤═╝                    ║  division and modulo (a ≥ 0 & b > 0)     ║
 │   ├───┤├─┤ + ╟────┘       │      └───────────────────┐  ╟──────────────────────────────────────────╢
 │   │   └┘ ╘═╤═╝          ┌─┴─╖                        │  ║  ÷%(a, b) = let (q, r) = ÷%(a >> 1, b);  ║
 │   │     ┌──┴────────────┤ · ╟───────┐ ╔═══╗  ┌────╖  │  ║             let i = (r << 1) + (a & 1);  ║
 │   │     │   ┌───╖  ┌───╖╘═╤═╝┌───╖  │ ║ 1 ╟──┤ << ╟──┘  ║             let j = i ≥ b;               ║
 │   │  ┌──┴───┤ + ╟──┤ ~ ╟──┴──┤ ≥ ╟──┘ ╚═══╝  ╘═╤══╝     ║             (a ? (q << 1)-j : 0,         ║
 └───┤  │      ╘═╤═╝  ╘═══╝     ╘═╤═╝             │        ║              a ? (j ? i-b : i) : 0)      ║
     │  │      ┌─┴─╖              │    ┌───╖      │        ╟──────────────────────────────────────────╢
     │  └──────┤ ? ╟──────────────┴────┤ − ╟──────┘        ║  (doesn’t crash for b = 0, but gives     ║
     │         ╘═╤═╝                   ╘═╤═╝               ║  interesting/nonsensical results)        ║
     │  ╔═══╗  ┌─┴─╖            ╔═══╗  ┌─┴─╖               ╚══════════════════════════════════════════╝
     │  ║ 0 ╟──┤ ? ╟──────┐     ║ 0 ╟──┤ ? ╟──┐
     │  ╚═══╝  ╘═╤═╝      │     ╚═══╝  ╘═╤═╝  │
     │           └─       │              │    │
     └────────────────────┤                   │
                          └───────────────────┘
 
 
 
