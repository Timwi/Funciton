﻿ 
    ╔╤═══════════════╗           ╔╤═════════════╗
    ║│ exclusive or  ║           ║│ shift left  ║
    ╚╧═══════════════╝           ╚╧═════════════╝
  ┌────────────────────┐        
  │  ╓───╖             │        ┌─────────┐
  ├──╢ ^ ╟──────┐      │        │ ╓────╖  │   ┌──┐
  │  ╙───╜  ┌┐  │   ┌┐ │        └─╢ << ╟──┼───┴──┘
  └───┬─────┤├──┴─┬─┤├─┘          ╙────╜  │
      │     └┘    │ └┘                 
      └────┬──────┘                    
           │                           
           
 ╔╤═══════════════════════════════════════════════════╗
 ║│ shift right (uses ~ declared in addsubtract.fnc)  ║
 ╚╧═══════════════════════════════════════════════════╝

                       ╓────╖
                    ┌──╢ >> ╟──┐
                    │  ╙────╜  │
                    │        ┌─┴─╖
                    │  ┌─┐   │ ~ ║
                    │    │   ╘═╤═╝
                    │  ┌─┴──╖  │
                    └──┤ << ╟──┘
                       ╘════╝
