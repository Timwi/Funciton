﻿
    ╔═════╤═════╤════════════════════════╗                                                ┌─────────┐
    ║  ᴜ  │  ›  │  list unshift          ║                              ╓┬───╖          ┌─┴─╖ ╓───╖ │
    ╟─────┴─────┴────────────────────────╢                              ╟┘ᴜp ╟───┐    ┌─┤ ᴜ ╟─╢ › ╟─┘  ┌──────────────────────────────┐
    ║  Prepends an element to the start  ║                              ╙──┬─╜   │    │ ╘═╤═╝ ╙───╜    │              ╔═══╗           │
    ║  of a list. Returns the new list   ║      ┌──────────────────────────┴─┐   │        ├─┐          │              ║ 0 ║           │
    ║  and the new element’s bitlength.  ║      │                          ┌─┴─╖ │        └─┘          │              ╚═╤═╝           │
    ╟────────────────────────────────────╢      │  ╔═════════╗      ┌──────┤ · ╟─┴────────────┐        │              ┌─┴─╖           │
    ║               LIST                 ║      │  ║ 2097151 ╟──┬───┴─┐    ╘═╤═╝              │        ├──────────────┤ < ║           │
    ║                ↓                   ║      │  ╚═════════╝ ┌┴┐ ┌──┴─╖ ┌──┴─╖ ┌───╖ ╔════╗ │        │   ╓───╖      ╘═╤═╝           │
    ║              ┌─┴─╖                 ║      │ ╔═══╗ ┌────╖ └┬┘ │ << ╟─┤ ᴜp ╟─┤ + ╟─╢ 22 ║ │        │   ║ ᴜ ╟────────┤             │
    ║   ELEMENT → ─┤ ᴜ ╟─ → (bitlength   ║      │ ║ 1 ╟─┤ << ╟──┘  ╘══╤═╝ ╘══╤═╝ ╘═╤═╝ ╚════╝ │        │   ╙─┬─╜        │       ╔═══╗ │
    ║              ╘═╤═╝      of new     ║      │ ╚═══╝ ╘═╤══╝     ╔══╧══╗   │     │   ┌────┐ │      ┌─┴─╖ ┌─┴──╖       │       ║ 1 ║ │
    ║                ↓        element)   ║      └──────┐  │        ║ −21 ║   │     │ ┌─┴─╖  ├─┘   ┌──┤ ? ╟─┤ ᴜp ╟───    │       ╚═╤═╝ │
    ║           (new list)               ║      ┌────╖ │  │        ╚═════╝   │     └─┤ ? ╟─ │    ┌┴┐ ╘═╤═╝ ╘═╤══╝       │ ╔═══╗ ┌─┴─╖ │
    ╟────────────────────────────────────╢    ┌─┤ << ╟─┘  │ ╔════╗ ┌────╖    │       ╘═╤═╝  │    └┬┘   │   ┌─┴──╖       │ ║ 0 ╟─┤ ? ╟─┘
    ║                 ┌───╖              ║    │ ╘══╤═╝    │ ║ 22 ╟─┤ << ╟────┘       ╔═╧═╗  │     └──┬─┘   │ << ╟──┐    │ ╚═══╝ ╘═╤═╝
    ║      ELEMENT → ─┤ › ╟─ ← LIST      ║    │   ┌┴┐     │ ╚════╝ ╘═╤══╝            ║ 2 ║  │        │     ╘═╤══╝ ┌┴┐   │        ┌┴┐
    ║                 ╘═╤═╝              ║    │   └┬┘    ┌┴┐        ┌┴┐              ╚═══╝  │        │     ╔═╧═╗  └┬┘ ┌─┴─╖      └┬┘
    ║               (new list)           ║    │    ├───┐ └┬┘        └┬┘                     │        │     ║ 1 ║   └──┤ · ╟───┬───┘
    ╚════════════════════════════════════╝    │   ┌┴┐  │  └─┬────────┘                      │        │     ╚═══╝      ╘═╤═╝   │
                                              │   └┬┘  │  ┌─┴─╖                             │        └──────────────────┘
                                              │  ╔═╧═╗ └──┤ ? ╟─────────────────────────────┘
                                              └──╢ 1 ║    ╘═╤═╝
    ╔═════╤═════╤════════════════════════╗       ╚═══╝      │       ┌──────────────────────────┐    ╔════╗ ┌────╖
    ║  ʜ  │  ‹  │  list shift            ║                          │    ╔═══╗          ╓┬───╖ │    ║ −1 ╟─┤ << ╟──────────┐
    ╟─────┴─────┴────────────────────────╢   ┌──────────────────────┴─┬──╢ 1 ║          ╟┘ʜp ║ │    ╚════╝ ╘═╤══╝    ╓───╖ │
    ║  Returns the first element (head)  ║   │                       ┌┴┐ ╚═══╝    ┌───┐ ╙─┬──╜ │           ┌─┴──╖    ║ ʜ ║ │
    ║  of a list, the rest of the list   ║   │                       └┬┘        ┌─┴─╖ └───┴────┘          ─┤ ʜp ╟─   ╙─┬─╜ │
    ║  (tail) and the head’s bitlength.  ║   │ ┌──────────────────────┴─────────┤ · ╟─────────────┐        ╘═╤══╝      └───┤
    ╟────────────────────────────────────╢   │ │                                ╘═╤═╝       ┌─────┴────┐   ┌─┴─┐  ╔═══╗    │
    ║              LIST                  ║   │ │                   ╔═════╗ ┌────╖ │ ╔═══╗ ┌─┴─╖        │   │  ┌┴┐ ║ 1 ╟──┬─┘
    ║               ↓                    ║   │ │                   ║ −22 ╟─┤ << ╟─┘ ║ 2 ╟─┤ ? ╟──      │   │  └┬┘ ╚═══╝ ┌┴┐
    ║             ┌─┴─╖                  ║   │ │ ──┐               ╚═════╝ ╘═╤══╝   ╚═══╝ ╘═╤═╝        │   │ ┌─┴─╖      └┬┘
    ║   (tail) ← ─┤ ʜ ╟─ → (bitlength    ║   │ │ ┌─┴─╖                     ┌─┴──╖         ┌─┴─╖ ╔════╗ │   └─┤ ? ╟───────┘
    ║             ╘═╤═╝      of head)    ║   │ └─┤ ? ╟─────────────────────┤ ʜp ╟─────────┤ + ╟─╢ 22 ║ │     ╘═╤═╝
    ║               ↓                    ║   │   ╘═╤═╝                     ╘═╤══╝         ╘═══╝ ╚════╝ │       │
    ║             (head)                 ║   │   ┌─┴──╖ ╔════╗ ╔════╗ ┌────╖ │                         │
    ╟────────────────────────────────────╢   │ ┌─┤ << ╟─╢ −1 ║ ║ 21 ╟─┤ << ╟─┘        ╔═══╗            │     ╓───╖
    ║                LIST                ║   └─┤ ╘════╝ ╚════╝ ╚════╝ ╘═╤══╝          ║ 0 ║            │     ║ ‹ ║
    ║                 ↓                  ║     │                       ┌┴┐            ╚═╤═╝            │     ╙─┬─╜
    ║               ┌─┴─╖                ║     └───────────────────┐   └┬┘            ┌─┴─╖            │     ┌─┴─╖
    ║     (tail) ← ─┤ ‹ ╟─ → (head)      ║                         │    ├─────────────┤ ? ╟────────────┘    ─┤ ʜ ╟─┬─┐
    ║               ╘═══╝                ║                ╔════╗ ┌─┴──╖ │ ╔═════════╗ ╘═╤═╝                  ╘═╤═╝ └─┘
    ╚════════════════════════════════════╝                ║ −1 ║ │ << ╟─┴─╢ 2097151 ║   │                      └─
                                                          ╚══╤═╝ ╘═╤══╝   ╚═════════╝
                                                             └─────┘

       ╓───╖
       ║ [ ╟─────────────────┐
       ╙─┬─╜                 │
     ┌───┴───┐      ┌────────┴────────┐      ╔═════╤═════════════════════════╗
     │     ┌─┴─╖  ┌─┴─╖               │      ║  [  │  indexing               ║
     │  ┌──┤ · ╟──┤ · ╟────┐ ┌─┐      │      ╟─────┴─────────────────────────╢
     │  │  ╘═╤═╝  ╘═╤═╝    │ └─┤      │      ║  Given a list and an index,   ║
     │  │    │    ┌─┴─╖  ┌─┴─╖ │      │      ║  returns the element at the   ║
     │  │    └────┤ · ╟──┤ ʜ ╟─┘  ┌───┴─┐    ║  index and the bit-length     ║
     │  │         ╘═╤═╝  ╘═╤═╝    │     │    ║  of all preceding elements    ║
     │  │         ┌─┴─╖  ┌─┴─╖  ┌─┴─╖   │    ╟───────────────────────────────╢
     │  │         │ + ╟──┤ [ ╟──┤ ? ╟── │    ║            LIST               ║
     │  │  ╔════╗ ╘═╤═╝  ╘═╤═╝  ╘═╤═╝   │    ║             ↓                 ║
     │  │  ║ −1 ╟───┘      │    ┌─┴─╖   │    ║           ┌─┴─╖               ║
     │  │  ╚════╝ ┌───╖    │ ┌──┤ ‹ ║   │    ║  INDEX → ─┤ [ ╟─ → (element)  ║
     │  └─────────┤ + ╟────┘ │  ╘═╤═╝   │    ║           ╘═╤═╝               ║
     │            ╘═╤═╝      │    ├─┐   │    ║             ↓                 ║
     │     ╔═══╗  ┌─┴─╖    ┌─┴─╖  └─┘   │    ║        (preceding             ║
     │     ║ 0 ╟──┤ ? ╟────┤ · ╟────────┘    ║         bitlength)            ║
     │     ╚═══╝  ╘═╤═╝    ╘═╤═╝             ╚═══════════════════════════════╝
     │              │        │
     └───────────────────────┘
                                             ╔═════╤════════════════╗
               ╔═════╤════════════════╗      ║  ʁ  │  list element  ║
               ║  ɛ  │  list element  ║      ║     │  bit offset    ║
               ╚═════╧════════════════╝      ╚═════╧════════════════╝
                      ┌────────────┐                 ┌────────────┐
                    ┌─┴─╖   ╓───╖  │               ┌─┴─╖   ╓───╖  │
                ┌─┬─┤ [ ╟───╢ ɛ ╟──┘          ┌────┤ [ ╟───╢ ʁ ╟──┘
                └─┘ ╘═╤═╝   ╙───╜             │    ╘═╤═╝   ╙───╜
                      │                              ├─┐
                                                     └─┘

   ┌───────────┐
   │  ╓───╖    │    ┌──────────────────────────┐      ╔═════╤═════════════════════════════════╗
   │  ║ ] ╟────┤  ┌─┴─╖                        │      ║  ]  │  reverse indexing               ║
   │  ╙─┬─╜  ┌─┴──┤ · ╟──────────────┐         │      ╟─────┴─────────────────────────────────╢
   │    │  ┌─┴──╖ ╘═╤═╝              │         │      ║  Given a list and a reverse index     ║
   │    └──┤ ]p ╟───┤  ╔═══╗  ┌───╖  │  ┌───╖  │      ║  (0 indicates the end of the list,    ║
   │       ╘═╤══╝   │  ║ 0 ╟──┤ ≥ ╟──┴──┤ ≥ ╟──┘      ║  (1 the beginning of the last         ║
   │         │      │  ╚═══╝  ╘═╤═╝     ╘═╤═╝         ║  element, etc.), returns the          ║
   │ ┌───╖ ┌───╖    │           └─────┬───┘           ║  corresponding forward index and      ║
   └─┤ − ╟─┤ + ╟────┘                ┌┴┐              ║  bit offset.                          ║
     ╘═══╝ ╘═╤═╝                     └┬┘              ╟───────────────────────────────────────╢
             │                      ┌─┴─╖             ║                 LIST                  ║
   ╓┬───╖    └──────────────────────┤ ? ╟──           ║                  ↓                    ║
   ╟┘]p ╟──────────────┐            ╘═╤═╝             ║                ┌─┴─╖                  ║
   ╙─┬──╜ ┌────────────┴──────────┐ ╔═╧══╗            ║     REVERSE → ─┤ ] ╟─ → (index of     ║
     │  ┌─┴─╖                     │ ║ −1 ║            ║     INDEX      ╘═╤═╝     element)     ║
  ┌──┴──┤ · ╟───────────┐         │ ╚════╝            ║                  ↓                    ║
  │     ╘═╤═╝           │       ┌─┴───┐               ║             (bit offset)              ║
  │     ┌─┴─╖       ┌─┐ │     ┌─┴─╖   │               ╟──────┬────────────────────────────────╢
  │  ┌──┤ · ╟─────┐ └─┤ │   ┌─┤ ? ╟── │               ║  ]p  │  reverse indexing (helper)     ║
  │  │  ╘═╤═╝   ┌─┴─╖ │ │   │ ╘═╤═╝   │               ╟──────┴────────────────────────────────╢
  │  │    └─────┤ ʜ ╟─┘ │ ┌─┴─╖ │     │               ║          REVERSE INDEX                ║
  │  │          ╘═╤═╝   │ │ ♯ ║ │     │               ║                ↓                      ║
  │  │   ┌───╖  ┌─┴──╖  │ ╘═╤═╝ │     │               ║              ┌─┴──╖                   ║
  │  └───┤ + ╟──┤ ]p ╟──┘   │ ╔═╧═╗   │               ║      LIST → ─┤ ]p ╟─ → (length        ║
  │      ╘═╤═╝  ╘═╤══╝      │ ║ 0 ║   │               ║              ╘═╤══╝     of list)      ║
  │  ╔═══╗ │      └─────────┤ ╚═══╝   │               ║                ↓                      ║
  │  ║ 0 ║ │              ┌─┴─╖       │               ║           (bit offset)                ║
  │  ╚═╤═╝ │        ┌─────┤ ≤ ║       │               ╚═══════════════════════════════════════╝
  │    │ ┌─┴─╖  ┌┐  │     ╘═╤═╝       │
  │    └─┤ ? ╟──┤├──┤     ┌─┴─╖       │
  │      ╘═╤═╝  └┘  └─────┤ · ╟───────┘
  │        │              ╘═╤═╝
  └─────────────────────────┘

                                                           ┌────────────┐
         ╔═════════════════════════════════╗             ┌─┴─╖   ╓───╖  │
         ║                ᴚ                ║        ┌────┤ ] ╟───╢ ᴚ ╟──┘
         ╟─────────────────────────────────╢        │    ╘═╤═╝   ╙───╜
         ║  bit offset from reverse index  ║             ┌─┤
         ╚═════════════════════════════════╝             └─┘

                                                         ╔═══╗
         ╔════════════════════════════════════╗          ║ 0 ╟───┐
         ║                  ʟ                 ║          ╚═══╝ ┌─┴─╖   ╓───╖
         ╟────────────────────────────────────╢           ┌─┬──┤ ] ╟───╢ ʟ ║
         ║  list length (number of elements)  ║           └─┘  ╘═╤═╝   ╙───╜
         ╚════════════════════════════════════╝                  │
                                                                ─┘

                                                      ┌───────────────┐
         ╔══════════════════════════════╗             │  ┌───╖        │
         ║               ɜ              ║        ┌─┐  └──┤ ᴚ ╟──┐   ╓─┴─╖
         ╟──────────────────────────────╢        └─┤     ╘═╤═╝  │   ║ ɜ ║
         ║  element from reverse index  ║        ┌─┴─╖  ┌──┴─╖  │   ╙─┬─╜
         ╚══════════════════════════════╝        │ ‹ ╟──┤ >> ║  ├─────┘
                                                 ╘═╤═╝  ╘═╤══╝  │
                                                   └─     └─────┘

         ╔═════════════════════╗                   ╓───╖
         ║          ʙ          ║                   ║ ʙ ║
         ╟─────────────────────╢     ╔═══╗  ┌───╖  ╙─┬─╜                   ╓───╖
         ║  list bit length    ║     ║ 0 ╟──┤ ᴚ ╟────┘                     ║ » ║
         ║  (of all elements)  ║     ╚═══╝  ╘═╤═╝                          ╙─┬─╜
         ╚═════════════════════╝              │                   ┌──────────┴──────────┐
                                                              ┌───┴───┐       ┌────┐    │
                    ╔═════╤══════════════════════╗          ┌─┴─╖     │       │   ┌┴┐   │
                    ║  »  │  pop                 ║          │ ᴚ ╟─┐ ┌─┴──╖  ┌─┴─╖ └┬┘   │
                    ╟─────┴──────────────────────╢          ╘═╤═╝ │ │ >> ╟──┤ ‹ ║  ├──  │
                    ║  Splits a list into foot   ║          ╔═╧═╗ │ ╘═╤══╝  ╘═╤═╝ ┌┴┐   │
                    ║  (last element) and leg    ║          ║ 1 ║ │   │       │   └┬┘   │
                    ║  (rest of the list)        ║   ╔════╗ ╚═══╝ └───┤       └────┘    │
                    ╟────────────────────────────╢   ║ −1 ╟───┐       │      ┌───────┬──┘
                    ║            LIST            ║   ╚════╝ ┌─┴──╖  ┌─┴─╖  ┌─┴─┐    ┌┴┐
                    ║             ↓              ║          │ << ╟──┤ · ╟──┤   │    └┬┘
                    ║           ┌─┴─╖            ║          ╘═╤══╝  ╘═╤═╝  └───┘    ─┘
                    ║  (leg) ← ─┤ » ╟─ → (foot)  ║            └───────┘
                    ║           ╘═══╝            ║
                    ╚════════════════════════════╝         ┌──────────────────┐
                                                           │                ┌─┴─╖
                                                           │      ┌─────────┤ · ╟──┐
                                                           │      │  ╓───╖  ╘═╤═╝  │
                                                           │      └──╢ ᴇ ╟────┘    │
                          ╔═════╤═══════════════════╗      │         ╙─┬─╜  ┌───╖  │
                          ║  ᴇ  │  extract sublist  ║      │  ┌───╖    └────┤ ʁ ╟──┴─┐
                          ╟─────┴───────────────────╢      └──┤ ʁ ╟──────┐  ╘═╤═╝    │
                          ║  Returns sublist given  ║         ╘═╤═╝      │  ┌─┴──╖   │
                          ║  index and length       ║        ┌──┴─╖      ├──┤ >> ║   │
                          ╚═════════════════════════╝    ┌───┤ << ║   ┌──┤  ╘═╤══╝   │
                                                         │   ╘══╤═╝  ┌┴┐ │    └──────┘
                                                        ┌┴┐  ╔══╧═╗  └┬┘ │
                                                        └┬┘  ║ −1 ║   │  │
                                                         │   ╚════╝      │
                                                         └───────────────┘
                                    ╓───╖
               ┌────────────────────╢ ʀ ╟──────────────┐
               │    ┌───────┐       ╙───╜              │
               │ ╔══╧═╗  ┌──┴─╖  ┌───┐  ╔════╗  ┌───╖  │
               │ ║ −1 ║  │ << ╟──┤   │  ║ −1 ╟──┤ + ╟──┴─┐
               │ ╚════╝  ╘══╤═╝  └─┬─┘  ╚════╝  ╘═╤═╝    │     ╔══════════════════╗
               │        ┌───┤    ┌─┴─╖  ┌────╖  ┌─┴─╖    │     ║        ʀ         ║
               │        │   └────┤ · ╟──┤ << ╟──┤ ʀ ║    │     ╟──────────────────╢
               │        │        ╘═╤═╝  ╘═╤══╝  ╘═╤═╝    │     ║  remove element  ║
               │      ┌─┴─╖        │     ┌┴┐      │      │     ║     at index     ║
               │   ┌──┤ · ╟───┬────┘     └┬┘      │      │     ╚══════════════════╝
               └───┤  ╘═╤═╝   └──────┬────┘       │      │
                   │  ┌─┴─╖ ┌─┐    ┌─┴─╖        ┌─┴─╖    │
                   └──┤ ʜ ╟─┴─┘ ┌──┤ ? ╟────────┤ · ╟────┘
                      ╘═╤═╝     │  ╘═╤═╝        ╘═╤═╝
                        └───────┤    │            │
                                └─────────────────┘

    ╔═════╤════════════════════════════════╗   ╔═════╤═══════════════════════════════╗
    ║  ɢ  │  index of first occurrence     ║   ║  ʛ  │  index of last occurrence of  ║
    ║     │  of element (−1 if not found)  ║   ║     │  element (−1 if not found)    ║
    ╚═════╧════════════════════════════════╝   ╚═════╧═══════════════════════════════╝
                                                       ┌────────────────────────┐
       ╓───╖  ┌────────────────────────┐             ┌─┴─╖                      │
    ┌──╢ ɢ ╟──┤   ┌───╖                │          ┌──┤ · ╟───────────────────┐  │
    │  ╙───╜  └───┤ ɢ ╟─────────┐      │          │  ╘═╤═╝  ╓───╖            │  │
    │             ╘═╤═╝         │      │          │    ├────╢ ʛ ╟──┐         │  │
    │  ┌────────────┴──────┐    │      │          │    │    ╙───╜  │         │  │
    │  │    ┌────┐  ┌───╖  │  ┌─┴─╖  ┌─┴─╖        │  ┌─┴─╖  ┌───╖  │  ┌───╖  │  │
    │  │  ┌─┴─╖  ├──┤ ♯ ╟──┘  │ ‹ ╟──┤ · ╟─┐      └──┤ ‹ ╟──┤ ≠ ╟──┴──┤ ʛ ╟──┘  │
    │  └──┤ ? ╟──┘  ╘═══╝     ╘═╤═╝  ╘═╤═╝ │         ╘═══╝  ╘═╤═╝     ╘═╤═╝     │
    │     ╘═╤═╝               ┌─┴─╖    │   │                  │    ┌────┴──┐    │
    │       └────────┬────────┤ ≠ ║    │   │                  │  ┌─┴─╖   ┌─┴─╖  │
    │               ┌┴┐       ╘═╤═╝    │   │                  │  │ ♯ ║ ┌─┤ ≠ ║  │
    │               └┬┘         └──────┘   │                  │  ╘═╤═╝ │ ╘═╤═╝  │
    │      ╔════╗  ┌─┴─╖                   │                  │  ┌─┴─╖ │ ╔═╧══╗ │
    │      ║ −1 ╟──┤ ? ╟────────┬──────────┘                  └──┤ ? ╟─┘ ║ −1 ║ │
    │      ╚════╝  ╘═╤═╝        │                                ╘═╤═╝   ╚════╝ │
    │                │          │                        ╔════╗  ┌─┴─╖          │
    └───────────────────────────┘                        ║ −1 ╟──┤ ? ╟──────────┘
                                                         ╚════╝  ╘═╤═╝
                                                                   │
           ╓───╖
        ┌──╢ « ╟───────────────┐       ╔═════╤═════════════════════════╗
        │  ╙───╜     ┌────╖  ┌─┴─╖     ║  «  │  list push              ║
        │  ┌───╖  ┌──┤ << ╟──┤ › ║     ╟─────┴─────────────────────────╢
      ┌─┴──┤ ʙ ╟──┘  ╘═╤══╝  ╘═╤═╝     ║  Adds element to end of list  ║
     ┌┴┐   ╘═══╝      ┌┴┐    ╔═╧═╗     ╚═══════════════════════════════╝
     └┬┘              └┬┘    ║ 0 ║
      └──────────┬─────┘     ╚═══╝
                 │

           ┌──────────────────────────────────────────────┐
           │                                ╓───╖  ┌───╖  │
           ├────────────────────────────────╢ ɪ ╟──┤ ʁ ╟──┘       ╔═════╤════════════════╗
           │  ╔════════════╗                ╙─┬─╜  ╘═╤═╝          ║  ᴙ  │  reverse list  ║
           │  ║     ɪ      ║                ┌─┴─╖    │            ╚═════╧════════════════╝
           │  ╟────────────╢      ┌─────────┤ · ╟──┐ │               ╓┬───╖
           │  ║  Inserts   ║      │         ╘═╤═╝  │ │        ┌──────╫┘ᴙp ╟──────┐
           │  ║  element   ║      │ ┌────╖  ┌─┴─╖  │ │     ┌──┴─┐    ╙────╜    ┌─┴──┐  ╓───╖
           │  ║  at index  ║   ┌──┴─┤ << ╟──┤ › ║  ├─┘     │  ┌─┴─╖  ┌────╖  ┌─┴─╖  │  ║ ᴙ ║
           │  ╚════════════╝ ┌─┴──╖ ╘═╤══╝  ╘═╤═╝  │       │  │ › ╟──┤ ᴙp ╟──┤ ‹ ╟┐ │  ╙─┬─╜
           │             ┌───┤ << ║   │     ┌─┴──╖ │       │  ╘═╤═╝  ╘══╤═╝  ╘═══╝│ │  ┌─┴──╖
           │             │   ╘═╤══╝   │  ┌──┤ >> ╟─┘       │    │     ┌─┴─╖       │ │  │ ᴙp ╟──┐
           │            ┌┴┐  ╔═╧══╗  ┌┴┐ │  ╘════╝         │    └─────┤ · ╟───────┘ │  ╘═╤══╝  │
           │            └┬┘  ║ −1 ║  └┬┘ │                 │          ╘═╤═╝         │  ╔═╧═╗
           │             │   ╚════╝   │  │                 │          ┌─┴─╖         │  ║ 0 ║
           │             ├─────┬──────┘  │                 └──────────┤ ? ╟─────────┘  ╚═══╝
           └─────────────┤     │         │                            ╘═╤═╝
                         └───────────────┘                         └────┘

   ┌────────────────────────┐
   │   ╓───╖              ┌─┴─╖
   ├───╢ ↨ ╟──────────────┤ · ╟──────┐           ╔═════╤══════════════════════╗
   │   ╙─┬─╜       ┌───╖  ╘═╤═╝    ┌─┴─╖         ║  ↨  │  replace element     ║
   │     └─────────┤ ʁ ╟────┘   ┌──┤ · ╟─────┐   ╟─────┴──────────────────────╢
   │               ╘═╤═╝      ┌─┘  ╘═╤═╝     │   ║     Changes the element    ║
   │         ┌───────┴────────┘    ┌─┴─╖     │   ║   at a specific index to   ║
   │         │                   ┌─┤ › ║     │   ║        a new element       ║
   │         │         ┌────╖  ┌─┘ ╘═╤═╝     │   ╟────────────────────────────╢
   │   ┌─────┴─────────┤ << ╟──┘    ┌┴┐      │   ║             INDEX          ║
   │ ┌─┴─╖             ╘═╤══╝       └┬┘      │   ║               ↓            ║
  ┌┴─┤ · ╟──────────┐   ┌┴┐   ╔═══╗  │       │   ║             ┌─┴─╖          ║
  │  ╘═╤═╝ ╔════╗   │   └┬┘   ║ 0 ║  │       │   ║      NEW → ─┤ ᴀ ╟─ ← LIST  ║
  │    │   ║ −1 ║   ├───┬┘    ╚═╤═╝  │       │   ║  ELEMENT    ╘═╤═╝          ║
  │    │   ╚═╤══╝  ┌┴┐  │       ├────┴────┐  │   ║               ↓            ║
  │    │  ┌──┴─╖   └┬┘          │  ┌───╖  │  │   ║          (new list)        ║
  │    │  │ << ╟────┘           └──┤ ‹ ╟──┘  │   ╚════════════════════════════╝
  │    │  ╘══╤═╝                   ╘═╤═╝     │
  │    └─────┘                    ┌──┴─╖     │
  └───────────────────────────────┤ >> ╟─────┘
                                  ╘════╝

    ╔═════╤══════════════════════════════╗        ╔═════╤══════════════════════════╗
    ║  ꜰ  │  filter                      ║        ║  ᴍ  │  map                     ║
    ╟─────┴──────────────────────────────╢        ╟─────┴──────────────────────────╢
    ║  Returns a new list containing     ║        ║  Returns a new list contain-   ║
    ║  only those elements that match a  ║        ║  ing the results of passing    ║
    ║  predicate provided as a lambda    ║        ║  every element through the     ║
    ╚════════════════════════════════════╝        ║  provided lambda function      ║
       ┌────────────────────────────┐             ╚════════════════════════════════╝
       │             ┌───╖ ┌───┐    │                          ┌──────────────┐
       │     ┌───────┤ ‹ ╟─┘ ┌─┴─╖  │           ┌──────────────┴─────┐      ╓─┴─╖
       │ ┌───┴─────┐ ╘═╤═╝   │ ꜰ ╟──┤           │   ┌───┬─┐          │      ║ ᴍ ║
       │ │         │   └─┐   ╘═╤═╝  │           │ ┌─┴─╖ └─┘  ┌───╖ ┌─┴─╖    ╙─┬─╜
       │ │ ┌───╖ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖  │           └─┤   ╟──────┤ › ╟─┤ ᴍ ║   ┌──┴──┐
       │ └─┤ › ╟─┤ · ╟─┤ · ╟─┤ · ╟──┘             └─┬─╜      ╘═╤═╝ ╘═╤═╝ ┌─┴─╖ ┌─┴─╖
       │   ╘═╤═╝ ╘═╤═╝ ╘═╤═╝ ╘═╤═╝                  │  ╔═══╗ ┌─┴─╖   └───┤ · ╟─┤ ‹ ╟─┐
       │   ┌─┴─╖ ┌─┴─╖ ┌─┴─╖   ├────┐               │  ║ 0 ╟─┤ ? ╟───┐   ╘═╤═╝ ╘═══╝ │
       └───┤ ? ╟─┤   ╟─┤ · ╟───┘  ╓─┴─╖             │  ╚═══╝ ╘═╤═╝   └─────┘         │
           ╘═╤═╝ └─┬─╜ ╘═╤═╝      ║ ꜰ ║             │          └───                  │
     ╔═══╗ ┌─┴─╖   ├─┐   │        ╙─┬─╜             └────────────────────────────────┘
     ║ 0 ╟─┤ ? ╟─┐ └─┘   ├──────────┘
     ╚═══╝ ╘═╤═╝ └───────┘
             └──

     ┌───────────────┐                   ╔═════╤═══════════════════════════╗
     │             ┌─┴─╖                 ║  ᴀ  │  aggregate (fold left)    ║
     │   ┌─────────┤ · ╟──────────┐      ╟─────┴───────────────────────────╢
     │   │ ╓───╖   ╘═╤═╝          │      ║  Starts with an initial value,  ║
     │   └─╢ ᴀ ╟─────┤            │      ║  iteratively applies the        ║
     │     ╙─┬─╜   ┌─┴─╖          │      ║  provided lambda to every list  ║
     │   ┌───┴─────┤   ╟──┐       │      ║  element and the previous       ║
     │   │         └─┬─╜  │  ┌─┐  │      ║  result and returns the final   ║
     │   │         ┌─┴─╖  ├──┴─┘  │      ║  result                         ║
     │   │     ┌───┤   ╟──┘       │      ╟─────────────────────────────────╢
     │   │     │   └─┬─╜          │      ║          INITIAL VALUE          ║
     │ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖          │      ║                 ↓               ║
     └─┤ · ╟─┤ · ╟─┤ ᴀ ╟───┐      │      ║               ┌─┴─╖             ║
       ╘═╤═╝ ╘═╤═╝ ╘═╤═╝ ┌─┴─╖    │      ║     LAMBDA → ─┤ ᴀ ╟─ ← LIST     ║
         │     │     │   │ ‹ ╟─┐  │      ║               ╘═╤═╝             ║
         │     │   ┌─┴─╖ ╘═╤═╝ ├──┘      ║                 ↓               ║
         │     └───┤ · ╟───┘   │         ║          (final value)          ║
         │         ╘═╤═╝       │         ╚═════════════════════════════════╝
         │         ┌─┴─╖       │
         └─────────┤ ? ╟───────┘
                   ╘═╤═╝
                     │
  ╔═════╤═════════════════════════════════════╗   ╔═════╤═════════════════════════════════════╗
  ║  →  │  index of                           ║   ║  ←  │  last index of                      ║
  ╟─────┴─────────────────────────────────────╢   ╟─────┴─────────────────────────────────────╢
  ║  Returns the index of the first element   ║   ║  Returns the index of the last element    ║
  ║  that satisfies a provided predicate      ║   ║  that satisfies a provided predicate      ║
  ║  (−1 if not found).                       ║   ║  (−1 if not found).                       ║
  ╚═══════════════════════════════════════════╝   ╚═══════════════════════════════════════════╝

                 ╓───╖           ┌──────┐               ╓───╖    ┌────────────────────┐
    ┌────────────╢ → ╟───────────┤      │           ┌───╢ ← ╟────┤ ┌─┐                │
    │            ╙───╜         ┌─┴─╖    │           │   ╙───╜    │ └─┤  ┌──────┐      │
    │       ┌──────────────────┤ → ║    │           │          ┌─┴─╖ │┌─┴─╖    │    ┌─┴─╖
    │       │                  ╘═╤═╝    │           │ ┌────────┤   ╟─┘│ ♯ ║    ├────┤ ← ║
    │       │    ┌───╖  ╔═══╗  ┌─┴─╖  ┌─┴─╖         │ │        └─┬─╜  ╘═╤═╝    │    ╘═╤═╝
    │  ┌────┴────┤ ≤ ╟──╢ 0 ║  │ ‹ ╟──┤ · ╟──┐      │ │ ╔═══╗  ┌─┴─╖  ┌─┴─╖  ┌─┴─╖  ┌─┴─╖
    │  │         ╘═╤═╝  ╚═╤═╝  ╘═╤═╝  ╘═╤═╝  │      │ │ ║ 0 ╟──┤ ? ╟──┤ ? ╟──┤ ≤ ║  │ ‹ ╟───┐
    │  │  ┌───╖  ┌─┴─╖  ┌─┴─╖  ┌─┴─╖    │    │      │ │ ╚═╤═╝  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝   │
    │  └──┤ ♯ ╟──┤ ? ╟──┤ ? ╟──┤   ╟────┘    │      │ │   │   ╔══╧═╗  ┌─┴─╖  ┌─┴─╖  ┌─┴─╖   │
    │     ╘═══╝  ╘═╤═╝  ╘═╤═╝  └─┬─╜ ┌─┐     │      │ │   │   ║ −1 ╟──┤ ? ╟──┤ · ╟──┤ · ╟─┬─┘
    │           ╔══╧═╗  ┌─┴─╖    └───┴─┘     │      │ │   │   ╚════╝  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝ │
    │           ║ −1 ╟──┤ ? ╟────┬───────────┘      │ │   └────────────────────┘      │   │
    │           ╚════╝  ╘═╤═╝    │                  │ └───────────────────────────────┘   │       ╓───╖
    └────────────────────────────┘                  └─────────────────────────────────────┘       ║ ʏ ║
                                                                                                  ╙─┬─╜
                                                                                                ┌───┴───┐
                                              ╓───╖        ╔═════╤══════════════╗             ┌─┴─╖     │
                                         ┌────╢ ᴄ ╟──┐     ║  ʏ  │  flatten     ║    ┌────────┤ ‹ ╟──┐  │
   ╔═════╤═══════════════════════╗    ┌──┴─╖  ╙───╜  │     ╟─────┴──────────────╢    │        ╘═══╝  │  │
   ║  ᴄ  │  concat               ║    │ << ╟┐┌┐      │     ║  Concatenates all  ║    │ ┌───╖  ┌───╖  │  │
   ╟─────┴───────────────────────╢    ╘══╤═╝└┤├┐│    │     ║  the lists in a    ║    └─┤ ʏ ╟──┤ ᴄ ╟──┘  │
   ║  Concatenates two lists     ║     ┌─┴─╖ └┘└┴┐┌┐ │     ║  list of lists     ║      ╘═══╝  ╘═╤═╝     │
   ╚═════════════════════════════╝     │ ʙ ║     └┤├┐│     ╚════════════════════╝      ╔═══╗  ┌─┴─╖     │
                                       ╘═╤═╝      └┘└┴┐                                ║ 0 ╟──┤ ? ╟─────┘
                                         └────────────┘                                ╚═══╝  ╘═╤═╝
                                                                                                │
   ╔═════╤══════════════════════════════════╗    ╔═════╤═════════════════════════════╗
   ║  ᴅ  │  distinct                        ║    ║  ʗ  │  count matches              ║
   ╟─────┴──────────────────────────────────╢    ╟─────┴─────────────────────────────╢
   ║  Removes duplicate elements from a     ║    ║  Returns the number of elements   ║
   ║  list (and keeps the last occurrence   ║    ║  in the provided list that match  ║
   ║  of each duplicate)                    ║    ║  the provided predicate.          ║
   ╚════════════════════════════════════════╝    ╚═══════════════════════════════════╝
                           ┌───┐                        ┌───────────────┬───┐
               ┌───╖ ┌─────┘ ┌─┴─╖                      │ ┌───╖         │   │
             ┌─┤ ‹ ╟─┘ ╓───╖ │ ᴅ ║                      └─┤ ʗ ╟─────┐ ╓─┴─╖ │
             │ ╘═╤═╝   ║ ᴅ ║ ╘═╤═╝                        ╘═╤═╝     │ ║ ʗ ║ │
             │   └───┐ ╙─┬─╜ ┌─┴─╖                        ┌─┴─┐     │ ╙─┬─╜ │
             │ ┌───╖ └───┴───┤ · ╟─────┐                  │ ┌─┴─╖ ┌─┴─╖ │ ┌─┴─╖
           ┌─┴─┤ › ╟────────┐╘═╤═╝     │                  │ │ ♯ ║ │ ‹ ╟─┴─┤ · ╟──┐
           │   ╘═╤═╝        ├──┴───┐   │                  │ ╘═╤═╝ ╘═╤═╝   ╘═╤═╝  │
           │   ┌─┴─╖  ┌───╖ │      │   │                  │ ┌─┴─╖ ┌─┴─╖     │    │
           └───┤ · ╟──┤ ɢ ╟─┘      │   │                  └─┤ ? ╟─┤   ╟─────┘    │
               ╘═╤═╝  ╘═╤═╝        │   │                    ╘═╤═╝ └─┬─╜ ┌─┐      │
               ┌─┴─╖  ┌─┴─╖ ╔════╗ │   │             ╔═══╗  ┌─┴─╖   └───┴─┘      │
      ┌────────┤ ? ╟──┤ = ║ ║ −1 ║ │   │             ║ 0 ╟──┤ ? ╟────────────────┘
      │        ╘═╤═╝  ╘═╤═╝ ╚═╤══╝ │   │             ╚═══╝  ╘═╤═╝
      │ ╔═══╗  ┌─┴─╖    └─────┘  ┌─┴─╖ │                      └──
      │ ║ 0 ╟──┤ ? ╟─────────────┤ · ╟─┘
      │ ╚═══╝  ╘═╤═╝             ╘═╤═╝
      └────────────────────────────┘

