﻿

         ┌────────────────────┐
         │  ╓───╖             │
         ├──╢ × ╟──┐          │
       ┌─┴─╖╙───╜  │          │
       │ ~ ║       │          │               ╔═════════════════════╗
       ╘═╤═╝       │          │               ║  multiplication     ║
       ┌─┴─╖       │  ┌────╖  │               ╟─────────────────────╢
    ┌──┤ · ╟───────┴──┤ ×p ╟──┴─┐             ║  ×(a, b) = a ≥ 0    ║
    │  ╘═╤═╝          ╘═╤══╝    │             ║     ? ×p(a, b)      ║
    │  ┌─┴──╖  ┌───╖  ┌─┴─╖   ┌─┴─╖           ║     : ~(×p(~a, b))  ║
    │  │ ×p ╟──┤ ~ ╟──┤ ? ╟───┤ ≤ ║           ╚═════════════════════╝
    │  ╘═╤══╝  ╘═══╝  ╘═╤═╝   ╘═╤═╝
    │    │              │     ╔═╧═╗
    └────┘                    ║ 0 ║
                              ╚═══╝


  ┌────────────────────────────────┐
  │                        ╓┬───╖  │
  │            ┌───────────╫┘×p ╟──┤
  │  ┌─────────┴──────┐    ╙────╜  │         ╔════════════════════════╗
  │  │       ╔═══╗  ┌─┴─╖  ┌────╖  │         ║  multiplication where  ║
  │  └──┬────╢ 1 ╟──┤ · ╟──┤ << ╟──┘         ║  a is non-negative     ║
  │    ┌┴┐   ╚═══╝  ╘═╤═╝  ╘═╤══╝            ╟────────────────────────╢
  │    └┬┘            │      │               ║  ×p(a, b) = a          ║
  │   ┌─┴─╖  ┌───╖  ┌─┴─╖  ┌─┴──╖            ║     ? (a&1 ? b : 0) +  ║
  └───┤ ? ╟──┤ + ╟──┤ · ╟──┤ ×p ║            ║       ×p(a>>1, b<<1)   ║
      ╘═╤═╝  ╘═╤═╝  ╘═╤═╝  ╘═╤══╝            ║     : 0                ║
      ╔═╧═╗  ┌─┴─╖    │    ┌─┴──╖  ╔════╗    ╚════════════════════════╝
      ║ 0 ╟──┤ ? ╟────┴────┤ << ╟──╢ −1 ║
      ╚═══╝  ╘═╤═╝         ╘════╝  ╚════╝
               │