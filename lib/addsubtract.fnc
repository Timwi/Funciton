﻿

                             ╓───╖
                             ║ ♯ ║             ╔════════════════════════════╗
                             ╙─┬─╜             ║  successor function        ║         ╓───╖
                    ┌──────────┴────────┐      ╟────────────────────────────╢         ║ ♭ ║
    ╔════╗  ┌────╖  │      ╔═══╗        │      ║  ♯(a) = a = -1 ? 0 :       ║         ╙─┬─╜     ╔════════════════════════╗
    ║ -1 ╟──┤ << ╟──┴─┬────╢ 1 ║        │      ║    a & 1 ? ♯(a>>1) << 1 :  ║          ┌┴┐      ║  predecessor function  ║
    ╚════╝  ╘══╤═╝    │    ╚═══╝        │      ║    a | 1                   ║          └┬┘      ╟────────────────────────╢
             ┌─┴─╖   ┌┴┐   ╔═══╗ ╔════╗ │      ╟────────────────────────────╢         ┌─┴─╖     ║  ♭(a) = ¬♯(¬a)         ║
             │ ♯ ║   └┬┘   ║ 0 ║ ║ -1 ║ │      ║  Note: can’t actually use  ║         │ ♯ ║     ╚════════════════════════╝
             ╘═╤═╝    │    ╚═╤═╝ ╚══╤═╝ │      ║  >> because >> depends on  ║         ╘═╤═╝
            ┌──┴─╖  ┌─┴─╖  ┌─┴─╖  ┌─┴─╖ │      ║  ~ (unary minus) which in  ║          ┌┴┐
            │ << ╟──┤ ? ╟──┤ ? ╟──┤ = ║ │      ║  turn depends on the       ║          └┬┘
            ╘══╤═╝  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝ │      ║  successor function.       ║           │
             ╔═╧═╗ ┌┐ │ ┌┐   │      ├───┘      ╚════════════════════════════╝
             ║ 1 ╟─┤├─┴─┤├──────────┘
             ╚═══╝ └┘   └┘


       ┌──────────────────────────────┐  ╔══════════════════════════════════╗
       │       ╓───╖                  │  ║  addition                        ║
       ├───────╢ + ╟───────┐          │  ╟──────────────────────────────────╢
     ┌─┴─╖     ╙───╜       │          │  ║  +(a, b) = (a≥0 & b≥0) | (¬b)≥a  ║
  ┌──┤ · ╟─────────────────┴──┐       │  ║            ? +p(a,b)             ║
  │  ╘═╤═╝          ┌─────────┴──┐    │  ║            : ¬(♯(+p(¬a, ¬b)))    ║
  │    │            │  ┌────╖  ┌─┴─╖  │  ╚══════════════════════════════════╝
  │    │            └──┤ +p ╟──┤ · ╟──┴──────────────────┐
 ┌┴┐  ┌┴┐              ╘═╤══╝  ╘═╤═╝┌───╖  ╔═══╗  ┌───╖  │
 └┬┘  └┬┘                │       └──┤ ≤ ╟──╢ 0 ╟──┤ ≥ ╟──┴─┐
  │    │                 │          ╘═╤═╝  ╚═══╝  ╘═╤═╝    │
  │ ┌──┴─╖  ┌───╖  ┌┐  ┌─┴─╖          └──────┬──────┘      │
  │ │ +p ╟──┤ ♯ ╟──┤├──┤ ? ╟─────────────────┤             │
  │ ╘══╤═╝  ╘═══╝  └┘  ╘═╤═╝                 │             │
  │    │                 │                   │             │
  └────┤                                   ┌─┴─╖           │
       └───────────────────────────────────┤ < ╟───────────┘
                                           ╘═══╝


                     ╓┬───╖
                  ┌──╫┘+p ╟──┐
                  │  ╙────╜  │
             ┌────┴─────┬────┴────┐
             │ ┌───┐  ┌─┴─╖    ┌┐ │
           ┌─┴─┤   ├──┤ · ╟──┬─┤├─┴─┐
           │   └───┘  ╘═╤═╝  │ └┘   │          ╔═══════════════════════════╗
           │    ┌───────┴───┬┘      │          ║  addition in the case of  ║
           │   ┌┴┐         ┌┴┐      │          ║  (a≥0 & b≥0) | (¬b)≥a     ║
           │   └┬┘         └┬┘      │          ╟───────────────────────────╢
           │ ┌──┴─╖  ┌────╖ │       │          ║  +p(a, b) = b             ║
           │ │ << ╟──┤ +p ╟─┘       │          ║    ? +p(a^b, (a&b) << 1)  ║
           │ ╘══╤═╝  ╘══╤═╝         │          ║    : a                    ║
           │  ╔═╧═╗     │           │          ╚═══════════════════════════╝
           │  ║ 1 ║     │           │
           │  ╚═══╝   ┌─┴─╖         │
           └──────────┤ ? ╟─────────┘
                      ╘═╤═╝
                        │


 ╔═══════════════╗   ╔════════════════════╗   ╔════════════════════════╗
 ║  unary minus  ║   ║  subtraction       ║   ║  absolute value        ║
 ╟───────────────╢   ╟────────────────────╢   ╟────────────────────────╢
 ║  ~x = ♯(¬x)   ║   ║  a − b = a + (~b)  ║   ║  |x| = x < 0 ? ~x : x  ║
 ╚═══════════════╝   ╚════════════════════╝   ╚════════════════════════╝

         ╓───╖               ╓───╖                                 ┌───╖  ╔═══╗
         ║ ~ ║            ┌──╢ − ╟───┐                 ┌───────────┤ > ╟──╢ 0 ║
         ╙─┬─╜            │  ╙───╜ ┌─┴─╖        ╓───╖  │           ╘═╤═╝  ╚═══╝
          ┌┴┐             │        │ ~ ║        ║ | ╟──┤    ┌───╖  ┌─┴─╖
          └┬┘             │  ┌───╖ ╘═╤═╝        ╙───╜  │ ┌──┤ ~ ╟──┤ ? ╟──
         ┌─┴─╖            └──┤ + ╟───┘                 └─┤  ╘═══╝  ╘═╤═╝
         │ ♯ ║               ╘═╤═╝                       └───────────┘
         ╘═╤═╝                 │
           │
