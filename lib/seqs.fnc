﻿      ╔══════════════════════════╗       ╔════════════════════════╗        ╔═════════════════════════════════════╗
      ║  convert lazy to list    ║       ║  convert list to lazy  ║        ║  lazy Fibonacci                     ║
      ╟──────────────────────────╢       ╟────────────────────────╢        ╟─────────────────────────────────────╢
      ║  Converts a finite lazy  ║       ║  Converts a list to a  ║        ║  Returns the Fibonacci sequence as  ║
      ║  sequence to a list      ║       ║  lazy sequence         ║        ║  an infinite lazy sequence          ║
      ╟──────────────────────────╢       ╟────────────────────────╢        ╟─────────────────────────────────────╢
      ║  ⌠(q) =                  ║       ║  ⌡(l) =                ║        ║  λFibo(•) = λfibo(1, 1)             ║
      ║    let (e, r) = q(0);    ║       ║    let (h, t) = ‹(l);  ║        ║  λfibo(a, b) = •[a, λfibo(b, a+b)]  ║
      ║    q ? ›(⌠(r), e) : 0    ║       ║    l ? •[h, ⌡(t)] : 0  ║        ╚═════════════════════════════════════╝
      ╚══════════════════════════╝       ╚════════════════════════╝      ╓───────╖                      ╓┬───────╖
                     ╓───╖                                ╓───╖          ║ λFibo ║                   ┌──╫┘ λfibo ╟──┐
                     ║ ⌠ ║                     ┌───┐      ║ ⌡ ║          ╙───┬───╜  ╔═══╗            │  ╙────────╜  │
                     ╙─┬─╜                   ┌─┴─╖ └────┐ ╙─┬─╜              │  ┌───╢ 1 ╟───┐        │  ┌───╖       │
            ┌──────────┴───────┐           ┌─┤ ‹ ╟────┐ └───┴─┐              │  │   ╚═══╝   │      ┌─┴──┤ + ╟───────┴───┐
          ┌─┴─╖                │           │ ╘═══╝    │       │              │  │ ┌───────╖ │    ╔═╧═╕  ╘═╤═╝ ┌───────╖ │
       ┌──┤   ╟──────────────┐ │           │ ┌───╖  ╔═╧═╕     │              │  └─┤ λfibo ╟─┘  ┌─╢   ├─┬─┐└───┤ λfibo ╟─┘
       │  └─┬─╜ ┌───╖  ┌───╖ │ │           └─┤ ⌡ ╟──╢   ├─┬─┐ │              │    ╘═══╤═══╝    │ ╚═╤═╛ └─┘    ╘═══╤═══╝
       │    └───┤ › ╟──┤ ⌠ ╟─┘ │             ╘═══╝  ╚═╤═╛ └─┘ │              │      ┌─┴─╖  ┌─┐ │   │              │
       │        ╘═╤═╝  ╘═══╝   │             ╔═══╗  ┌─┴─╖     │              └──────┤ · ╟──┴─┘ └──────────────────┘
       │ ╔═══╗  ┌─┴─╖          │             ║ 0 ╟──┤ ? ╟─────┘                     ╘═╤═╝
       └─╢ 0 ╟──┤ ? ╟──────────┘             ╚═══╝  ╘═╤═╝                             │
         ╚═══╝  ╘═╤═╝                                 │
                  │

    ╔═════════════════════════════════╗   ╔═════════════════════════════╗     ╔═════════════════════════════╗     ╔═══════════════════════════════╗
    ║  map                            ║   ║  filter                     ║     ║  truncate                   ║     ║  skip                         ║
    ╟─────────────────────────────────╢   ╟─────────────────────────────╢     ╟─────────────────────────────╢     ╟───────────────────────────────╢
    ║  Returns a new lazy sequence    ║   ║  Filter a lazy sequence by  ║     ║  Truncates a lazy sequence  ║     ║  Removes the first n items    ║
    ║  that passes every element of   ║   ║  a provided predicate       ║     ║  after at most n elements   ║     ║  from a lazy sequence         ║
    ║  the provided sequence through  ║   ╟─────────────────────────────╢     ╟─────────────────────────────╢     ╟───────────────────────────────╢
    ║  the provided function          ║   ║  ƒ(q, f) =                  ║     ║  ȶ(q, n) =                  ║     ║  ʓ(q, n) =                    ║
    ╟─────────────────────────────────╢   ║    let (e, r) = q(0);       ║     ║    let (e, r) = q(0);       ║     ║    let (•, r) = q(0);         ║
    ║  ɱ(q, f) =                      ║   ║    let s = ƒ(r, f);         ║     ║    q ? n ? •[e, ȶ(r, n−1)]  ║     ║    q ? n ? ʓ(r, n−1) : q : 0  ║
    ║    let (e, r) = q(0);           ║   ║    q ? f(e) ? •[e, s]       ║     ║      : 0 : 0                ║     ╚═══════════════════════════════╝
    ║    q ? •[f(e), ɱ(r, f)] : 0     ║   ║             : s : 0         ║     ╚═════════════════════════════╝     ┌───────────────────────────────┐
    ╚═════════════════════════════════╝   ╚═════════════════════════════╝         ┌────────────┬───────────┐      │       ┌─┬──────────┐          │
           ┌───────────────────────┐                       ╓───╖                ┌─┴─╖          │  ╓───╖    │      │       └─┘ ┌───╖  ┌─┴─╖  ╔═══╗ │
           │       ╓───╖           │    ┌──────────────────╢ ƒ ╟──────────┐   ┌─┤   ╟─────┐    └──╢ ȶ ╟──┐ │      │   ┌───────┤ ʓ ╟──┤   ╟──╢ 0 ║ │
           ├───────╢ ɱ ╟────┐      │    │     ┌──────┐     ╙───╜          │   │ └─┬─╜   ┌─┴─╖     ╙───╜  │ │      │  ┌┴┐      ╘═╤═╝  └─┬─╜  ╚═══╝ │
         ┌─┴─╖     ╙───╜    │      │    │   ┌─┴─╖  ┌─┴─╖ ┌───────┐        │   │   └─────┤ · ╟────┐       │ │      │  └┬┘      ┌─┴─╖    │    ╓───╖ │
      ┌──┤   ╟───┐      ┌───┴────┐ │    │ ┌─┤   ╟──┤ · ╟─┘   ┌───┴──┐     │   │         ╘═╤═╝    │       │ │      │ ┌─┴─╖ ┌───┤ · ╟─┐  ├────╢ ʓ ╟─┘
      │  └─┬─╜ ┌─┴─╖  ┌─┴─╖  ┌─┐ │ │    │ │ └─┬─╜  ╘═╤═╝   ╔═╧═╕ ┌┐ │     │   │   ┌───┐ ┌─┴─╖  ╔═╧═╕     │ │      │ │ ♯ ║ │   ╘═╤═╝ │  │    ╙───╜
      │    └───┤ · ╟──┤   ╟──┴─┘ │ │    │ │   │    ┌─┴─╖ ┌─╢   ├─┴┘ │     │   │  ┌┴┐  │ │ ȶ ╟──╢   ├─┬─┐ │ │      │ ╘═╤═╝ │   ┌─┴─╖ │  │
      │        ╘═╤═╝  └─┬─╜      │ │    │ │   │    │ ƒ ╟─┤ ╚═╤═╛    │     │   │  └┬┘  │ ╘═╤═╝  ╚═╤═╛ └─┘ │ │      │  ┌┴┐  │ ┌─┤ ? ╟─┘  │
      │        ┌─┴─╖  ╔═╧═╕  ┌─┐ │ │    │ │   │    ╘═╤═╝ │ ┌─┴─╖  ┌─┴─╖   │   │ ┌─┴─╖ └───┘    ┌─┴─╖     │ │      │  └┬┘  │ │ ╘═╤═╝    │
      │        │ ɱ ╟──╢   ├──┴─┘ │ │    │ │   │      │   └─┤ ? ╟──┤   ╟─┐ │   │ │ ♯ ║ ┌────────┤ · ╟───┐ │ │      └───┤   │ │ ┌─┴─╖    │
      │        ╘═╤═╝  ╚═╤═╛      │ │    └─┤   │      │     ╘═╤═╝  └─┬─╜ ├─┘   │ ╘═╤═╝ │        ╘═╤═╝   ├─┘ │          └───┘ └─┤ · ╟─┐  │
      │          │    ┌─┴─╖      │ │      │   │      │     ┌─┴─╖    ├─┐ │     │  ┌┴┐  │        ┌─┴─╖   │   │                  ╘═╤═╝ ├──┘
      │          └────┤ · ╟──────┘ │      │   │      └─────┤ · ╟──┐ └─┘ │     │  └┬┘  │   ┌────┤ ? ╟───┘   │           ╔═══╗  ┌─┴─╖ │
      │               ╘═╤═╝        │      │   │            ╘═╤═╝  └─────┘     │   └───┘   │    ╘═╤═╝       │           ║ 0 ╟──┤ ? ╟─┘
      │        ╔═══╗  ┌─┴─╖        │      │   │     ╔═══╗  ┌─┴─╖              │         ╔═╧═╗  ┌─┴─╖       │           ╚═══╝  ╘═╤═╝
      └────────╢ 0 ╟──┤ ? ╟────────┘      │   └─────╢ 0 ╟──┤ ? ╟─┐            └─────────╢ 0 ╟──┤ ? ╟───────┘                    │
               ╚═══╝  ╘═╤═╝               │         ╚═══╝  ╘═╤═╝ │                      ╚═══╝  ╘═╤═╝
                        │                 └──────────────────────┘                               │

         ╓───╖                        ╔═══════════════════════════════════╗      ╔═════════════════════════════════════════════════════════════════╗
     ┌───╢ ʝ ╟───────────┐            ║  string join                      ║      ║  Lazy sequence repeating one, two or three values indefinitely  ║
     │   ╙───╜    ┌──────┴──────┐     ╟───────────────────────────────────╢      ╟─────┬───────────────────────────────────────────────────────────╢
     │          ┌─┴─╖  ┌───╖  ┌─┴─╖   ║  Concatenates a lazy sequence     ║      ║  1  │  ⁞(x) = let f = •[x, f]; f                                ║
     │     ┌────┤ · ╟──┤ ‼ ╟──┤ ‼ ║   ║  of strings into a single string  ║      ║  2  │  ⸗(x, y) = let f = •[x, •[y, f]]; f                       ║
     │   ┌─┴─╖  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝   ║  using the provided separator     ║      ║  3  │  ≡(x, y, z) = let f = •[x, •[y, •[z, f]]]; f              ║
     │ ┌─┤ ʝ ╟────┘    ┌─┴─╖    │     ╟───────────────────────────────────╢      ╚═════╧═══════════════════════════════════════════════════════════╝
     │ │ ╘═══╝       ┌─┤ · ╟────┘     ║  ʝ(q, s) =                        ║                                                                  ╓───╖
     │ │             │ ╘═╤═╝          ║    let (e, r) = q(0);             ║         ╓───╖        ┌──────────────────┐                    ┌───╢ ≡ ╟─┐
     │ │   ┌─────────┤ ┌─┴─╖          ║    q ? r ? e ‼ s ‼ ʝ(r, s)        ║         ║ ⁞ ║        │            ╓───╖ │                  ┌─┴─╖ ╙─┬─╜ │
     │ │   │         └─┤ ? ╟─────┐    ║          : e : 0                  ║         ╙─┬─╜        │   ┌─┐    ┌─╢ ⸗ ╟─┘    ┌─────────────┤ · ╟───┘   │
     │ │   │           ╘═╤═╝     │    ╚═══════════════════════════════════╝         ╔═╧═╕  ┌─┐   │   ├─┘    │ ╙───╜      │ ┌─┬────┐    ╘═╤═╝       │
     │ │ ┌─┴─╖  ╔═══╗  ┌─┴─╖     │                                                ┌─╢   ├──┴─┘   │ ╔═╧═╕  ╔═╧═╕  ┌─┐     │ └─┘  ╔═╧═╕  ╔═╧═╕  ┌─┐  │
     │ ├─┤   ╟──╢ 0 ╟──┤ ? ╟─┐ ┌─┴─╖                                     ╓───╖    │ ╚═╤═╛        └─╢   ├──╢   ├──┴─┘     └──────╢   ├──╢   ├──┴─┘  │
     │ │ └─┬─╜  ╚═══╝  ╘═╤═╝ ├─┤ · ╟─┐    ┌────────────────────┐         ║ ɕ ║    └───┴──┐         ╚═╤═╛  ╚═╤═╛                 ╚═╤═╛  ╚═╤═╛       │
     │ │   └─────────────────┘ ╘═╤═╝ │    │     ┌──────┐       │         ╙─┬─╜           │           └──────┴──┐           ┌─┐  ╔═╧═╕    │         │
     │ └─────────────────────────┘   │  ┌─┴─╖ ┌─┴─╖    │ ╓───╖ │   ┌───────┴───────┐                           │           └─┴──╢   ├────┴──┐      │
     └───────────────────────────────┘  │ ♭ ║ │ ♯ ║    ├─╢ ♫ ╟─┤   │   ┌──────┐    │                                            ╚═╤═╛       │      │
                                        ╘═╤═╝ ╘═╤═╝    │ ╙───╜ │   │   │    ┌─┴─╖  │  ╔══════════════════════════════════╗        └────────────────┘
     ╔═════════════════════════════╗      │   ┌─┴─╖  ╔═╧═╕ ┌─┐ │   │   │ ┌┐ │ ɕ ║  │  ║  count                           ║
     ║  Sequence of n consecutive  ║      │   │ ♫ ╟──╢   ├─┴─┘ │   │   │ └┤ ╘═╤═╝  │  ╟──────────────────────────────────╢
     ║  integers starting at x     ║      │   ╘═╤═╝  ╚═╤═╛     │   │ ┌─┴─╖│ ┌─┴─╖  │  ║  Returns the number of elements  ║            ┌──────┐
     ╟─────────────────────────────╢      └─────┘      │       │   └─┤   ╟┘ │ ♯ ║  │  ║  in a finite lazy sequence       ║    ╔═══╗ ┌─┴─╖    │
     ║  ♫(x, n) = n ?              ║          ╔═══╗  ┌─┴─╖     │     └─┬─╜  ╘═╤═╝  │  ╟──────────────────────────────────╢    ║ 0 ╟─┤   ╟──┐ │
     ║    ? •[x, ♫(♯(x), ♭(n))]    ║          ║ 0 ╟──┤ ? ╟─────┘     ╔═╧═╗  ┌─┴─╖  │  ║  ɕ(q) = let (•, r) = q(0);       ║    ╚═══╝ └─┬─╜  │ │
     ║    : 0                      ║          ╚═══╝  ╘═╤═╝           ║ 0 ╟──┤ ? ╟──┘  ║         q ? ♯(ɕ(r)) : 0          ║          ┌─┴─╖  │ │
     ╚═════════════════════════════╝                   │             ╚═══╝  ╘═╤═╝     ╚══════════════════════════════════╝      ┌───┤ · ╟──┘ │
                                                                              │                                                 │   ╘═╤═╝    │
   ╔═══════════════════════════════════════╗                                                ╔═════════════════════════════╗   ┌─┴─╖ ╔═╧═╕ ┌─┐│
   ║  Lazy string split                    ║                                                ║  Concatenate two sequences  ║   │ ʭ ╟─╢   ├─┴─┘│            ╔═══════════════════╗
   ╟───────────────────────────────────────╢       ╔════════════════════════════════════╗   ╟─────────────────────────────╢   ╘═╤═╝ ╚═╤═╛    │ ╓───╖      ║  equal            ║
   ║  ǁ(h, n) =                            ║       ║  Infinite sequence of consecutive  ║   ║  ʭ(q, r) =                  ║   ┌─┤   ┌─┴─╖    ├─╢ ʭ ╟─┐    ╟───────────────────╢
   ║    let p = ʘ(h, n);                   ║       ║  integers starting at x            ║   ║    let (h, t) = q(0);       ║   │ └───┤ ? ╟────┘ ╙───╜ │    ║  Determines if    ║
   ║    let r = ǁ(h >> 21×(p + ℓ(n)), n);  ║       ╟────────────────────────────────────╢   ║    let s = •[h, ʭ(t, r)];   ║   │     ╘═╤═╝            │    ║  two sequences    ║
   ║    let q = p = −1;                    ║       ║  ♪(x) = •[x, ♪(♯(x))]              ║   ║    q ? s : r                ║   │       │              │    ║  are equal.       ║
   ║    •[q ? h : ʃ(h, 0, p),              ║       ╚════════════════════════════════════╝   ╚═════════════════════════════╝   └──────────────────────┘    ╟───────────────────╢
   ║      q ? 0 : r]                       ║                       ╓───╖                                                                                  ║  ≛(q, r) =        ║
   ╚═══════════════════════════════════════╝                       ║ ♪ ║                           ┌───────┬─────────────────────────────────────┐        ║    let (h, t) =   ║
                  ┌─────────┐                                      ╙─┬─╜                           │ ┌─┐ ┌─┴─╖                                   │        ║      q(0);        ║
            ┌─────┴───┐     ├─────────────────┐           ┌──────────┴──────┐                      │ └─┴─┤   ╟─────────────┐                     │        ║    let (i, u) =   ║
            │ ╔═══╗ ┌─┴─╖ ┌─┴─╖       ┌─────┐ │           │ ┌───╖  ┌───╖  ╔═╧═╕  ┌─┐               │     └─┬─╜           ┌─┴─╖                   │        ║      r(0);        ║
            │ ║ 0 ╟─┤ ʃ ╟─┤ ? ╟──┐  ┌─┴─╖   │ │           └─┤ ♯ ╟──┤ ♪ ╟──╢   ├──┴─┘               │       ├─────────────┤ · ╟─┐                 │        ║    q              ║
            │ ╚═══╝ ╘═╤═╝ ╘═╤═╝  ├──┤ = ║   │ │             ╘═══╝  ╘═══╝  ╚═╤═╛                    │     ┌─┘             ╘═╤═╝ │                 │        ║    ? r            ║
          ┌─┴─╖   ┌───┘   ┌─┴─╖  │  ╘═╤═╝   │ │                             │                    ┌─┴─╖ ┌─┴─╖   ┌─┐ ╓───╖ ┌─┴─╖ │                 │        ║      ? h = i      ║
      ┌───┤ · ╟───┘   ┌───┤ · ╟──┘  ╔═╧══╗  │ │                                                ┌─┤ · ╟─┤ · ╟───┘ ├─╢ ʆ ╟─┤ · ╟─┤                 │        ║        ? ≛(t, u)  ║
      │   ╘═╤═╝       │   ╘═╤═╝     ║ −1 ║  │ │                                                │ ╘═╤═╝ ╘═╤═╝     │ ╙───╜ ╘═╤═╝ │                 │        ║        : 0        ║
      │     │ ╔═══╗ ┌─┴─╖ ╔═╧═╕ ┌─┐ ╚════╝  │ │     ╔═══════════════════════════════════════╗  │   │     │       └───┐     │   └───┐             │        ║      : 0          ║
      │     │ ║ 0 ╟─┤ ? ╟─╢   ├─┴─┘         │ │     ║  Sort by selector                     ║  │   │     │     ╔═══╗ │     │     ┌─┴─╖         ┌─┴─╖      ║    : r            ║
      │     │ ╚═══╝ ╘═╤═╝ ╚═╤═╛ ┌─────────┐ │ │     ╟───────────────────────────────────────╢  │   │   ┌─┴─╖   ║ 0 ║ │     │   ┌─┤ · ╟─────────┤ · ╟─┐    ║      ? 0          ║
      │   ┌─┴──╖    ┌─┴─╖   │   │   ╓───╖ │ │ │     ║  ʆ(q, s) =                            ║  │   │ ┌─┤   ╟─┐ ╚═╤═╝ │     │   │ ╘═╤═╝ ┌─┐     ╘═╤═╝ │    ║      : −1         ║
      │   │ >> ╟────┤ ǁ ╟─┐     │ ┌─╢ ǁ ╟─┘ │ │     ║    let (h, t) = q(0);                 ║  │   │ │ └─┬─╜ ├─┐ │ ┌─┴─╖ ┌─┴─╖ │   │   ├─┘       │   │    ╚═══════════════════╝
  ┌───┤   ╘═╤══╝    ╘═══╝ ├───┐ │ │ ╙───╜   │ │     ║    let i = s(h);                      ║  │   │ │ ┌─┴─╖ └─┘ └─┤   ╟─┤ · ╟─┤   │ ┌─┴─╖ ┌───╖ │   │            ╓───╖
  │ ┌─┴─╖ ┌─┴─╖ ╔════╗  ┌─┴─╖ ├─┘ └───┐   ┌─┘ │     ║    let l = ʆ(ƒ(t, α·(s(α) < i)), s);  ║  │   │ └─┤ · ╟───┐   └─┬─╜ ╘═╤═╝ │ ┌─┴─┤   ╟─┤ > ╟─┘   │      ┌─────╢ ≛ ╟────┐
  │ │ + ╟─┤ × ╟─╢ 21 ║  │ ℓ ║ │ ┌───╖ │ ┌─┴─╖ │     ║    let r = ʆ(ƒ(t, α·(s(α) ≥ i)), s);  ║  │   │   ╘═╤═╝   │     └─────┤   │ │   └─┬─╜ ╘═╤═╝     │      │     ╙───╜    │
  │ ╘═╤═╝ ╘═══╝ ╚════╝  ╘═╤═╝ └─┤ ʘ ╟─┴─┤ · ╟─┘     ║    q ? ʭ(l, •[h, r]) : 0              ║  │   │   ┌─┴─╖ ╔═╧═╕ ┌───╖ ┌─┴─╖ │ │     │   ┌─┴─╖     │      │ ╔═══╗     ┌──┴──┐
  │   └───────────────────┘     ╘═╤═╝   ╘═╤═╝       ╚═══════════════════════════════════════╝  │   │   │ ≥ ╟─╢   ├─┤ ƒ ╟─┤ · ╟─┘ └─┐   └───┤ · ╟─┐   │      │ ║ 0 ╟─┐ ┌─┴─╖   │
  └───────────────────────────────┴───────┘                                                    │   │   ╘═╤═╝ ╚═╤═╛ ╘═╤═╝ ╘═╤═╝     │       ╘═╤═╝ │   │      │ ╚═══╝ └─┤   ╟─┐ │
                                                                                               │   └─────┘   ╔═╧═╗ ┌─┴─╖ ╔═╧═╕     │ ╔═══╗ ╔═╧═╕ │   │    ┌─┴─────┐   └─┬─╜ │ │
  ┌─────────────────────────────┐                                                              └───────────┐ ║ 0 ║ │ ʆ ╟─╢   ├─┬─┐ │ ║ 0 ╟─╢   ├─┘   │    │     ┌─┴─╖ ┌─┴─╖ │ │
  │         ┌─────────────────┐ │                ╓───╖        ╔═══════════════════════════════════════╗    │ ╚═╤═╝ ╘═╤═╝ ╚═╤═╛ └─┘ │ ╚═══╝ ╚═╤═╛     │    │ ┌───┤ · ╟─┤ · ╟─┘ │
  │         │         ╓┬────╖ │ │              ┌─╢ ɠ ╟──┐     ║  Group by selector                    ║    │   │     │   ┌─┴─╖     │ ┌───╖ ┌─┴─╖     │    │ │   ╘═╤═╝ ╘═╤═╝   │
  │         │       ┌─╫┘ ɠp ╟─┘ │              │ ╙───╜  │     ╟───────────────────────────────────────╢    │   │     └───┤ · ╟─────┴─┤ ʆ ╟─┤ ƒ ║     │    │ │     └───┐ └───┐ │
  │         │       │ ╙─────╜ ┌─┴─╖        ┌───┴──────┐ │     ║  ɠ(q, s) = ɠp(q, ɱ(q, s))             ║    │   │         ╘═╤═╝ ┌───╖ ╘═╤═╝ ╘═╤═╝     │    │ │ ╔═══╗ ┌─┴─╖   │ │
  │         │     ┌─┴─────────┤ · ╟───┐  ┌─┴─╖ ┌────╖ │ │     ╟───────────────────────────────────────╢    │   │           └───┤ ʭ ╟───┘     └───────┘    │ │ ║ 0 ╟─┤   ╟─┐ │ │
  │ ╔═══╗ ┌─┴─╖ ┌─┴─╖         ╘═╤═╝   │  │ ɱ ╟─┤ ɠp ╟─┘ │     ║  ɠp(q, m) =                           ║    │   │               ╘═╤═╝                      │ │ ╚═══╝ └─┬─╜ │ │ │
  │ ║ 0 ╟─┤ · ╟─┤   ╟─┐         │     │  ╘═╤═╝ ╘═╤══╝   │     ║    let (h, t) = q(0);                 ║    │   │               ┌─┴─╖                      │ │ ┌───╖ ┌─┴─╖ │ │ │
  │ ╚═╤═╝ ╘═╤═╝ └─┬─╜ │         │     │    │     │      │     ║    let (k, u) = m(0);                 ║    │   └───────────────┤ ? ╟─┐                    │ └─┤ ≛ ╟─┤ · ╟─┘ │ │
  │ ┌─┴─╖   │   ┌─┴─╖ │   ┌─┐   │ ┌─┐ │    └────────────┘     ║    let (a, x) = ɠq(t, u, k)(0);       ║    │                   ╘═╤═╝ │                    │   ╘═╤═╝ ╘═╤═╝   │ │
  ├─┤   ╟───┘ ┌─┤ · ╟─┘   ├─┘   │ └─┤ │                       ║    let (c, b) = x(0);                 ║    │                     │   │                    │     └─┐   └─┐   │ │
  │ └─┬─╜   ┌─┘ ╘═╤═╝   ╔═╧═╕ ╔═╧═╕ │ │                       ║    let s = •[k, •[h, c]];             ║    └─────────────────────────┘                    │     ┌─┴─╖ ┌─┴─╖ │ │
  │   │     │     └─────╢   ├─╢   ├─┘ │        ┌───────────┐  ║    q ? •[s, ɠp(a, b)] : 0             ║                                                   │   ┌─┤ ? ╟─┤ = ║ │ │
  │   │     │  ┌───┐    ╚═╤═╛ ╚═╤═╛   │      ╔═╧═╕         │  ╟───────────────────────────────────────╢         ╔═════════════════════════════════╗       │   │ ╘═╤═╝ ╘═╤═╝ │ │
  │ ┌─┴──╖  │  │ ┌─┴──╖ ┌─┴─╖ ╔═╧═╕   │  ┌───╢   ├─┬─┐     │  ║  ɠq(q, m, k) =                        ║         ║  Sort by selector (descending)  ║       │   └─┐ └───┐ └───┘ │
  └─┤ ɠq ╟──┘  │ │ ɠp ╟─┤ · ╟─╢   ├─┐ │  │   ╚═╤═╛ └─┘     │  ║    let (h, v) = q(0);                 ║         ╟─────────────────────────────────╢       │   ╔═╧═╗ ┌─┴─╖     │
    ╘══╤═╝   ┌─┘ ╘═╤══╝ ╘═╤═╝ ╚═╤═╛ │ │  │   ╔═╧═╕ ╓┬────╖ │  ║    let (i, w) = m(0);                 ║         ║  ƪ(q, s) = ʆ(q, α·(¬s(α)))      ║       │ ┌─╢ 0 ╟─┤ ? ╟─┐   │
     ┌─┴─╖ ┌─┴─╖ ┌─┴─╖    │     │ ┌─┤ │  │ ┌─╢   ├─╫┘ ɠr ╟─┘  ║    let (x, t) = ɠq(v, w, k)(0);       ║         ╚═════════════════════════════════╝       │ │ ╚═══╝ ╘═╤═╝ ├───┘
   ┌─┤   ╟─┤ · ╟─┤   ╟────┘     │ └─┘ │  │ │ ╚═╤═╛ ╙──┬──╜    ║    let (z, y) = t(0);                 ║            ┌──────────────┐                       │ │       ┌─┴─╖ │
   │ └─┬─╜ ╘═╤═╝ └─┬─╜╔═══╗   ┌─┴─╖   │  │     ├─┐    │       ║    let p = i = k;                     ║            │     ┌─┐      │                       │ │   ┌───┤ · ╟─┘
   │   └─────┘     └──╢ 0 ╟───┤ ? ╟───┘  │     └─┘    │       ║    q ? p ? ɠr(x, y, •[h, z])          ║            │     ├─┘      │                       │ │   │   ╘═╤═╝
   │                  ╚═╤═╝   ╘═╤═╝      └────────────┘       ║          : ɠr(•[h, x], •[i, y], z)    ║          ┌─┴─╖ ┌─┴─╖ ┌┐ ╔═╧═╕ ┌───╖ ╓───╖         │ │ ┌─┴─╖ ┌─┴─╖
   └────────────────────┘       │                             ║      : ɠr(0, 0, 0)                    ║        ┌─┤ · ╟─┤   ╟─┤├─╢   ├─┤ ʆ ╟─╢ ƪ ╟─┐       │ └─┤ ? ╟─┤ ? ╟─┐
                                                              ╟───────────────────────────────────────╢        │ ╘═╤═╝ └─┬─╜ └┘ ╚═╤═╛ ╘═╤═╝ ╙───╜ │       │   ╘═╤═╝ ╘═╤═╝ │
      ┌───────────────────────────────────────────┐           ║  ɠr(x, y, z) = •[x, •[z, y]]          ║        │   └─────┘      ╔═╧═╗   │         │       │  ╔══╧═╗   │   │
    ┌─┴─╖                       ╓┬────╖           │           ╚═══════════════════════════════════════╝        │                ║ 0 ║             │       │  ║ −1 ║       │
  ┌─┤ · ╟───────────────────────╫┘ ɠq ╟─┐         │                                                            │                ╚═══╝             │       │  ╚════╝       │
  │ ╘═╤═╝                       ╙──┬──╜ │         │              ╔══════════════════════════════╗              └──────────────────────────────────┘       └───────────────┘
  │   │                          ┌─┴─╖  │       ┌─┴─╖            ║  fold (aggregate)            ║
  │   │               ┌──────────┤ · ╟──┴───────┤ · ╟─────┐      ╟──────────────────────────────╢    ╔═══════════════════════════════╗  ╔═══════════════════════════════╗
  │   │               │          ╘═╤═╝          ╘═╤═╝     │      ║  ʩ(q, i, f) =                ║    ║  sum                          ║  ║  product                      ║
  │   │               │    ╔═══╗ ┌─┴─╖            │       │      ║    let (h, t) = q(0);        ║    ╟───────────────────────────────╢  ╟───────────────────────────────╢
  │   │               │    ║ 0 ╟─┤   ╟─┐          │       │      ║    q ? ʩ(t, f(i)(h), f) : i  ║    ║  ∑(q) = ʩ(q, 0, α·β·(α + β))  ║  ║  ∏(q) = ʩ(q, 1, α·β·(α × β))  ║
  │   │ ╔═══╗         │    ╚═══╝ └─┬─╜ │          │       │      ╚══════════════════════════════╝    ╚═══════════════════════════════╝  ╚═══════════════════════════════╝
  │   │ ║ 0 ║         │          ┌─┴─╖ │          │       │      ┌─────────┐                            ┌─────────────────────┐                    ┌─────────┐ ╔═══╗
  │   │ ╚═╤═╝       ┌─┴──╖   ┌───┤ · ╟─┘          │       │      │         ├─────────────────────┐      │         ┌─────────┐ │                  ╔═╧═╕       │ ║ 1 ║
  │   │ ┌─┴─╖   ┌───┤ ɠq ╟───┘   ╘═╤═╝            │       │      │       ┌─┴─╖                   │      │       ╔═╧═╕       │ │              ┌───╢   ├───┐   │ ╚═╤═╝
  │   ├─┤   ╟───┘   ╘═╤══╝       ┌─┴─╖            │       │      │   ┌───┤   ╟─┬─┐               │      │   ┌───╢   ├───┐   │ │            ╔═╧═╗ ╚═╤═╛ ┌─┴─╖ │ ┌─┘
  │   │ └─┬─╜         └──────────┤ · ╟──────┐     │       │      │   │   └─┬─╜ └─┘               │      │ ╔═╧═╗ ╚═╤═╛ ┌─┴─╖ │ │            ║ 0 ║   │   │ × ╟─┘ │ ╓───╖
  │   │   └─────────┐            ╘═╤═╝      │     │       │      │   │   ┌─┴─╖     ╔═══╗         │      └─╢ 0 ║   │   │ + ╟─┘ │ ╓───╖      ╚═╤═╝ ╔═╧═╕ ╘═╤═╝   │ ║ ∏ ║
  │   │           ┌─┴─╖        ┌───┴──┐     │     │       │      │   │ ┌─┤   ╟─┬─┐ ║ 0 ║         │        ╚═╤═╝ ╔═╧═╕ ╘═╤═╝   │ ║ ∑ ║        └───╢   ├───┘     │ ╙─┬─╜
  │   │   ┌───────┤ · ╟─┐    ╔═╧═╕ ┌─┐│     │     │       │      │   │ │ └─┬─╜ └─┘ ╚═╤═╝         │          └───╢   ├───┘     │ ╙─┬─╜            ╚═╤═╛       ┌─┴─╖ │
  │   │   │       ╘═╤═╝ └─┬──╢   ├─┴─┘│     │     │       │      │   │ │ ┌─┴─╖     ┌─┴─╖         │              ╚═╤═╛       ┌─┴─╖ │                └─────────┤ ʩ ╟─┘
  │   │   │ ╔═══╗ ┌─┴─╖   │  ╚═╤═╛    │     │     │       │      │   │ └─┤ · ╟─────┤   ╟─┐ ╓───╖ │                └─────────┤ ʩ ╟─┘                          ╘═╤═╝
  │   │   │ ║ 0 ╟─┤   ╟───┘  ┌─┴──╖ ┌─┴─╖ ┌─┴─╖ ╔═╧═╕     │      │   │   ╘═╤═╝     └─┬─╜ ├─╢ ʩ ╟─┘                          ╘═╤═╝                              │
  │   │   │ ╚═══╝ └─┬─╜  ┌───┤ ɠr ╟─┤ · ╟─┤ · ╟─╢   ├─┐   │      │ ┌─┴─╖ ┌─┴─╖       │   │ ╙─┬─╜                              │
  │   │ ┌─┴─╖ ┌───┐ └────┤   ╘═╤══╝ ╘═╤═╝ ╘═╤═╝ ╚═╤═╛ │   │      └─┤ · ╟─┤ ʩ ╟───────┘   │   │
  │   └─┤ · ╟─┘ ┌─┴──╖ ┌─┴─╖ ┌─┴─╖  ┌─┴─╖   │     ├─┐ │   │        ╘═╤═╝ ╘═╤═╝           │   │                                  ╔════════════════════════════════════════╗
  │     ╘═╤═╝ ┌─┤ ɠr ╟─┤ · ╟─┤ ? ╟──┤ ≠ ║   │     └─┘ │   │          │   ┌─┴─╖           │   │      ┌───────────────────────┐   ║  monadic bind (map + concat)           ║
  │       └───┘ ╘═╤══╝ ╘═╤═╝ ╘═╤═╝  ╘═╤═╝ ┌─┴─╖     ┌─┴─╖ │          └─┬─┤ ? ╟───────────┘   │      │ ┌─────────┐           │   ╟────────────────────────────────────────╢
  │       ┌───────┘      │   ┌─┴─╖    └───┤ · ╟─────┤ · ╟─┘            │ ╘═╤═╝               │      │ │ ┌───╖ ┌─┴─╖       ╔═╧═╗ ║  ɓ(q, f) = ʩ(ɱ(q, f), 0, α·β·ʭ(α, β))  ║
  │ ┌─┐ ╔═╧═╕    ┌───────┘ ┌─┤ ? ╟─┐      ╘═╤═╝     ╘═╤═╝              │   │                 │      │ └─┤ ʭ ╟─┤ · ╟───┐   ║ 0 ║ ╚════════════════════════════════════════╝
  │ └─┴─╢   ├────┘  ┌──────┘ ╘═╤═╝ ├────┐ ┌─┴─╖       │                └─────────────────────┘      │   ╘═╤═╝ ╘═╤═╝   │   ╚═╤═╝   ┌─────────┐
  │     ╚═╤═╛    ┌──┴─╖  ╔═══╗ │ ┌─┴─╖  └─┤ · ╟─┐     │                                             │     │   ╔═╧═╕ ╔═╧═╕ ┌─┴─╖ ┌─┴─╖       │
  │       │    ┌─┤ ɠr ╟──╢ 0 ╟───┤   ╟─┐  ╘═╤═╝ │     │                                             │     └───╢   ├─╢   ├─┤ ʩ ╟─┤ ɱ ║       │    ┌─────────────┐   ╓───╖
  │       │    │ ╘══╤═╝  ╚═╤═╝   └─┬─╜ └────┘ ┌─┴─╖   │    ╔═════════════════════════════════╗      │         ╚═╤═╛ ╚═╤═╛ ╘═╤═╝ ╘═╤═╝ ╓───╖ │    │             ├───╢ ᶗ ╟─┐
  │       │    │    ├──────┘       │        ┌─┤ · ╟───┘    ║  is true for any one            ║      │           └──┬──┘     │     └───╢ ɓ ╟─┘    │           ┌─┴─╖ ╙───╜ │
  │       │    └────┘              │        │ ╘═╤═╝        ╟─────────────────────────────────╢      └──────────────┘                  ╙───╜      │ ┌─────────┤   ╟─┐     │
  │       └────────────────────────┴────────┘   │          ║  ∃(q, f) =                      ║                                                   │ │         └─┬─╜ │     │
  └─────────────────────────────────────────────┘          ║    let (h, t) = q(0);           ║      ╔═════════════════════════════════╗          │ │         ┌─┴─╖ │     │
                                                           ║    q ? f(h) ? −1 : ∃(t, f) : 0  ║      ║  is true for all                ║          │ │   ┌─────┤ · ╟─┘     │
                                                           ╚═════════════════════════════════╝      ╟─────────────────────────────────╢          │ │ ┌─┴─┐   ╘═╤═╝       │
     ╔════════════════════════════════════════════╗                            ┌───────────┐        ║  ∀(q, f) =                      ║          │ │ │ ┌─┴─╖ ╔═╧═╕       │
     ║  zip                                       ║                            │   ╓───╖   │        ║    let (h, t) = q(0);           ║          │ │ │ │ ᶗ ╟─╢   ├─┬─┐   │
     ╟────────────────────────────────────────────╢                            ├───╢ ∃ ╟─┐ │        ║    q ? f(h) ? ∀(t, f) : 0 : −1  ║          │ │ │ ╘═╤═╝ ╚═╤═╛ └─┘   │
     ║  ʑ(q, r, f) =                              ║                          ┌─┴─╖ ╙───╜ │ │        ╚═════════════════════════════════╝          │ │ │   │   ┌─┴─╖ ┌───╖ │
     ║    let (h, t) = q(0);                      ║          ┌───────────────┤   ╟─┐     │ │                ┌───────────────────────┐            │ │ │   └───┤ · ╟─┤ ♭ ╟─┴─┐
     ║    let (i, u) = r(0);                      ║          │   ┌─────────┐ └─┬─╜ │     │ │                │         ┌─────┐       │            │ │ │       ╘═╤═╝ ╘═══╝   │
     ║    q ? r ? •[f(h)(i), ʑ(t, u, f)] : 0 : 0  ║          │   │  ╔════╗ │ ┌─┴─╖ │     │ │                │ ╔═══╗ ┌─┴─╖   │       │            │ │ │       ┌─┴─╖         │
     ╚════════════════════════════════════════════╝          │   │  ║ −1 ║ └─┤ · ╟─┘     │ │                │ ║ 0 ╟─┤   ╟─┐ │ ╓───╖ │            │ │ └───────┤ ? ╟─────────┘
     ┌───────────────────────────────────────────┐           │   │  ╚══╤═╝   ╘═╤═╝       │ │                │ ╚═══╝ └─┬─╜ │ ├─╢ ∀ ╟─┤            │ │         ╘═╤═╝
     │             ╔═══╗                         │           │ ┌─┴─╖ ┌─┴─╖   ┌─┴─╖       │ │                │ ┌───╖ ┌─┴─╖ │ │ ╙───╜ │            │ │   ╔═══╗ ┌─┴─╖
     │             ║ 0 ║                         │           │ │ ∃ ╟─┤ ? ╟───┤   ╟─────┐ │ │                └─┤ ∀ ╟─┤ · ╟─┘ │       │            │ └───╢ 0 ╟─┤ ? ╟─┐
     │             ╚═╤═╝                         │           │ ╘═╤═╝ ╘═╤═╝   └─┬─╜ ┌─┐ ├─┘ │                  ╘═╤═╝ ╘═╤═╝   └───┐   │            │     ╚═══╝ ╘═╤═╝ │
     │             ┌─┴─╖                         │           │   │   ┌─┴─╖     └───┴─┘ │   │            ╔═══╗ ┌─┴─╖ ┌─┴─╖     ┌─┴─╖ │            │             │   │
     │           ┌─┤   ╟───────────────┐         │           │   └───┤ · ╟─────────────┘   │            ║ 0 ╟─┤ ? ╟─┤   ╟─────┤ · ╟─┘            └─────────────────┘
     │           │ └─┬─╜   ┌───────┐   │         │           │       ╘═╤═╝                 │            ╚═══╝ ╘═╤═╝ └─┬─╜ ┌─┐ ╘═╤═╝     ╔══════════════════════════════════════╗
     │           │ ┌─┴─╖ ┌─┴─╖     │ ┌─┴─╖       │           │ ╔═══╗ ┌─┴─╖                 │           ╔════╗ ┌─┴─╖   └───┴─┘   │       ║  Omit element at index               ║
     │           └─┤ · ╟─┤   ╟─┬─┐ ├─┤ · ╟─────┐ │           └─╢ 0 ╟─┤ ? ╟─────────────────┘           ║ −1 ╟─┤ ? ╟─────────────┘       ╟──────────────────────────────────────╢
     │   ┌─────┐   ╘═╤═╝ └─┬─╜ └─┘ │ ╘═╤═╝     │ │             ╚═══╝ ╘═╤═╝                             ╚════╝ ╘═╤═╝                     ║  ᶗ(q, i) =                           ║
     │ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖ ┌─┴─╖     │   │       │ │                     │                                        │                       ║    var (h, t) = q(0);                ║
     └─┤   ╟─┤ · ╟─┤ · ╟─┤   ╟─┬─┐ │   │       │ │                                                                                      ║    q ? i ? •[h, ᶗ(t, ♭(i))] : t : 0  ║
       └─┬─╜ ╘═╤═╝ ╘═╤═╝ └─┬─╜ └─┘ │   │       │ │      ╔══════════════════════════════════════════════════════════╗                    ╚══════════════════════════════════════╝
         │     │   ┌─┴─╖ ╔═╧═╕     │   │ ╓───╖ │ │      ║  permutations                                            ║
         │     └───┤ ʑ ╟─╢   ├─┬─┐ │   ├─╢ ʑ ╟─┘ │      ╟──────────────────────────────────────────────────────────╢    ╔════════════════════════════════════════╗
         │         ╘═╤═╝ ╚═╤═╛ └─┘ │   │ ╙─┬─╜   │      ║  ƥ(q) =                                                  ║    ║  • Cartesian product                   ║
         │           │   ┌─┴─╖     │   │   │     │      ║    q ? ɓ(ʑ(q, ♪(0), ε·ι·ɱ(ƥ(ᶗ(q, ι)), π·•[ε, π])), α·α)  ║    ║  • default if empty                    ║
         │           └───┤ · ╟─────┘   │   │     │      ║      : •[0, 0]                                           ║    ╟────────────────────────────────────────╢
         │               ╘═╤═╝         │   │     │      ╚══════════════════════════════════════════════════════════╝    ║  ⊗(q, r, f) = ɓ(q, ε·ɱ(r, ζ·f(ε)(ζ)))  ║
         │               ┌─┴─╖         │   │     │           ┌───────────────────────────────────────────┐              ║  ɗ(q, v) = q ? q : •[v, 0]             ║
         │           ┌───┤ ? ╟─────────┘   │     │           │ ┌─┬───┐     ┌─────┐                 ┌───┐ │              ╚════════════════════════════════════════╝
         │           │   ╘═╤═╝             │     │           │ └─┘ ╔═╧═╕ ┌─┴─╖ ╔═╧═╕ ┌───╖ ┌───╖ ┌─┴─╖ │ │                  ┌───────────────────────┐
         │         ╔═╧═╗ ┌─┴─╖             │     │           └─────╢   ├─┤ · ╟─╢   ├─┤ ɱ ╟─┤ ƥ ╟─┤ ᶗ ║ │ │                  │     ┌─────┐           │
         └─────────╢ 0 ╟─┤ ? ╟─────────────┴─────┘                 ╚═╤═╛ ╘═╤═╝ ╚═╤═╛ ╘═╤═╝ ╘═══╝ ╘═╤═╝ │ │                ┌─┴─╖ ╔═╧═╕ ┌─┴─╖ ┌───╖ ┌─┴─╖ ╓───╖
                   ╚═══╝ ╘═╤═╝                                       └─────┘     │   ╔═╧═╕       ┌─┴─╖ │ │                │ ɱ ╟─╢   ├─┤ · ╟─┤ ɓ ╟─┤ · ╟─╢ ⊗ ╟─┐
                           │                                                     └─┬─╢   ├───────┤ · ╟─┘ │                ╘═╤═╝ ╚═╤═╛ ╘═╤═╝ ╘═╤═╝ ╘═╤═╝ ╙─┬─╜ │
                                                                                   │ ╚═╤═╛       ╘═╤═╝   │                ╔═╧═╕ ╔═╧═╗   │     │     └─────┘   │
    ╔══════════════════════════════════════════════════╗       ┌───────────────────┤ ╔═╧═╕       ┌─┴─╖   │ ╓───╖        ┌─╢   ├─╢ 0 ║   │                     │
    ║  subsequences                                    ║       │                   └─╢   ├───────┤ · ╟───┘ ║ ƥ ║        │ ╚═╤═╛ ╚═══╝   │            ╓───╖    │
    ╟──────────────────────────────────────────────────╢       │ ┌───┐               ╚═╤═╛       ╘═╤═╝     ╙─┬─╜        │ ┌─┴─╖         │  ┌─────────╢ ɗ ╟─┐  │
    ║  ʂ(q) =                                          ║       │ │ ╔═╧═╕       ┌───╖ ┌─┴─╖         └─────┐   │          └─┤ · ╟─┐       │  │   ┌─┐   ╙───╜ │  │
    ║    let (h, t) = q(0);                            ║       │ └─╢   ├───────┤ ɓ ╟─┤ ʑ ╟─┐ ┌───╖ ╔═══╗ │   │            ╘═╤═╝ │       │  │   ├─┘   ┌───┐ │  │
    ║    q ? ɓ(ʂ(t), σ·•[σ, •[•[h, σ], 0]]) : •[0, 0]  ║       │   ╚═╤═╛ ┌─┐   ╘═╤═╝ ╘═╤═╝ └─┤ ♪ ╟─╢ 0 ║ ├───┘            ┌─┴─╖ │       │  │ ╔═╧═╕ ┌─┴─╖ ├─┘  │
    ╚══════════════════════════════════════════════════╝       │     │   └─┤     │     │     ╘═══╝ ╚═══╝ │            ┌─┬─┤   ╟─┘       │  └─╢   ├─┤ ? ╟─┘    │
    ┌───────────────────────────────────────┐                  │   ╔═╧═╗ ╔═╧═╕ ┌─┴─╖   ├─────────────────┘            └─┘ └─┬─╜         │    ╚═╤═╛ ╘═╤═╝      │
    │   ┌─────────────────┐                 │                  └───╢ 0 ╟─╢   ├─┤ ? ╟───┘                                  ┌─┴─╖         │    ╔═╧═╗   │        │
    │   │     ┌─┐   ┌─┐   │                 │                      ╚═╤═╝ ╚═╤═╛ ╘═╤═╝                                  ┌─┬─┤   ╟─────────┘    ║ 0 ║   └─┘      │
    │   │     ├─┘   ├─┘   ├───────┐         │                        └─────┘     │                                    └─┘ └─┬─╜              ╚═══╝            │
    │ ┌─┴─╖ ╔═╧═╕ ╔═╧═╕ ╔═╧═╕ ┌─┐ │         │                                                                               └─────────────────────────────────┘
    └─┤ · ╟─╢   ├─╢   ├─╢   ├─┴─┘ │         │
      ╘═╤═╝ ╚═╤═╛ ╚═╤═╛ ╚═╤═╛     │         │       ╓───╖
        └─────┘ ┌───┘   ╔═╧═╕     │         │       ║ ʂ ║
                │   ┌───╢   ├─────┘         │       ╙─┬─╜
                │   │   ╚═╤═╛ ┌───╖ ┌───╖ ┌─┴─╖ ╔═══╗ │
                │   │ ┌─┐ └───┤ ɓ ╟─┤ ʂ ╟─┤   ╟─╢ 0 ║ │
                │   │ └─┴─┐   ╘═╤═╝ ╘═══╝ └─┬─╜ ╚═══╝ │
                │ ╔═╧═╗ ╔═╧═╕ ┌─┴─╖         ├─────────┘
                └─╢ 0 ╟─╢   ├─┤ ? ╟─────────┘
                  ╚═╤═╝ ╚═╤═╛ ╘═╤═╝
                    └─────┘     │
