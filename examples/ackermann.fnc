﻿ ╔═══════════════════════════════════════════╗
 ║  Ackermann function                       ║
 ╟───────────────────────────────────────────╢
 ║  Acker(a, b) = !a ? succ(b) :             ║
 ║                !b ? Acker(a−1, 1) :       ║
 ║                Acker(a−1, Acker(a, b−1))  ║
 ╚═══════════════════════════════════════════╝
     ┌───────────────────────────────┐
     │      ╓───────╖                │
     ├──────╢ Acker ╟─────┐          │
    ┌┴┐     ╙───────╜     │    ┌─────┴────┐
    └┬┘                   │    │          │
  ┌──┴───╖                │  ┌─┴─╖        │
  │ succ ║          ┌─────┴──┤ · ╟──┐     │
  ╘══╤═══╝       ┌──┴───┐    ╘═╤═╝  │     │
    ┌┴┐          │   ┌──┴───╖  │    │     │
    └┬┘         ┌┴┐  │ succ ║ ┌┴┐  ┌┴┐    │
  ┌──┴──┐       └┬┘  ╘══╤═══╝ └┬┘  └┬┘    │
  │ ┌───┴───╖  ┌─┴─╖  ┌─┴─╖    │ ┌──┴───╖ │
  │ │ Acker ╟──┤ ? ╟──┤ ? ╟────┘ │ succ ║ │
  │ ╘═══╤═══╝  ╘═╤═╝  ╘═╤═╝      ╘══╤═══╝ │
  │   ╔═╧═╗      │      │          ┌┴┐    │
  │   ║ 1 ║      │                 └┬┘    │
  │   ╚═══╝  ┌───┴───╖          ┌───┴───╖ │
  └──────────┤ Acker ╟──────────┤ Acker ║ │
             ╘═══════╝          ╘═══╤═══╝ │
                                    └─────┘
