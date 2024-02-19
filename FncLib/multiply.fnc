﻿
╔════════════════════════════════════════════════════════════════════════════════════════════════╗
║                                                  MULTIPLICATION                                ║
╟────────────────────────────────────────────────────────────────────────────────────────────────╢
║  We could do multiplication the simple/boring way:                                             ║
║                                                                                                ║
║      ×(a, b) = a ? +(b, ×(a−1, b)) : 0                                                         ║
║                                                                                                ║
║  But that would be exponential-time. We can have it linear-time using the following recursive  ║
║  formula instead:                                                                              ║
║                                                                                                ║
║      ×(a, b) = a ? ((a & 1) ? b : 0) + ×(a >> 1, b << 1) : 0                                   ║
║                                                                                                ║
║  Proof that the formula is correct:                                                            ║
║                                                                                                ║
║      If a is even (a & 1 = 0):                                                                 ║
║          0 + (a >> 1)×(b << 1) = 0 + a/2 × b×2 = a × b ✓                                       ║
║      If a is odd (a & 1 ≠ 0):                                                                  ║
║          b + (a >> 1)×(b << 1) = b + (a−1)/2 × b×2 = b + (a−1)×b = a × b ✓                     ║
║                                                                                                ║
║  However, the recursion is infinite unless a is non-negative (otherwise no amount of shifting  ║
║  it to the right will ever make it zero). Thus, if a is negative, we need to use unary-minus   ║
║  before and after the multiplication to fix this.                                              ║
╚════════════════════════════════════════════════════════════════════════════════════════════════╝

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