# Game of Life

Basic Game of Life in F# using [Aardvark](https://github.com/aardvark-platform).

The simple version is a poorly implemented copy of [Phillip Trelford's Game of Life on fssnip.net](http://www.fssnip.net/da/title/Game-of-Life).

"Optimised" implementation is taken from [Michael Abrash's "Graphics Programmer's Black Book"](http://www.jagregory.com/abrash-black-book/#chapter-17-the-game-of-life) except I've just used an array rather than pointers.

## Caveats

- I don't understand how the Aardvark incremental engine is working so please don't use this as guidance... I wouldn't be surprised if my approach is redrawing the whole UI on each update!
- No error checking anywhere.
- Benchmarks are completely unfair as they're testing implementation details not the algorithms :)
