﻿      ╔══════════════════════════╗      ╔══════════════════════════╗       ╔═══════════════════════════════════════╗
      ║  convert lazy to list    ║      ║  convert list to lazy    ║       ║  lazy Fibonacci                       ║
      ╟──────────────────────────╢      ╟──────────────────────────╢       ╟───────────────────────────────────────╢
      ║  Converts a finite lazy  ║      ║  Converts a list to a    ║       ║  Returns the Fibonacci sequence as    ║
      ║  sequence to a list      ║      ║  lazy sequence           ║       ║  an infinite lazy sequence            ║
      ╟──────────────────────────╢      ╟──────────────────────────╢       ╟───────────────────────────────────────╢
      ║  ⌠(q) =                  ║      ║  ⌡(l) =                  ║       ║  λFibo(_) = λfibo(1, 1)               ║
      ║    let (e, r) = q(0);    ║      ║    let (h, t) = ‹(l);    ║       ║  λfibo(a, b) = λ_·[a, λfibo(b, a+b)]  ║
      ║    q ? ›(⌠(r), e) : 0    ║      ║    l ? λ_·[h, ⌡(t)] : 0  ║       ╚═══════════════════════════════════════╝
      ╚══════════════════════════╝      ╚══════════════════════════╝     ╓───────╖                      ╓┬───────╖
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

    ╔═════════════════════════════════╗   ╔═════════════════════════════╗    ╔═══════════════════════════════╗    ╔═══════════════════════════════╗
    ║  map                            ║   ║  filter                     ║    ║  truncate                     ║    ║  skip                         ║
    ╟─────────────────────────────────╢   ╟─────────────────────────────╢    ╟───────────────────────────────╢    ╟───────────────────────────────╢
    ║  Returns a new lazy sequence    ║   ║  Filter a lazy sequence by  ║    ║  Truncates a lazy sequence    ║    ║  Removes the first n items    ║
    ║  that passes every element of   ║   ║  a provided predicate       ║    ║  after at most n elements     ║    ║  from a lazy sequence         ║
    ║  the provided sequence through  ║   ╟─────────────────────────────╢    ╟───────────────────────────────╢    ╟───────────────────────────────╢
    ║  the provided function          ║   ║  ƒ(q, f) =                  ║    ║  ȶ(q, n) =                    ║    ║  ʓ(q, n) =                    ║
    ╟─────────────────────────────────╢   ║    let (e, r) = q(0);       ║    ║    let (e, r) = q(0);         ║    ║    let (_, r) = q(0);         ║
    ║  ɱ(q, f) =                      ║   ║    let s = ƒ(r, f);         ║    ║    q ? n ? λ_·[e, ȶ(r, n−1)]  ║    ║    q ? n ? ʓ(r, n−1) : q : 0  ║
    ║    let (e, r) = q(0);           ║   ║    q ? f(e) ? λ_·[e, s]     ║    ║      : 0 : 0                  ║    ╚═══════════════════════════════╝
    ║    q ? λ_·[f(e), ɱ(r, f)] : 0   ║   ║             : s : 0         ║    ╚═══════════════════════════════╝    ┌───────────────────────────────┐
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
     │          ┌─┴─╖  ┌───╖  ┌─┴─╖   ║  Concatenates a lazy sequence     ║      ║  1  │  ⁞(x) = let f = λ_·[x, f]; f                              ║
     │     ┌────┤ · ╟──┤ ‼ ╟──┤ ‼ ║   ║  of strings into a single string  ║      ║  2  │  ⸗(x, y) = let f = λ_·[x, λ_·[y, f]]; f                   ║
     │   ┌─┴─╖  ╘═╤═╝  ╘═╤═╝  ╘═╤═╝   ║  using the provided separator     ║      ║  3  │  ≡(x, y, z) = let f = λ_·[x, λ_·[y, λ_·[z, f]]]; f        ║
     │ ┌─┤ ʝ ╟────┘    ┌─┴─╖    │     ╟───────────────────────────────────╢      ╚═════╧═══════════════════════════════════════════════════════════╝
     │ │ ╘═══╝       ┌─┤ · ╟────┘     ║  ʝ(q, s) =                        ║                                                                  ╓───╖
     │ │             │ ╘═╤═╝          ║    let (e, r) = q(0);             ║         ╓───╖        ┌──────────────────┐                    ┌───╢ ≡ ╟─┐
     │ │   ┌─────────┤ ┌─┴─╖          ║    q ? r ? e ‼ s ‼ ʝ(r, s)        ║         ║ ⁞ ║        │            ╓───╖ │                  ┌─┴─╖ ╙─┬─╜ │
     │ │   │         └─┤ ? ╟─────┐    ║          : e : 0                  ║         ╙─┬─╜        │   ┌─┐    ┌─╢ ⸗ ╟─┘    ┌─────────────┤ · ╟───┘   │
     │ │   │           ╘═╤═╝     │    ╚═══════════════════════════════════╝         ╔═╧═╕  ┌─┐   │   ├─┘    │ ╙───╜      │ ┌─┬────┐    ╘═╤═╝       │
     │ │ ┌─┴─╖  ╔═══╗  ┌─┴─╖     │                                                ┌─╢   ├──┴─┘   │ ╔═╧═╕  ╔═╧═╕  ┌─┐     │ └─┘  ╔═╧═╕  ╔═╧═╕  ┌─┐  │
     │ ├─┤   ╟──╢ 0 ╟──┤ ? ╟─┐ ┌─┴─╖                                     ╓───╖    │ ╚═╤═╛        └─╢   ├──╢   ├──┴─┘     └──────╢   ├──╢   ├──┴─┘  │
     │ │ └─┬─╜  ╚═══╝  ╘═╤═╝ ├─┤ · ╟─┐    ┌────────────────────┐         ║ ɕ ║    └───┴──┐         ╚═╤═╛  ╚═╤═╛                 ╚═╤═╛  ╚═╤═╛       │
     │ │   └─────────────────┘ ╘═╤═╝ │   ┌┴┐    ┌──────┐       │         ╙─┬─╜           │           └──────┴──┐           ┌─┐  ╔═╧═╕    │         │
     │ └─────────────────────────┘   │   └┬┘  ┌─┴─╖    │ ╓───╖ │   ┌───────┴───────┐                           │           └─┴──╢   ├────┴──┐      │
     └───────────────────────────────┘  ┌─┴─╖ │ ♯ ║    ├─╢ ♫ ╟─┤   │   ┌──────┐    │                                            ╚═╤═╛       │      │
                                        │ ♯ ║ ╘═╤═╝    │ ╙───╜ │   │   │    ┌─┴─╖  │  ╔══════════════════════════════════╗        └────────────────┘
     ╔═════════════════════════════╗    ╘═╤═╝ ┌─┴─╖  ╔═╧═╕ ┌─┐ │   │   │ ┌┐ │ ɕ ║  │  ║  count                           ║
     ║  Sequence of n consecutive  ║     ┌┴┐  │ ♫ ╟──╢   ├─┴─┘ │   │   │ └┤ ╘═╤═╝  │  ╟──────────────────────────────────╢
     ║  integers starting at x     ║     └┬┘  ╘═╤═╝  ╚═╤═╛     │   │ ┌─┴─╖│ ┌─┴─╖  │  ║  Returns the number of elements  ║            ┌──────┐
     ╟─────────────────────────────╢      └─────┘      │       │   └─┤   ╟┘ │ ♯ ║  │  ║  in a finite lazy sequence       ║    ╔═══╗ ┌─┴─╖    │
     ║  ♫(x, n) = n ? λ_·[x,       ║          ╔═══╗  ┌─┴─╖     │     └─┬─╜  ╘═╤═╝  │  ╟──────────────────────────────────╢    ║ 0 ╟─┤   ╟──┐ │
     ║       ♫(♯(x), ¬♯(¬n))] : 0  ║          ║ 0 ╟──┤ ? ╟─────┘     ╔═╧═╗  ┌─┴─╖  │  ║  ɕ(q) = let (_, r) = q(0);       ║    ╚═══╝ └─┬─╜  │ │
     ╚═════════════════════════════╝          ╚═══╝  ╘═╤═╝           ║ 0 ╟──┤ ? ╟──┘  ║         q ? ♯(ɕ(r)) : 0          ║          ┌─┴─╖  │ │
                                                       │             ╚═══╝  ╘═╤═╝     ╚══════════════════════════════════╝      ┌───┤ · ╟──┘ │
                                                                              │                                                 │   ╘═╤═╝    │
   ╔═══════════════════════════════════════╗                                                ╔══════════════════════════════╗  ┌─┴─╖ ╔═╧═╕ ┌─┐│
   ║  Lazy string split                    ║                                                ║  Concatenate two sequences   ║  │ ʭ ╟─╢   ├─┴─┘│
   ╟───────────────────────────────────────╢       ╔════════════════════════════════════╗   ╟──────────────────────────────╢  ╘═╤═╝ ╚═╤═╛    │ ╓───╖
   ║  ǁ(h, n) =                            ║       ║  Infinite sequence of consecutive  ║   ║  ʭ(q, r) =                   ║  ┌─┤   ┌─┴─╖    ├─╢ ʭ ╟─┐
   ║    let p = ʘ(h, n);                   ║       ║  integers starting at x            ║   ║    let (h, t) = q(0);        ║  │ └───┤ ? ╟────┘ ╙───╜ │
   ║    let r = ǁ(h >> 21×(p + ℓ(n)), n);  ║       ╟────────────────────────────────────╢   ║    let s = λ_·[h, ʭ(t, r)];  ║  │     ╘═╤═╝            │
   ║    let q = p = −1;                    ║       ║  ♪(x) = λ_·[x, ♪(♯(x))]            ║   ║    q ? s : r                 ║  │       │              │
   ║    λ_·[q ? h : ʃ(h, 0, p),            ║       ╚════════════════════════════════════╝   ╚══════════════════════════════╝  └──────────────────────┘
   ║        q ? 0 : r]                     ║                       ╓───╖
   ╚═══════════════════════════════════════╝                       ║ ♪ ║                         ┌───────┬─────────────────────────────────────┐
                  ┌─────────┐                                      ╙─┬─╜                         │ ┌─┐ ┌─┴─╖                                   │
            ┌─────┴───┐     ├─────────────────┐           ┌──────────┴──────┐                    │ └─┴─┤   ╟─────────────┐                     │
            │ ╔═══╗ ┌─┴─╖ ┌─┴─╖       ┌─────┐ │           │ ┌───╖  ┌───╖  ╔═╧═╕  ┌─┐             │     └─┬─╜           ┌─┴─╖                   │
            │ ║ 0 ╟─┤ ʃ ╟─┤ ? ╟──┐  ┌─┴─╖   │ │           └─┤ ♯ ╟──┤ ♪ ╟──╢   ├──┴─┘             │       ├─────────────┤ · ╟─┐                 │
            │ ╚═══╝ ╘═╤═╝ ╘═╤═╝  ├──┤ = ║   │ │             ╘═══╝  ╘═══╝  ╚═╤═╛                  │     ┌─┘             ╘═╤═╝ │                 │
          ┌─┴─╖   ┌───┘   ┌─┴─╖  │  ╘═╤═╝   │ │                             │                  ┌─┴─╖ ┌─┴─╖   ┌─┐ ╓───╖ ┌─┴─╖ │                 │
      ┌───┤ · ╟───┘   ┌───┤ · ╟──┘  ╔═╧══╗  │ │                                              ┌─┤ · ╟─┤ · ╟───┘ ├─╢ ʆ ╟─┤ · ╟─┤                 │
      │   ╘═╤═╝       │   ╘═╤═╝     ║ −1 ║  │ │                                              │ ╘═╤═╝ ╘═╤═╝     │ ╙───╜ ╘═╤═╝ │                 │
      │     │ ╔═══╗ ┌─┴─╖ ╔═╧═╕ ┌─┐ ╚════╝  │ │    ╔══════════════════════════════════════╗  │   │     │       └───┐     │   └───┐             │
      │     │ ║ 0 ╟─┤ ? ╟─╢   ├─┴─┘         │ │    ║  Sort by selector                    ║  │   │     │     ╔═══╗ │     │     ┌─┴─╖         ┌─┴─╖
      │     │ ╚═══╝ ╘═╤═╝ ╚═╤═╛ ┌─────────┐ │ │    ╟──────────────────────────────────────╢  │   │   ┌─┴─╖   ║ 0 ║ │     │   ┌─┤ · ╟─────────┤ · ╟─┐
      │   ┌─┴──╖    ┌─┴─╖   │   │   ╓───╖ │ │ │    ║  ʆ(q, s) =                           ║  │   │ ┌─┤   ╟─┐ ╚═╤═╝ │     │   │ ╘═╤═╝ ┌─┐     ╘═╤═╝ │
      │   │ >> ╟────┤ ǁ ╟─┐     │ ┌─╢ ǁ ╟─┘ │ │    ║    let (h, t) = q(0);                ║  │   │ │ └─┬─╜ ├─┐ │ ┌─┴─╖ ┌─┴─╖ │   │   ├─┘       │   │
  ┌───┤   ╘═╤══╝    ╘═══╝ ├───┐ │ │ ╙───╜   │ │    ║    let i = s(h);                     ║  │   │ │ ┌─┴─╖ └─┘ └─┤   ╟─┤ · ╟─┤   │ ┌─┴─╖ ┌───╖ │   │
  │ ┌─┴─╖ ┌─┴─╖ ╔════╗  ┌─┴─╖ ├─┘ └───┐   ┌─┘ │    ║    let l = ʆ(ƒ(t, λe·s(e) < i), s);  ║  │   │ └─┤ · ╟───┐   └─┬─╜ ╘═╤═╝ │ ┌─┴─┤   ╟─┤ > ╟─┘   │
  │ │ + ╟─┤ × ╟─╢ 21 ║  │ ℓ ║ │ ┌───╖ │ ┌─┴─╖ │    ║    let r = ʆ(ƒ(t, λe·s(e) ≥ i), s);  ║  │   │   ╘═╤═╝   │     └─────┤   │ │   └─┬─╜ ╘═╤═╝     │
  │ ╘═╤═╝ ╘═══╝ ╚════╝  ╘═╤═╝ └─┤ ʘ ╟─┴─┤ · ╟─┘    ║    q ? ʭ(l, λ_·[h, r]) : 0           ║  │   │   ┌─┴─╖ ╔═╧═╕ ┌───╖ ┌─┴─╖ │ │     │   ┌─┴─╖     │
  │   └───────────────────┘     ╘═╤═╝   ╘═╤═╝      ╚══════════════════════════════════════╝  │   │   │ ≥ ╟─╢   ├─┤ ƒ ╟─┤ · ╟─┘ └─┐   └───┤ · ╟─┐   │
  └───────────────────────────────┴───────┘                                                  │   │   ╘═╤═╝ ╚═╤═╛ ╘═╤═╝ ╘═╤═╝     │       ╘═╤═╝ │   │
                                                                                             │   └─────┘   ╔═╧═╗ ┌─┴─╖ ╔═╧═╕     │ ╔═══╗ ╔═╧═╕ │   │
                                                                                             │             ║ 0 ║ │ ʆ ╟─╢   ├─┬─┐ │ ║ 0 ╟─╢   ├─┘   │
                                                                                             │             ╚═╤═╝ ╘═╤═╝ ╚═╤═╛ └─┘ │ ╚═══╝ ╚═╤═╛     │
                                                                                             │               │     │   ┌─┴─╖     │ ┌───╖ ┌─┴─╖     │
                                                                                             │               │     └───┤ · ╟─────┴─┤ ʆ ╟─┤ ƒ ║     │
                                                                                             │               │         ╘═╤═╝ ┌───╖ ╘═╤═╝ ╘═╤═╝     │
                                                                                             │               │           └───┤ ʭ ╟───┘     └───────┘
                                                                                             │               │               ╘═╤═╝
                                                                                             │               │               ┌─┴─╖
                                                                                             │               └───────────────┤ ? ╟─┐
                                                                                             │                               ╘═╤═╝ │
                                                                                             │                                 │   │
                                                                                             └─────────────────────────────────────┘


