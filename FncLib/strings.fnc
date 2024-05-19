﻿
                                               ┌────────────────┐       ┌────────────────────┐
                           ┌──────────┐ ╓┬───╖ │  ╔════╗ ┌────╖ │       │    ╔════╗ ╔═══╗    │
                           │          ├─╫┘》p ╟─┘  ║ 21 ╟─┤ << ╟─┴─┐     │  ┌─╢ 21 ║ ║ 0 ║    │
                           │          │ ╙────╜    ╚════╝ ╘══╤═╝   │     │  │ ╚════╝ ╚═╤═╝    ├───────┐
 ╔═══════════════════════╗ │ ╔════╗ ┌─┴──╖     ┌───╖ ╔════╗ │     │     │  │ ┌────╖ ┌─┴──╖ ┌─┴─╖     │
 ║           》           ║ │ ║ 10 ╟─┤ ÷% ╟─────┤ + ╟─╢ 48 ║ │     │     │  └─┤ << ╟─┤ 》p ║ │ − ║     │
 ╟───────────────────────╢ │ ╚════╝ ╘═╤══╝     ╘═╤═╝ ╚════╝ │     │     │    ╘═╤══╝ ╘═╤══╝ ╘═╤═╝     │
 ║  Converts an integer  ║ │        ┌─┴──╖      ┌┴┐        ┌┴┐    │     │      │      └──────┘       │
 ║  to a string that     ║ │        │ 》p ╟───┐  └┬┘        └┬┘    │     │     ┌┴┐  ┌─────────┐       │
 ║  represents it in     ║ │        ╘═╤══╝ ┌─┴─╖ └────┬─────┘     │     │     └┬┘  │ ╔═══╗ ╔═╧═════╗ │
 ║  decimal.             ║ │          └────┤ · ╟──────┘           │   ┌─┴──╖   └─┬─┘ ║ 0 ║ ║ −8723 ║ │
 ╟───────────────────────╢ │               ╘═╤═╝                  │   │ 》p ╟─┐   │   ╚═╤═╝ ╚═══════╝ │
 ║  Uses − (minus), not  ║ │               ┌─┴─╖                  │   ╘═╤══╝ │ ┌─┴─╖ ┌─┴─╖           │
 ║  - (hyphen) for       ║ │           ┌───┤ ? ╟─┐                │   ╔═╧═╗  └─┤ ? ╟─┤ < ║   ╓───╖   │
 ║  negatives.           ║ │           │   ╘═╤═╝ │                │   ║ 0 ║    ╘═╤═╝ ╘═╤═╝   ║ 》 ║   │
 ╚═══════════════════════╝ │         ┌─┴─╖   │   │                │   ╚═══╝      │     │     ╙─┬─╜   │
                           └─────────┤ · ╟───────┘                │     ╔════╗ ┌─┴─╖   ├───────┴─────┘
                                     ╘═╤═╝                        │     ║ 48 ╟─┤ ? ╟───┘
                                       └──────────────────────────┘     ╚════╝ ╘═╤═╝
                       ╓───╖                                                     │
                       ║ ℓ ║                                                        ┌────┐   ╓───╖
        ╔═════╗ ┌────╖ ╙─┬─╜     ╔═══╤═════╤═════════════════╗                      │    ├───╢ ‼ ╟──┐
        ║ −21 ╟─┤ << ╟───┴─┐     ║ ← │  ℓ  │  String length  ║                      │  ┌─┴─╖ ╙───╜  │
        ╚═════╝ ╘═╤══╝     │     ╚═══╧═════╧═════════════════╝                      │  │ ℓ ║        │
                ┌─┴─╖      │                                                        │  ╘═╤═╝        │
                │ ℓ ║      │                                                        │  ┌─┴─╖ ┌────╖ │
                ╘═╤═╝      │                                                        │  │ × ╟─┤ << ╟─┘
                ┌─┴─╖      │             ╔═════╤═══════════════════════════╤═══╗    │  ╘═╤═╝ ╘═╤══╝
                │ ♯ ║      │             ║  ‼  │  Concatenate two strings  │ → ║    │  ╔═╧══╗  │
                ╘═╤═╝      │             ╚═════╧═══════════════════════════╧═══╝   ┌┴┐ ║ 21 ║ ┌┴┐
          ╔═══╗ ┌─┴─╖      │                                                       └┬┘ ╚════╝ └┬┘
          ║ 0 ╟─┤ ? ╟──────┘                                                        └───────┬──┘
          ╚═══╝ ╘═╤═╝                                                                       │
                  │

                           ┌────────────────────────┐       ╔═════╤══════════════════════════╗
      ╔═══════════╗        │     ╓┬───╖             │       ║  ʃ  │  Substring               ║
      ║     ⮌     ║        ├─────╫┘⮌p ╟───┐         │       ╟─────┴──────────────────────────╢
      ╟───────────╢      ┌─┴─╖   ╙────╜   │         │       ║  Takes start index and         ║
      ║  Reverse  ║    ┌─┤ · ╟────────────┘         │       ║  length of desired substring.  ║
      ║  string   ║    │ ╘═╤═╝ ╔═════════╗  ╔═════╗ │       ╚════════════════════════════════╝
      ╚═══════════╝    │   └─┬─╢ 2097151 ║  ║ −21 ║ │                  ╓───╖
                       │     │ ╚═════════╝  ╚══╤══╝ │             ┌────╢ ʃ ╟────┐
           ╓───╖       │     │        ┌────╖ ┌─┴──╖ │             │    ╙─┬─╜  ┌─┴─╖ ┌────╖ ╔════╗
           ║ ⮌ ║       │     ├────────┤ ⮌p ╟─┤ << ║ │             │      │    │ × ╟─┤ << ╟─╢ −1 ║
           ╙─┬─╜       │    ┌┴┐       ╘══╤═╝ ╘═╤══╝ │             │      │    ╘═╤═╝ ╘═╤══╝ ╚════╝
           ┌─┴──╖      │    └┬┘          │     ├────┘             │      │   ╔══╧═╗   │
           │ ⮌p ╟──┐   │   ┌─┴──╖ ╔════╗ │     │                  │      │   ║ 21 ║  ┌┴┐
           ╘═╤══╝  │   │ ┌─┤ << ╟─╢ 21 ║ │     │                ┌─┴──╖ ┌─┴─╖ ╚════╝  └┬┘
           ╔═╧═╗       │ │ ╘════╝ ╚════╝ │     │                │ << ╟─┤ · ╟───────┬──┘
           ║ 0 ║       └─┤             ┌─┴─╖   │                ╘═╤══╝ ╘═╤═╝      ┌┴┐
           ╚═══╝         └─────────────┤ ? ╟───┘        ╔═════╗ ┌─┴─╖    │        └┬┘
                                       ╘═╤═╝            ║ −21 ╟─┤ × ╟────┘         │
                                         │              ╚═════╝ ╘═══╝


                 ┌────────────────────┐        ┌──────────────────────────────┐
                 │   ┌────────────┐   │        │   ┌──────────────────────┐   │
                 │ ┌─┴──╖ ╓───╖   │   │        │   │       ┌──────┐       │   │
                 └─┤ ʘp ╟─╢ ʘ ╟───┤   │        │ ┌─┴──╖ ┌──┴─╖ ╔══╧══╗    │   │
                   ╘═╤══╝ ╙───╜ ┌─┴─╖ │        └─┤ ʘp ╟─┤ << ║ ║ −21 ║    │   │
                     │          │ ℓ ║ │          ╘═╤══╝ ╘══╤═╝ ╚═════╝    │   │
                                ╘═╤═╝ │       ┌────┴────┐  │              │   │
                                ┌─┴─╖ │     ┌─┴─╖ ╔═══╗ │  │              │   │
                                │ × ╟─┘     │ ♯ ║ ║ 0 ║ │  │              │   │
                                ╘═╤═╝       ╘═╤═╝ ╚═╤═╝ │  │   ╓┬───╖   ┌─┴─╖ │
                                ╔═╧══╗      ┌─┴─╖ ┌─┴─╖ │  ├───╫┘ʘp ╟───┤ · ╟─┤
                                ║ 21 ║  ┌───┤ ? ╟─┤ ≥ ║ │  │   ╙──┬─╜   ╘═╤═╝ │
                                ╚════╝  │   ╘═╤═╝ ╘═╤═╝ │  │      └───────┤   │
                                        │     │     └───┘┌─┴─╖          ┌─┴─╖ │
                                        │     │     ┌────┤ · ╟──────────┤ · ╟─┴─┐
             ╔════════════════╗      ╔══╧═╗ ┌─┴─╖ ┌─┴──╖ ╘═╤═╝          ╘═╤═╝   │
             ║        ʘ       ║      ║ −1 ╟─┤ ? ╟─┤ >> ║   │              │     │
             ╟────────────────╢      ╚════╝ ╘═╤═╝ ╘═╤══╝   │              │     │
             ║      Index     ║               │     ├──────┘              │     │
             ║    of first    ║               │     └─┬────────────┐      │     │
             ║   occurrence   ║               │      ┌┴┐          ┌┴┐     │     │
             ║  of substring  ║               │      └┬┘          └┬┘     │     │
             ║   (−1 if not   ║       ╔═══╗ ┌─┴─╖   ┌─┴─╖ ╔════╗ ┌─┴──╖ ┌─┴─╖   │
             ║    present)    ║       ║ 0 ╟─┤ ? ╟───┤ ≠ ║ ║ −1 ╟─┤ << ╟─┤ · ╟───┘
             ╚════════════════╝       ╚═══╝ ╘═╤═╝   ╘═╤═╝ ╚════╝ ╘════╝ ╘═╤═╝
                                              │       └───────────────────┘


     ┌───────────────────────────────┐                    ╔════════════════════════════════╗
     │    ┌─────────────────┐      ┌─┴─╖                  ║                ǂ               ║
     │  ┌─┴─╖ ┌────╖ ╔════╗ │   ┌──┤ · ╟──────────┐       ╟────────────────────────────────╢
     │  │ × ╟─┤ << ╟─╢ −1 ║ │   │  ╘═╤═╝  ╓───╖   │       ║  Splits a string into two at   ║
     │  ╘═╤═╝ ╘═╤══╝ ╚════╝ │ ┌─┴─╖  │    ║ ǂ ╟───┤       ║  the first occurrence of a     ║
     │ ╔══╧═╗   │           ├─┤ ʘ ║  │    ╙─┬─╜   │       ║  separator.                    ║
     │ ║ 21 ║  ┌┴┐          │ ╘═╤═╝  ├──────┘     │       ╟────────────────────────────────╢
     │ ╚════╝  └┬┘          │   ├────┘            │       ║  If separator is not present,  ║
     │   ┌───┬──┘           │ ┌─┴─╖ ┌───╖ ╔═══╗ ┌─┴─╖     ║  returns the original string   ║
     │   │  ┌┴┐           ┌─┴─┤ · ╟─┤ ≤ ╟─╢ 0 ║ │ ℓ ║     ║  and an empty string.          ║
     └───┤  └┬┘           │   ╘═╤═╝ ╘═╤═╝ ╚═══╝ ╘═╤═╝     ╟────────────────────────────────╢
         │ ┌─┴─╖        ┌─┴─╖ ┌─┴─╖   │           │       ║          string                ║
         └─┤ ? ╟────────┤ · ╟─┤ · ╟───┴──────┐    │       ║            ↓                   ║
           ╘═╤═╝        ╘═╤═╝ ╘═╤═╝          │    │       ║  sep     ┌─┴─╖     second      ║
             │            │   ┌─┴──╖       ┌─┴─╖  │       ║  ara → ──┤ ǂ ╟── → substring   ║
                          │   │ >> ╟───────┤ ? ╟─ │       ║  tor     ╘═╤═╝     (or empty)  ║
                          │   ╘═╤══╝       ╘═╤═╝  │       ║            ↓                   ║
                        ┌─┴─╖ ┌─┴─╖ ╔════╗ ╔═╧═╗  │       ║     first substring            ║
                        │ + ╟─┤ × ╟─╢ 21 ║ ║ 0 ║  │       ║   (or original string)         ║
                        ╘═╤═╝ ╘═══╝ ╚════╝ ╚═══╝  │       ╚════════════════════════════════╝
                          └───────────────────────┘




           ╔═════╤═══════════════════════════════════════════════════════════════════════════╗
           ║  …  │  Repeat string                                                            ║
           ╟─────┴───────────────────────────────────────────────────────────────────────────╢
           ║  Returns a string consisting of a specified number of repetitions of a string.  ║
           ╚═════════════════════════════════════════════════════════════════════════════════╝


            ╔════╗  ┌───╖  ┌───╖                     ┌────────────────────────────────┐
            ║ 21 ╟──┤ × ╟──┤ ℓ ╟──┐                  │                       ╓┬────╖  │
            ╚════╝  ╘═╤═╝  ╘═══╝  │  ╓───╖           │                   ┌───╫┘ …p ╟──┘   ┌────╖  ╔════╗
                    ┌─┴──╖        ├──╢ … ╟─┐         │                 ┌─┴─╖ ╙──┬──╜   ┌──┤ << ╟──╢ −1 ║
                 ┌──┤ …p ╟────────┘  ╙───╜ │         │             ┌───┤ · ╟────┴──────┘  ╘══╤═╝  ╚════╝
                 │  ╘═╤══╝                 │         │         ┌───┴──┐╘═╤═╝                ┌┴┐
                 │  ┌─┴─╖                  │         │ ┌───╖ ┌─┴─╖ ┌──┴─╖└─────────┐        └┬┘
                 └──┤ · ╟────┐             │       ┌─┴─┤ ♭ ╟─┤ · ╟─┤ …p ╟──────────┴───┐   ┌─┴─╖
                    ╘═╤═╝    ├─────────────┘       │   ╘═══╝ ╘═╤═╝ ╘══╤═╝              └───┤ · ╟─┐
             ╔═══╗  ┌─┴─╖  ┌─┴─╖                   │           │   ┌──┴─╖        ┌──┐      ╘═╤═╝ │
             ║ 0 ╟──┤ ‽ ╟──┤ < ║                   │           │   │ << ╟────────┤  ├───┬────┤   │
             ╚═╤═╝  ╘═╤═╝  ╘═╤═╝                   │           │   ╘══╤═╝  ╔═══╗ └──┘ ┌─┴─╖  └───┘
               │      │      │                     │           └──────┘    ║ 0 ╟──────┤ ? ╟──┐
               │             │                     │                       ╚═══╝      ╘═╤═╝  │
               └─────────────┘                     │                                    │    │
                                                   │                                         │
                                                   └─────────────────────────────────────────┘
                         ╓───╖
                         ║ ↯ ║
                         ╙─┬─╜
                ┌──────────┴───────┐     ╔═══════════════════════════════════╗
                │            ╔═══╗ │     ║                 ↯                 ║
                │      ┌─┐   ║ 0 ║ │     ╟───────────────────────────────────╢
                │      └─┤   ╚═╤═╝ │     ║  Converts a string into a lazy    ║
         ┌──────┴─┐ ┌┐ ╔═╧═╕ ┌─┴─╖ │     ║  sequence of its characters.      ║
         │        ├─┤├─╢   ├─┤ ? ╟─┘     ╚═══════════════════════════════════╝
         │        │ └┘ ╚═╤═╛ ╘═╤═╝
         │ ╔══════╧══╗ ┌─┴─╖   │
         │ ║ 2097151 ║ │ ↯ ║
         │ ╚═════════╝ ╘═╤═╝
         │             ┌─┴──╖ ╔════╗
         └─────────────┤ >> ╟─╢ 21 ║
                       ╘════╝ ╚════╝

                            ╓───╖          ╔═════════════════════════════════════════════════════╗
                            ║ 《 ║          ║  String to integer conversion                       ║
     ╔═════════╗            ╙─┬─╜          ╟─────────────────────────────────────────────────────╢
     ║ 2097151 ╟────┬─────────┴─────────┐  ║  • Converts numeric strings to their integer value  ║
     ╚═════════╝   ┌┴┐                  │  ║  • Accepts − (minus) or - (hyphen) for negative     ║
     ╔════╗  ┌───╖ └┬┘ ┌───╖  ╔══════╗  │  ║  • Returns 0 if the string is not an integer        ║
     ║ 45 ╟──┤ ≠ ╟──┴──┤ ≠ ╟──╢ 8722 ║  │  ║  • No tolerance for leading/trailing whitespace     ║
     ╚════╝  ╘═╤═╝     ╘═╤═╝  ╚══════╝  │  ║  • No support for thousands separators              ║
               └────┬────┘            ┌─┴─╖╚═════════════════════════════════════════════════════╝
             ┌──────┴─────────────────┤ · ╟──────────────────────┐
             │                        ╘═╤═╝                      │
             │                          │  ┌──────────────────┐  │
             │                          │  │  ╔════╗  ┌────╖  │  │    ╔═════════════════════════════╗
             │                          └──┤  ║ 21 ╟──┤ >> ╟──┘  │    ║  Deprecated function names  ║
             │                             │  ╚════╝  ╘═╤══╝     │    ║ for backwards compatibility ║
             │                             │          ┌─┴─╖      │    ╚═════════════════════════════╝
             │                             └──────────┤ ? ╟──────┘       ╓─────────╖  ╓─────────╖
             │    ╔═══╗                   ┌────╖      ╘═╤═╝              ║ str→int ║  ║ int→str ║
             │    ║ 0 ╟───────────────────┤ 《p ╟────────┘                ╙────┬────╜  ╙────┬────╜
             │    ╚═╤═╝    ┌──────┐       ╘═╤══╝                            ┌─┴─╖        ┌─┴─╖
           ┌─┴─╖  ┌─┴─╖  ┌─┴─╖  ╔═╧══╗      │                               │ 《 ║        │ 》 ║
        ┌──┤ ‽ ╟──┤ ‽ ╟──┤ = ║  ║ −1 ║      │                               ╘═╤═╝        ╘═╤═╝
      ┌─┴─╖╘═╤═╝  ╘═╤═╝  ╘═╤═╝  ╚════╝      │                                 │            │
      │ − ║  │      │      ├────────────────┘
      ╘═╤═╝  ├─────────────┘
        └────┘

            ╓┬────╖
      ┌─────╫┘ 《p ╟────────────────────┐
      │     ╙─────╜      ┌─────────────┴─────────────┐
      │                  │      ╔═════════╗          │
      │               ┌──┴───┬──╢ 2097151 ║          │
   ┌──┴─┐             │     ┌┴┐ ╚═════════╝ ╔════╗   │
   │  ┌─┴─╖  ┌───╖  ┌─┴─╖   └┬┘             ║ 57 ╟─┐ │
   │  │ × ╟──┤ + ╟──┤ · ╟────┴───────────┐  ╚════╝ │ │
   │  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝ ╔════╗  ┌───╖  │  ┌───╖  │ │
   │  ╔═╧══╗   │      │   ║ 48 ╟──┤ ≥ ╟──┴──┤ ≥ ╟──┘ │
   │  ║ 10 ║   │      └┐  ╚════╝  ╘═╤═╝     ╘═╤═╝    │
   │  ╚════╝   │       └┐   ┌───┐   └────┬────┘      │
   │  ┌────────┘        └┐  │   ├────────┘    ┌──────┘
   │  │  ╔════╗  ┌────╖  │  └─┬─┘             │  ╔══════╤═════════════════════════════════════╗
   │  │  ║ 21 ╟──┤ >> ╟──┘    ├────────────┐  │  ║  《p  │  string-to-integer helper function  ║
   │  │  ╚════╝  ╘═╤══╝     ┌─┴─╖  ╔════╗  │  │  ╚══════╧═════════════════════════════════════╝
   │  │  ┌─────────┴─╖    ┌─┤ ≠ ╟──╢ −1 ║  │  │
   │  │  │    《p     ╟────┤ ╘═══╝  ╚══╤═╝  │  │
   │  │  ╘═╤═════════╝    │         ┌─┴─╖  │  │
   │  │  ┌─┴─╖  ╔═════╗   └─────────┤ ? ╟──┘  │
   │  └──┤ + ╟──╢ −48 ║             ╘═╤═╝     │
   │     ╘═══╝  ╚═════╝             ┌─┴─╖     │
   └────────────────────────────────┤ ? ╟─────┘
                                    ╘═╤═╝
                                      │
                                   └──┘
