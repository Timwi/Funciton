﻿

         ╔══════════════╤═══════════════════╤═════════════════════╗             ╓───╖
         ║  list shift  │  ‹(l) = ‹p(l, 0)  │      LIST           ║             ║ ‹ ║
         ╟──────────────┴─────────────┬─────┘       ↓             ║             ╙─┬─╜
         ║  Splits a list into head   │           ┌─┴─╖           ║       ╔═══╗ ┌─┴──╖
         ║  (first element) and tail  │  (tail) ← ┤ ‹ ╟ → (head)  ║       ║ 0 ╟─┤ ‹p ╟─
         ║  (rest of list)            │           ╘═══╝           ║       ╚═══╝ ╘═╤══╝
         ╚════════════════════════════╧═══════════════════════════╝             ──┘

      ┌───────────────────────────────┐
      │                       ╓┬────╖ │                               ╔═════════════════════════════════╗
      │                       ╟┘ ‹p ╟─┘                               ║  list shift (helper)            ║
      │ ╔═══╗ ┌────╖          ╙──┬──╜                                 ╟─────────────────────────────────╢
      │ ║ 1 ╟─┤ >> ╟─────────────┴────────┐                           ║  ‹p(l, a) =                     ║
      │ ╚═══╝ ╘══╤═╝    ╔════╗ ┌────╖     │                           ║    let b = (l >> 1) & 2097151;  ║
      │          │      ║ 22 ╟─┤ >> ╟─────┴─────────┐                 ║    let m = a << 21;             ║
      │          │      ╚════╝ ╘═╤══╝               │                 ║    let h = m | b;               ║
      │          │   ╔═════════╗ │ ╔════╗ ┌────╖    │         ╔═══╗   ║    let c = l >> 22;             ║
      │          └─┬─╢ 2097151 ║ │ ║ 23 ╟─┤ >> ╟────┴──────┬──╢ 1 ║   ║    let t = l >> 23;             ║
      │            │ ╚═════════╝ │ ╚════╝ ╘═╤══╝           │  ╚═══╝   ║    let (u, i) = ‹p(c, h);       ║
    ┌─┴──╖         │         ┌───┴──┐       │              │          ║    let n = l & 1;               ║
    │ << ╟────┐    │       ┌─┴─╖  ┌─┴──╖  ┌─┴─╖            │          ║    [n ? t : u,                  ║
    ╘═╤══╝   ┌┴┐   │  ┌────┤ · ╟──┤ ‹p ╟──┤ · ╟───────┐   ┌┴┐         ║     n ? (c & 1 ? ¬h : h) : i]   ║
    ╔═╧══╗   └┬┘   │  │    ╘═╤═╝  ╘═╤══╝  ╘═╤═╝       │   └┬┘         ╟─────────────────────────────────╢
    ║ 21 ║    └──┬─┘  │      │      │     ┌─┴─╖       │    │          ║              l                  ║
    ╚════╝       │    │      ├────┐ └─────┤ ? ╟──┐  ┌─┴─╖  │          ║              ↓                  ║
                 └────┤    ╔═╧═╗  └──┐    ╘═╤═╝  ├──┤ · ╟──┘          ║            ┌─┴──╖               ║
                      │    ║ 1 ║    ┌┴┐          │  ╘═╤═╝             ║        a → ┤ ‹p ╟ → (head)      ║
                      │    ╚═══╝    └┬┘     ┌────┘    │               ║            ╘═╤══╝               ║
                      │    ┌───┐   ┌─┴─╖  ┌─┴─╖       │               ║              ↓                  ║
                    ┌─┴────┤   ├───┤ ? ╟──┤ ? ╟──     │               ║            (tail)               ║
                    │      └───┘   ╘═╤═╝  ╘═╤═╝       │               ╚═════════════════════════════════╝
                    └────────────────┘      └─────────┘


   ╔═════════════════════╤════════════════════╤══════════════════════════════╗            ╓───╖
   ║  list shift with    │  ꜱ(l) = ꜱp(l, 0)   │           LIST               ║            ║ ꜱ ║
   ║  bitlength of head  ├───────────────────┬┘            ↓                 ║            ╙─┬─╜
   ╟─────────────────────┘  Splits a list    │           ┌─┴─╖               ║     ╔═══╗  ┌─┴──╖
   ║  into the bitlength of the head (first  │  (tail) ← ┤ ꜱ ╟ → (bitlength  ║     ║ 0 ╟──┤ ꜱp ╟───┐
   ║  element) and the tail (rest of list)   │           ╘═══╝     of head)  ║     ╚═══╝  ╘═╤══╝   │
   ╚═════════════════════════════════════════╧═══════════════════════════════╝              └──  ──┘


       ┌─────────────────────────────┐              ╔════════════════════════════════╗
       │                    ╓┬────╖  │              ║  list shift with bitlength     ║
       │                    ╟┘ ꜱp ╟──┘              ╟───────────────────┐  of head   ║
       │                    ╙──┬──╜                 ║  ꜱp(l, a) =       │  (helper)  ║
       │  ╔════╗  ┌───╖      ┌─┴─╖                  ║    let n = l & 1; └────────────╢
       │  ║ 22 ╟──┤ + ╟──────┤ · ╟───┬─────┐        ║    let b = 22 + n;             ║
       │  ╚════╝  ╘═╤═╝      ╘═╤═╝  ┌┴┐    │        ║    let h = a + b;              ║
       │     ┌───╖  │  ┌────╖  │    └┬┘    │        ║    let c = l >> b;             ║
       └─────┤ + ╟──┴──┤ >> ╟──┴─────┴┐    │        ║    let (i, u) = ꜱp(c, h);      ║
             ╘═╤═╝     ╘══╤═╝       ╔═╧═╗  │        ║    [n ? h : i,                 ║
               │    ┌─────┴──────┐  ║ 1 ║  │        ║     n ? c : u]                 ║
               │  ┌─┴──╖         │  ╚═══╝  │        ╟────────────────────────────────╢
             ┌─┴──┤ ꜱp ╟──────┐  │    ┌────┴─┐      ║              l                 ║
             │    ╘═╤══╝      │  │  ┌─┴─╖    │      ║              ↓                 ║
             │    ┌─┴─╖       │  └──┤ ? ╟──  │      ║            ┌─┴──╖              ║
             └────┤ · ╟────┐  │     ╘═╤═╝    │      ║        a → ┤ ꜱp ╟ → (tail)     ║
                  ╘═╤═╝    │  └───────┘      │      ║            ╘═╤══╝              ║
                    │    ┌─┴─╖               │      ║              ↓                 ║
                    └────┤ ? ╟───────────────┘      ║      (head bitlength)          ║
                         ╘═╤═╝                      ╚════════════════════════════════╝
                           │


        ┌──────┐ ╓───╖
     ┌──┴────┐ │ ║ [ ╟───────┐                      ╔══════════════════════════════╗
     │       │ │ ╙─┬─╜       │                      ║  indexing                    ║
     │       │ └───┘┌────────┴──────────┐           ╟──────────────────────────────╢
     │     ┌─┴─╖  ┌─┴─╖                 │           ║  Given a list and an index,  ║
     │  ┌──┤ · ╟──┤ · ╟────┐            │           ║  returns the element at the  ║
     │  │  ╘═╤═╝  ╘═╤═╝    │            │           ║  index and the bit-length    ║
     │  │    │    ┌─┴─╖  ┌─┴─╖          │           ║  of all preceding elements   ║
     │  │    └────┤ · ╟──┤ ꜱ ║    ┌─────┴─────┐     ╟──────────────────────────────╢
     │  │         ╘═╤═╝  ╘═╤═╝    │           │     ║  [(l, i) =                   ║
     │  │         ┌─┴─╖  ┌─┴─╖  ┌─┴─╖         │     ║    let (h, _) = ‹(l);        ║
     │  │         │ + ╟──┤ [ ╟──┤ ? ╟──       │     ║    let (j, t) = ꜱ(l);        ║
     │  │         ╘═╤═╝  ╘═╤═╝  ╘═╤═╝         │     ║    let (e, s) = [(t, i−1);   ║
     │  │         ╔═╧══╗   │  ┌───┴───┐       │     ║    [i ? e : h, i ? s+j : 0]  ║
     │  │         ║ −1 ║   │ ┌┴┐      │       │     ╟──────────────────────────────╢
     │  │         ╚════╝   │ └┬┘      │       │     ║           LIST               ║
     │  │         ┌───╖    │  │ ┌───╖ │ ╔═══╗ │     ║            ↓                 ║
     │  └─────────┤ + ╟────┘  └─┤ ‹ ╟─┴─╢ 0 ║ │     ║          ┌─┴─╖               ║
     │            ╘═╤═╝         ╘═╤═╝   ╚═══╝ │     ║  INDEX → ┤ [ ╟ → (element)   ║
     │     ╔═══╗  ┌─┴─╖         ┌─┴─╖         │     ║          ╘═╤═╝               ║
     │     ║ 0 ╟──┤ ? ╟─────────┤ · ╟─────────┘     ║            ↓                 ║
     │     ╚═══╝  ╘═╤═╝         ╘═╤═╝               ║       (preceding             ║
     │              │             │                 ║        bitlength)            ║
     └────────────────────────────┘                 ╚══════════════════════════════╝


                                                                ┌────────────┐
         ╔═════════════════════════════════════╗      ╔═══╗   ┌─┴─╖   ╓───╖  │
         ║  list element                       ║      ║ 0 ╟─┬─┤ [ ╟───╢ ɛ ╟──┘
         ╟─────────────────────────────────────╢      ╚═══╝ │ ╘═╤═╝   ╙───╜
         ║  ɛ(l, i) = let (e, _) = [(l, i); e  ║            └─┬─┘
         ╚═════════════════════════════════════╝             ┌┴┐
                                                             └┬┘

                                                               ┌────────────┐
                                                             ┌─┴─╖   ╓───╖  │
         ╔═════════════════════════════════════╗        ┌────┤ [ ╟───╢ ʁ ╟──┘
         ║  list element bit offset            ║        │    ╘═╤═╝   ╙───╜
         ╟─────────────────────────────────────╢        └─┬────┤
         ║  ʁ(l, i) = let (_, b) = [(l, i); b  ║         ┌┴┐ ╔═╧═╗
         ╚═════════════════════════════════════╝         └┬┘ ║ 0 ║
                                                             ╚═══╝

   ┌───────────┐                                      ╔═══════════════════════════════════════╗
   │  ╓───╖    │    ┌──────────────────────────┐      ║  reverse indexing                     ║
   │  ║ ] ╟────┤  ┌─┴─╖                        │      ╟───────────────────────────────────────╢
   │  ╙─┬─╜  ┌─┴──┤ · ╟──────────────┐         │      ║  Given a list and a reverse index     ║
   │    │  ┌─┴──╖ ╘═╤═╝              │         │      ║  (0 indicates the end of the list,    ║
   │    └──┤ ]p ╟───┤  ╔═══╗  ┌───╖  │  ┌───╖  │      ║  (1 the beginning of the last         ║
   │       ╘═╤══╝   │  ║ 0 ╟──┤ ≥ ╟──┴──┤ ≥ ╟──┘      ║  element, etc.), returns the          ║
   │         │      │  ╚═══╝  ╘═╤═╝     ╘═╤═╝         ║  corresponding forward index and      ║
   │       ┌───╖    │           └─────┬───┘           ║  bit offset.                          ║
   └───────┤ − ╟────┘                ┌┴┐              ╟───────────────────────────────────────╢
           ╘═╤═╝                     └┬┘              ║  ](l, i) = let (b, j) = ]p(l, i);     ║
             │                      ┌─┴─╖             ║    [b, (i ≥ 0) & (j ≥ i) ? j−i : −1]  ║
   ╓┬───╖    └──────────────────────┤ ? ╟─            ╟───────────────────────────────────────╢
   ╟┘]p ╟──────────────┐            ╘═╤═╝             ║                 LIST                  ║
   ╙─┬──╜ ┌────────────┴──────────┐ ╔═╧══╗            ║                  ↓                    ║
     │  ┌─┴─╖                     │ ║ −1 ║            ║                ┌─┴─╖                  ║
  ┌──┴──┤ · ╟───────────┐         │ ╚════╝            ║      REVERSE → ┤ ] ╟ → (index of      ║
  │     ╘═╤═╝           │       ┌─┴──┐                ║      INDEX     ╘═╤═╝    element)      ║
  │     ┌─┴─╖           │     ┌─┴─╖  │                ║                  ↓                    ║
  │  ┌──┤ · ╟─────┐     │   ┌─┤ ? ╟─ │                ║             (bit offset)              ║
  │  │  ╘═╤═╝   ┌─┴─╖   │   │ ╘═╤═╝  │                ╟───────────────────────────────────────╢
  │  │    └─────┤ ꜱ ║   │ ┌─┴─╖ │    │                ║  ]p(l, i) = let (h, t) = ꜱ(l);        ║
  │  │          ╘═╤═╝   │ │ ♯ ║ │    │                ║             let (b, j) = ]p(t, i);    ║
  │  │   ┌───╖  ┌─┴──╖  │ ╘═╤═╝ │    │                ║             [l & (j ≥ i) ? b+h : 0,   ║
  │  └───┤ + ╟──┤ ]p ╟──┘   │ ╔═╧═╗  │                ║              l ? ♯(j) : 0]            ║
  │      ╘═╤═╝  ╘═╤══╝      │ ║ 0 ║  │                ╟───────────────────────────────────────╢
  │  ╔═══╗ │      └─────────┤ ╚═══╝  │                ║          REVERSE INDEX                ║
  │  ║ 0 ║ │              ┌─┴─╖      │                ║                ↓                      ║
  │  ╚═╤═╝ │        ┌─────┤ ≤ ║      │                ║              ┌─┴──╖                   ║
  │    │ ┌─┴─╖  ┌┐  │     ╘═╤═╝      │                ║       LIST → ┤ ]p ╟ → (length         ║
  │    └─┤ ? ╟──┤├──┤     ┌─┴─╖      │                ║              ╘═╤══╝    of list)       ║
  │      ╘═╤═╝  └┘  └─────┤ · ╟──────┘                ║                ↓                      ║
  │        │              ╘═╤═╝                       ║           (bit offset)                ║
  └─────────────────────────┘                         ╚═══════════════════════════════════════╝

                                                               ┌────────────┐
         ╔═════════════════════════════════════╗             ┌─┴─╖   ╓───╖  │
         ║  bit offset from reverse index      ║        ┌────┤ ] ╟───╢ ᴚ ╟──┘
         ╟─────────────────────────────────────╢        │    ╘═╤═╝   ╙───╜
         ║  ᴚ(l, i) = let (b, _) = ](l, i); b  ║             ┌─┤
         ╚═════════════════════════════════════╝             └─┘

                                                         ╔═══╗
         ╔════════════════════════════════════╗          ║ 0 ╟───┐
         ║  list length (number of elements)  ║          ╚═══╝ ┌─┴─╖   ╓───╖
         ╟────────────────────────────────────╢           ┌─┬──┤ ] ╟───╢ ʟ ║
         ║  ʟ(l) = let (_, r) = ](l, 0); r    ║           └─┘  ╘═╤═╝   ╙───╜
         ╚════════════════════════════════════╝                  │
                                                                ─┘

         ╔════════════════════════════════════╗             ┌───────────────┐
         ║  element from reverse index        ║             │  ┌───╖        │
         ╟────────────────────────────────────╢        ┌─┐  └──┤ ᴚ ╟──┐   ╓─┴─╖
         ║  ɜ(l, i) =                         ║        └─┤     ╘═╤═╝  │   ║ ɜ ║
         ║    let (h, _) = ‹(l >> ᴚ(l, i));   ║        ┌─┴─╖  ┌──┴─╖  │   ╙─┬─╜
         ║    h                               ║        │ ‹ ╟──┤ >> ║  ├─────┘
         ╚════════════════════════════════════╝        ╘═╤═╝  ╘═╤══╝  │
                                                         └─     └─────┘

         ╔═════════════════════╗                   ╓───╖
         ║  list bit length    ║                   ║ ʙ ║
         ║  (of all elements)  ║     ╔═══╗  ┌───╖  ╙─┬─╜                   ╓───╖
         ╟─────────────────────╢     ║ 0 ╟──┤ ᴚ ╟────┘                     ║ » ║
         ║  ʙ(l) = ᴚ(l, 0)     ║     ╚═══╝  ╘═╤═╝                          ╙─┬─╜
         ╚═════════════════════╝              │                   ┌──────────┴──────────┐
                                                              ┌───┴───┐       ┌────┐    │
                                                            ┌─┴─╖     │       │   ┌┴┐   │
         ╔═══════════════════════════════════════╗          │ ᴚ ╟─┐ ┌─┴──╖  ┌─┴─╖ └┬┘   │
         ║  pop                                  ║          ╘═╤═╝ │ │ >> ╟──┤ ‹ ║  ├──  │
         ╟───────────────────────────────────────╢          ╔═╧═╗ │ ╘═╤══╝  ╘═╤═╝ ┌┴┐   │
         ║  Splits a list into foot (last        ║          ║ 1 ║ │   │       │   └┬┘   │
         ║  element) and leg (rest of the list)  ║   ╔════╗ ╚═══╝ └───┤       └────┘    │
         ╟───────────────────────────────────────╢   ║ −1 ╟───┐       │      ┌───────┬──┘
         ║  »(l) = let b = ᴚ(l, 1);              ║   ╚════╝ ┌─┴──╖  ┌─┴─╖  ┌─┴─┐    ┌┴┐
         ║         let (f, _) = ‹(l >> b);       ║          │ << ╟──┤ · ╟──┤   │    └┬┘
         ║         [l & ¬(−1 << b), f]           ║          ╘═╤══╝  ╘═╤═╝  └───┘    ─┘
         ╚═══════════════════════════════════════╝            └───────┘

                                                           ┌──────────────────┐
                                                           │                ┌─┴─╖
         ╔════════════════════════════════╗                │      ┌─────────┤ · ╟──┐
         ║  extract sublist               ║                │      │  ╓───╖  ╘═╤═╝  │
         ╟────────────────────────────────╢                │      └──╢ ᴇ ╟────┘    │
         ║  Returns the sublist starting  ║                │         ╙─┬─╜  ┌───╖  │
         ║  at index i and of length c    ║                │  ┌───╖    └────┤ ʁ ╟──┴─┐
         ╟────────────────────────────────╢                └──┤ ʁ ╟──────┐  ╘═╤═╝    │
         ║  ᴇ(l, i, c) =                  ║                   ╘═╤═╝      │  ┌─┴──╖   │
         ║    let m = l >> ʁ(l, i);       ║                  ┌──┴─╖      ├──┤ >> ║   │
         ║    ¬(−1 << ʁ(m, c)) & m        ║              ┌───┤ << ║   ┌──┤  ╘═╤══╝   │
         ╚════════════════════════════════╝              │   ╘══╤═╝  ┌┴┐ │    └──────┘
                                                        ┌┴┐  ╔══╧═╗  └┬┘ │
                                                        └┬┘  ║ −1 ║   │  │
                                                         │   ╚════╝      │
                                                         └───────────────┘

                    ╔═══════════════════════════════════════════════╗
                    ║  remove element at index                      ║
                    ╟───────────────────────────────────────────────╢
                    ║  ʀ(l, i) =                                    ║
                    ║    let (h, t) = ꜱ(l);                         ║
                    ║    i ? l & ¬(−1 << h) | (ʀ(t, i−1) << h) : t  ║
                    ╚═══════════════════════════════════════════════╝

                                            ╓───╖
                       ┌────────────────────╢ ʀ ╟──────────────┐
                       │    ┌───────┐       ╙───╜              │
                       │ ╔══╧═╗  ┌──┴─╖  ┌───┐  ╔════╗  ┌───╖  │
                       │ ║ −1 ║  │ << ╟──┤   │  ║ −1 ╟──┤ + ╟──┴─┐
                       │ ╚════╝  ╘══╤═╝  └─┬─┘  ╚════╝  ╘═╤═╝    │
                       │         ┌──┤    ┌─┴─╖  ┌────╖  ┌─┴─╖    │
                       │         │  └────┤ · ╟──┤ << ╟──┤ ʀ ║    │
                       │         │       ╘═╤═╝  ╘═╤══╝  ╘═╤═╝    │
                       │       ┌─┴─╖       │     ┌┴┐      │      │
                       │    ┌──┤ · ╟──┬────┘     └┬┘      │      │
                       └────┤  ╘═╤═╝  └──────┬────┘       │      │
                            │  ┌─┴─╖       ┌─┴─╖        ┌─┴─╖    │
                            └──┤ ꜱ ║    ┌──┤ ? ╟────────┤ · ╟────┘
                               ╘═╤═╝    │  ╘═╤═╝        ╘═╤═╝
                                 └──────┤    │            │
                                        └─────────────────┘

      ╔════════════════════════════════╗         ╔═══════════════════════════════╗
      ║  index of first occurrence     ║         ║  index of last occurrence of  ║
      ║  of element (−1 if not found)  ║         ║  element (−1 if not found)    ║
      ╟────────────────────────────────╢         ╟───────────────────────────────╢
      ║  ɢ(l, e) =                     ║         ║  ʛ(l, e) =                    ║
      ║    let (h, t) = ‹(l);          ║         ║    let (h, t) = ‹(l);         ║
      ║    let i = ɢ(t, e);            ║         ║    let i = ʛ(t, e);           ║
      ║    let j = ♯(i);               ║         ║    l ? i ≠ −1 ? ♯(i)          ║
      ║    l ? (h ≠ e) & (j ? j : i)   ║         ║      : (h ≠ e) : −1           ║
      ║      : −1                      ║         ╚═══════════════════════════════╝
      ╚════════════════════════════════╝
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


    ╔═══════════════════════════════════════════════╤═══════════════════════════╗
    ║  unshift (insert element at front of list)    │                           ║
    ║  (returns new list and bitlength of element)  │             LIST          ║
    ╟───────────────────────────────────────────────┤              ↓            ║
    ║  ᴜ(l, e) =                                    │            ┌─┴─╖          ║
    ║    let s = e < 0;                             │  ELEMENT → ┤ ᴜ ╟ → (new   ║
    ║    let f = s ? ¬e : e;                        │  TO ADD    ╘═╤═╝   list)  ║
    ║    let (n, b) = ᴜp(                           │              ↓            ║
    ║      l << 23 | (s ? (1 << 22) + 1 : 1)        │         (bitlength)       ║
    ║              | ((f & ((1 << 21) − 1)) << 1),  │                           ║
    ║      f >> 21); ┌───────────────────────────┐  ├───────────────────────────╢
    ║    [n, b]      │  (1 << 21) − 1 = 2097151  │  │     ELEMENT TO ADD        ║
    ╟────────────────┤  (1 << 22) + 1 = 4194305  ├──┤           ↓               ║
    ║  ᴜp(l, e) =    └───────────────────────────┘  │         ┌─┴──╖            ║
    ║    let (n, b) = ᴜp(                           │  LIST → ┤ ᴜp ╟ → (new     ║
    ║      l << 22 | ((e & ((1 << 21) − 1)) << 1),  │         ╘═╤══╝   list)    ║
    ║      e >> 21);                                │           ↓               ║
    ║    [e ? n : l, e ? b+22 : 23]                 │      (bitlength)          ║
    ╚═══════════════════════════════════════════════╧═══════════════════════════╝

                                                              ┌┐
                                                            ┌─┤├─┐
                 ╓───╖                                   ┌──┤ └┘ │
                 ║ ᴜ ╟───────────────────────────────────┤  │  ┌─┴─╖
                 ╙─┬─╜                     ╔═══╗  ┌───╖  │  └──┤ ? ╟──┐
                   │         ╔═════════╗   ║ 0 ╟──┤ < ╟──┘     ╘═╤═╝  │
                   │         ║ 4194305 ║   ╚═══╝  ╘═╤═╝        ┌─┴─╖  │
                   │         ╚════╤════╝   ┌────────┴──────────┤ · ╟──┘
                   │     ╔═══╗  ┌─┴─╖      │      ╔═════════╗  ╘═╤═╝
                   │     ║ 1 ╟──┤ ? ╟──────┘      ║ 2097151 ╟──┬─┴────┐
                   │     ╚═══╝  ╘═╤═╝╔═══╗  ┌────╖╚═════════╝┌┐│      │
                 ┌─┴──╖  ┌┐     ┌┐│  ║ 1 ╟──┤ << ╟───────────┤├┘      │
                 │ << ╟──┤├──┬──┤├┘  ╚═══╝  ╘═╤══╝   ╔════╗  └┘┌────╖ │
                 ╘═╤══╝  └┘  │┌┐└┘   ┌┐       │      ║ 21 ╟────┤ >> ╟─┘
                 ╔═╧══╗      └┤├──┬──┤├───────┘      ╚════╝    ╘═╤══╝
                 ║ 23 ║       └┘  │  └┘                        ┌─┴──╖
                 ╚════╝           └────────────────────────────┤ ᴜp ╟─
                                                               ╘═╤══╝
                                                                 │

                     ┌────────────────────────────────────────────┐
                     │                              ╓┬───╖        │
                     │                              ╟┘ᴜp ╟────────┤
                     │    ╔═════════╗               ╙─┬──╜        │
                     │    ║ 2097151 ╟─────┬───────────┴───┐       │
                     │    ╚═════════╝    ┌┴┐              │       │
                     │  ╔═══╗  ┌────╖    └┬┘          ┌───┴───┐   │
                     │  ║ 1 ╟──┤ << ╟─────┘           │       │   │
                     │  ╚═══╝  ╘══╤═╝ ╔════╗  ┌────╖  │       │   │
                     │           ┌┴┐  ║ 21 ╟──┤ >> ╟──┴─┐     │   │
                  ┌──┴─╖  ┌┐     └┬┘  ╚════╝  ╘═╤══╝    │     │   │
                  │ << ╟──┤├───┬──┘           ┌─┴──╖  ┌─┴─╖   │   │
                  ╘══╤═╝  └┘   └──────────────┤ ᴜp ╟──┤ ? ╟── │   │
                     │         ╔════╗  ┌───╖  ╘═╤══╝  ╘═╤═╝ ┌─┴─╖ │
                     └─────────╢ 22 ╟──┤ + ╟────┘       └───┤ · ╟─┘
                               ╚════╝  ╘═╤═╝                ╘═╤═╝
                               ╔════╗  ┌─┴─╖                  │
                               ║ 23 ╟──┤ ? ╟──────────────────┘
                               ╚════╝  ╘═╤═╝
                                         │

         ┌───────────┐              ╔═════════════════════════════════════╗
         │  ╓───╖  ┌─┴─╖            ║  unshift (insert element            ║
         └──╢ › ╟──┤ ᴜ ╟───┐        ║  at front of list)                  ║
            ╙───╜  ╘═╤═╝   │        ╟─────────────────────────────────────╢
                     ├─┐            ║  ›(l, e) = let (n, _) = ᴜ(l, e); n  ║
                     └─┘            ╚═════════════════════════════════════╝

           ╓───╖
        ┌──╢ « ╟───────────────┐    ╔═════════════════════════════════════╗
        │  ╙───╜     ┌────╖  ┌─┴─╖  ║  push (add element to end of list)  ║
        │  ┌───╖  ┌──┤ << ╟──┤ › ║  ╟─────────────────────────────────────╢
      ┌─┴──┤ ʙ ╟──┘  ╘═╤══╝  ╘═╤═╝  ║  «(l, e) = l | (›(0, e) << ʙ(l))    ║
     ┌┴┐   ╘═══╝      ┌┴┐    ╔═╧═╗  ╚═════════════════════════════════════╝
     └┬┘              └┬┘    ║ 0 ║
      └──────────┬─────┘     ╚═══╝
                 │

           ┌───────────────────────────────────────────────────────┐
           │                                         ╓───╖  ┌───╖  │
           ├─────────────────────────────────────────╢ ɪ ╟──┤ ʁ ╟──┘
           │  ╔════════════════╗                     ╙─┬─╜  ╘═╤═╝
           │  ║  insert        ║                     ┌─┴─╖    │
           │  ║  element at    ║           ┌─────────┤ · ╟──┐ │
           │  ║  index         ║           │         ╘═╤═╝  │ │
           │  ╟────────────────╢           │ ┌────╖  ┌─┴─╖  │ │
           │  ║  ɪ(l, e, i) =  ║        ┌──┴─┤ << ╟──┤ › ║  ├─┘
           │  ║    let b =     ║      ┌─┴──╖ ╘═╤══╝  ╘═╤═╝  │
           │  ║      ʁ(l, i);  ║  ┌───┤ << ║   │     ┌─┴──╖ │
           │  ║    (l & ¬(−1   ║  │   ╘═╤══╝   │  ┌──┤ >> ╟─┘
           │  ║      << b)) |  ║ ┌┴┐  ╔═╧══╗  ┌┴┐ │  ╘════╝
           │  ║    (›(l >> b,  ║ └┬┘  ║ −1 ║  └┬┘ │
           │  ║      e) << b)  ║  │   ╚════╝   │  │
           │  ╚════════════════╝  ├─────┬──────┘  │
           └──────────────────────┤     │         │
                                  └───────────────┘
                                                                 ╓┬───╖
                                                          ┌──────╫┘ᴙp ╟──────┐
        ╔════════════════════════════╗     ╓───╖       ┌──┴─┐    ╙────╜    ┌─┴──┐
        ║  reverse list              ║     ║ ᴙ ║       │  ┌─┴─╖  ┌────╖  ┌─┴─╖  │
        ╟────────────────────────────╢     ╙─┬─╜       │  │ › ╟──┤ ᴙp ╟──┤ ‹ ╟┐ │
        ║  ᴙ(l) = ᴙp(l, 0);          ║     ┌─┴──╖      │  ╘═╤═╝  ╘══╤═╝  ╘═══╝│ │
        ╟────────────────────────────╢     │ ᴙp ╟──┐   │    │     ┌─┴─╖       │ │
        ║  ᴙp(l, a) =                ║     ╘═╤══╝  │   │    └─────┤ · ╟───────┘ │
        ║    let (h, t) = ‹(l);      ║     ╔═╧═╗       │          ╘═╤═╝         │
        ║    l ? ᴙp(t, ›(a, h)) : a  ║     ║ 0 ║       │          ┌─┴─╖         │
        ╚════════════════════════════╝     ╚═══╝       └──────────┤ ? ╟─────────┘
                                                                  ╘═╤═╝
                                                               └────┘
   ┌────────────────────────┐
   │   ╓───╖              ┌─┴─╖
   ├───╢ ↨ ╟──────────────┤ · ╟──────┐
   │   ╙─┬─╜       ┌───╖  ╘═╤═╝    ┌─┴─╖
   │     └─────────┤ ʁ ╟────┘   ┌──┤ · ╟─────┐
   │               ╘═╤═╝      ┌─┘  ╘═╤═╝     │   ╔═════════════════════════════╗
   │         ┌───────┴────────┘    ┌─┴─╖     │   ║  replace element            ║
   │         │                   ┌─┤ › ║     │   ╟─────────────────────────────╢
   │         │         ┌────╖  ┌─┘ ╘═╤═╝     │   ║  Changes the element at a   ║
   │   ┌─────┴─────────┤ << ╟──┘    ┌┴┐      │   ║  specific index to a new    ║
   │ ┌─┴─╖             ╘═╤══╝       └┬┘      │   ║  element                    ║
  ┌┴─┤ · ╟──────────┐   ┌┴┐   ╔═══╗  │       │   ╟─────────────────────────────╢
  │  ╘═╤═╝ ╔════╗   │   └┬┘   ║ 0 ║  │       │   ║  ↨(l, i, e) =               ║
  │    │   ║ −1 ║   ├───┬┘    ╚═╤═╝  │       │   ║    let b = ʁ(l, i);         ║
  │    │   ╚═╤══╝  ┌┴┐  │       ├────┴────┐  │   ║    let (_, t) = ‹(l >> b);  ║
  │    │  ┌──┴─╖   └┬┘          │  ┌───╖  │  │   ║    (›(t, e) << b) |         ║
  │    │  │ << ╟────┘           └──┤ ‹ ╟──┘  │   ║      (l & ¬(−1 << b))       ║
  │    │  ╘══╤═╝                   ╘═╤═╝     │   ╚═════════════════════════════╝
  │    └─────┘                    ┌──┴─╖     │
  └───────────────────────────────┤ >> ╟─────┘
                                  ╘════╝

    ╔════════════════════════════════════╗        ╔════════════════════════════════╗
    ║  filter                            ║        ║  map                           ║
    ╟────────────────────────────────────╢        ╟────────────────────────────────╢
    ║  Returns a new list containing     ║        ║  Returns a new list contain-   ║
    ║  only those elements that match a  ║        ║  ing the results of passing    ║
    ║  predicate provided as a lambda    ║        ║  every element through the     ║
    ╟────────────────────────────────────╢        ║  provided lambda function      ║
    ║  Ƒ(l, f) =                         ║        ╟────────────────────────────────╢
    ║    let (h, t) = ‹(l);              ║        ║  ᴍ(l, f) =                     ║
    ║    let r = Ƒ(t, f);                ║        ║    let (h, t) = ‹(l);          ║
    ║    l ? f(h) ? ›(r, h) : r : 0      ║        ║    l ? ›(ᴍ(t, f), f(h)) : 0    ║
    ╚════════════════════════════════════╝        ╚════════════════════════════════╝
       ┌────────────────────────────┐
       │             ┌───╖ ┌───┐    │                          ┌──────────────┐
       │     ┌───────┤ ‹ ╟─┘ ┌─┴─╖  │           ┌──────────────┴─────┐      ╓─┴─╖
       │ ┌───┴─────┐ ╘═╤═╝   │ Ƒ ╟──┤           │   ┌───┬─┐          │      ║ ᴍ ║
       │ │         │   └─┐   ╘═╤═╝  │           │ ┌─┴─╖ └─┘  ┌───╖ ┌─┴─╖    ╙─┬─╜
       │ │ ┌───╖ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖  │           └─┤   ╟──────┤ › ╟─┤ ᴍ ║   ┌──┴──┐
       │ └─┤ › ╟─┤ · ╟─┤ · ╟─┤ · ╟──┘             └─┬─╜      ╘═╤═╝ ╘═╤═╝ ┌─┴─╖ ┌─┴─╖
       │   ╘═╤═╝ ╘═╤═╝ ╘═╤═╝ ╘═╤═╝                  │  ╔═══╗ ┌─┴─╖   └───┤ · ╟─┤ ‹ ╟─┐
       │   ┌─┴─╖ ┌─┴─╖ ┌─┴─╖   ├────┐               │  ║ 0 ╟─┤ ? ╟───┐   ╘═╤═╝ ╘═══╝ │
       └───┤ ? ╟─┤   ╟─┤ · ╟───┘  ╓─┴─╖             │  ╚═══╝ ╘═╤═╝   └─────┘         │
           ╘═╤═╝ └─┬─╜ ╘═╤═╝      ║ Ƒ ║             │          └───                  │
     ╔═══╗ ┌─┴─╖   ├─┐   │        ╙─┬─╜             └────────────────────────────────┘
     ║ 0 ╟─┤ ? ╟─┐ └─┘   ├──────────┘
     ╚═══╝ ╘═╤═╝ └───────┘
             └──

     ┌───────────────┐                   ╔═════════════════════════════════╗
     │             ┌─┴─╖                 ║  aggregate (fold left)          ║
     │   ┌─────────┤ · ╟──────────┐      ╟─────────────────────────────────╢
     │   │ ╓───╖   ╘═╤═╝          │      ║  Starts with an initial value,  ║
     │   └─╢ ᴀ ╟─────┤            │      ║  iteratively applies the        ║
     │     ╙─┬─╜   ┌─┴─╖          │      ║  provided lambda to every list  ║
     │   ┌───┴─────┤   ╟──┐       │      ║  element and the previous       ║
     │   │         └─┬─╜  │  ┌─┐  │      ║  result and returns the final   ║
     │   │         ┌─┴─╖  ├──┴─┘  │      ║  result                         ║
     │   │     ┌───┤   ╟──┘       │      ╟─────────────────────────────────╢
     │   │     │   └─┬─╜          │      ║  ᴀ(l, i, f) =                   ║
     │ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖          │      ║      let (h, t) = ‹(l);         ║
     └─┤ · ╟─┤ · ╟─┤ ᴀ ╟───┐      │      ║      l ? ᴀ(t, f(i)(h), f) : i   ║
       ╘═╤═╝ ╘═╤═╝ ╘═╤═╝ ┌─┴─╖    │      ╚═════════════════════════════════╝
         │     │     │   │ ‹ ╟─┐  │
         │     │   ┌─┴─╖ ╘═╤═╝ ├──┘
         │     └───┤ · ╟───┘   │
         │         ╘═╤═╝       │
         │         ┌─┴─╖       │
         └─────────┤ ? ╟───────┘
                   ╘═╤═╝
                     │
  ╔═══════════════════════════════════════════╗   ╔═══════════════════════════════════════════╗
  ║  index of                                 ║   ║  last index of                            ║
  ╟───────────────────────────────────────────╢   ╟───────────────────────────────────────────╢
  ║  Returns the index of the first element   ║   ║  Returns the index of the last element    ║
  ║  that satisfies a provided predicate      ║   ║  that satisfies a provided predicate      ║
  ║  (−1 if not found).                       ║   ║  (−1 if not found).                       ║
  ╟───────────────────────────────────────────╢   ╟───────────────────────────────────────────╢
  ║  →(l, f) =                                ║   ║  ←(l, f) =                                ║
  ║    let (h, t) = ‹(l);                     ║   ║    let (h, t) = ‹(l);                     ║
  ║    let i = →(t, f);                       ║   ║    let i = ←(t, f);                       ║
  ║    l ? f(h) ? 0 : i ≥ 0 ? ♯(i) : −1 : −1  ║   ║    l ? i ≥ 0 ? ♯(i) : f(h) ? 0 : −1 : −1  ║
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
    │           ╚════╝  ╘═╤═╝    │                  │ └───────────────────────────────┘   │                ╓───╖
    └────────────────────────────┘                  └─────────────────────────────────────┘                ║ ʏ ║
                                                                                                           ╙─┬─╜
                                                                                                         ┌───┴───┐
                                              ╓───╖        ╔═════════════════════════════╗             ┌─┴─╖     │
   ╔═════════════════════════════╗       ┌────╢ ᴄ ╟──┐     ║  flatten                    ║    ┌────────┤ ‹ ╟──┐  │
   ║  concat                     ║    ┌──┴─╖  ╙───╜  │     ╟─────────────────────────────╢    │        ╘═══╝  │  │
   ╟─────────────────────────────╢    │ << ╟┐┌┐      │     ║  Concatenates all the       ║    │ ┌───╖  ┌───╖  │  │
   ║  Concatenates two lists     ║    ╘══╤═╝└┤├┐│    │     ║  lists in a list of lists   ║    └─┤ ʏ ╟──┤ ᴄ ╟──┘  │
   ╟─────────────────────────────╢     ┌─┴─╖ └┘└┴┐┌┐ │     ╟─────────────────────────────╢      ╘═══╝  ╘═╤═╝     │
   ║  ᴄ(l, m) = l | (m << ʙ(l))  ║     │ ʙ ║     └┤├┐│     ║  ʏ(l) = let (h, t) = ‹(l);  ║      ╔═══╗  ┌─┴─╖     │
   ╚═════════════════════════════╝     ╘═╤═╝      └┘└┴┐    ║         l ? ᴄ(h, ʏ(t)) : 0  ║      ║ 0 ╟──┤ ? ╟─────┘
                                         └────────────┘    ╚═════════════════════════════╝      ╚═══╝  ╘═╤═╝
                                                                                                         │
   ╔════════════════════════════════════════╗    ╔═══════════════════════════════════╗
   ║  distinct                              ║    ║  count matches                    ║
   ╟────────────────────────────────────────╢    ╟───────────────────────────────────╢
   ║  Removes duplicate elements from a     ║    ║  Returns the number of elements   ║
   ║  list (and keeps the last occurrence   ║    ║  in the provided list that match  ║
   ║  of each duplicate)                    ║    ║  the provided predicate.          ║
   ╟────────────────────────────────────────╢    ╟───────────────────────────────────╢
   ║  ᴅ(l) =                                ║    ║  ʗ(l, f) =                        ║
   ║    let (h, t) = ‹(l);                  ║    ║    let (h, t) = ‹(l);             ║
   ║    let d = ᴅ(t);                       ║    ║    let r = ʗ(t, f);               ║
   ║    l ? ɢ(d, h) = −1 ? ›(d, h) : d : 0  ║    ║    l ? f(h) ? ♯(r) : r : 0        ║
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

