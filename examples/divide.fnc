﻿                           
      ╔╤════════════╗            ╔╤══════════════════════╗       ╔╤══════════════════════╗
      ║│  division  ║            ║│  remainder (modulo)  ║       ║│  remainder (modulo)  ║
      ╚╧════════════╝            ║│  (negative for a<0)  ║       ║│  (never negative)    ║
                                 ╚╧══════════════════════╝       ╚╧══════════════════════╝
 ┌───────────┐                  ┌───────────┐                   ┌──────────────────┐
 │  ╓───╖  ┌─┴──╖     ╔═══╗     │  ╓───╖  ┌─┴──╖                │  ┌───╖  ╓─────╖  │
 └──╢ ÷ ╟──┤ ÷% ╟──┬──╢ 0 ║     └──╢ % ╟──┤ ÷% ╟─────────┐      └──┤ % ╟──╢ mod ╟──┤
    ╙───╜  ╘═╤══╝  │  ╚═══╝        ╙───╜  ╘═╤══╝  ╔═══╗ ┌┴┐        ╘═╤═╝  ╙─────╜  │
             └──┬──┘                        └──┬──╢ 0 ║ └┬┘          │   ┌───╖     │  ╔═══╗
               ┌┴┐                             │  ╚═══╝  │          ┌┴───┤ + ╟─────┘  ║ 0 ║
               └┬┘                             └────┬────┘          │    ╘═╤═╝        ╚═╤═╝
                │                                   │               │    ┌─┴─╖        ┌─┴─╖
                                                                   ┌┴────┤ ? ╟────────┤ < ║
   ┌──────────────────────────────────┐                            │     ╘═╤═╝        ╘═╤═╝
   │                   ╓────╖       ┌─┴─╖                          └────────────────────┘
   ├───────────────────╢ ÷% ║       │ | ║                  
   │                   ╙──┬─╜       ╘═╤═╝                  ╔═════════════════════════════════════╗
   │  ┌───╖  ╔═══╗  ┌───╖ │ ┌───╖  ┌──┴──╖     ┌────┐      ║  division and modulo                ║
   └──┤ > ╟──╢ 0 ╟──┤ < ╟─┴─┤ | ╟──┤ ÷%p ╟─────┤    │      ╟─────────────────────────────────────╢
      ╘═╤═╝  ╚═══╝  ╘═╤═╝   ╘═══╝  ╘══╤══╝   ┌─┴─╖  │      ║  ÷%(a, b) =                         ║
     ┌──┴─────────┬───┴──┐          ┌─┴──┐   │ ~ ║  │      ║      let (q, r) = ÷%p(|a|, |b|);    ║
    ┌┴┐           │     ┌┴┐  ─┐   ┌─┴─╖  │   ╘═╤═╝  │      ║      a<0 ^ b<0 ? (~q, ~r) : (q, r)  ║
    └┬┘         ┌─┴─╖   └┬┘   │   │ ~ ║  │   ┌─┴─╖  │      ╚═════════════════════════════════════╝
     └────┬─────┤ · ╟────┘  ┌─┴─╖ ╘═╤═╝  │ ┌─┤ ? ╟──┘          ╔═════════════════════════════╗
          │     ╘═╤═╝    ┌──┤ ? ╟───┘    │ │ ╘═╤═╝             ║         a                   ║
          └───┬───┘      │  ╘═╤═╝        │     │               ║         ↓                   ║
              └──────────┤    └──────────┘     │               ║       ┌─┴──╖     a % b      ║
                         └─────────────────────┘               ║   b → ┤ ÷% ╟ → (Remainder)  ║
                                                               ║       ╘═╤══╝                ║
                           ╔═══╗  ┌────╖                       ║         ↓                   ║
                           ║ 1 ╟──┤ >> ╟────┐                  ║       a ÷ b                 ║
                           ╚═══╝  ╘═╤══╝    │                  ║     (Quotient)              ║
                                 ┌──┴──╖  ┌─┴─╖                ╚═════════════════════════════╝
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



