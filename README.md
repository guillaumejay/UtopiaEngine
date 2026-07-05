UtopiaEngine
================

This a port of this [boardgame](ttp://www.boardgamegeek.com/boardgame/75223/utopia-engine), done with permission from the game creator, Nick Hayes.

It's a good game, with nice mechanics, and full of atmosphere.

My main goal is to create a cross-platform game, using [Avalonia](https://avaloniaui.net/) on .NET.

Current state
=============
- the game engine (`UE.Core`) is done, and most of it is covered by unit tests
- the Avalonia UI (`UI/`) is fully playable end-to-end on desktop: new game, region search, combat, construct activation, links, final activation, camp/rest, autosave — bilingual (FR/EN)
- Android and Browser (WASM) heads build but are untested on device

Future Plans
============
- Test the Android and Browser heads for real
- Sounds/animations
