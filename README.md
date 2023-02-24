<img src="icon.png" alt="Mod Icon" align="right" />

# Chronicles Mod ![](https://img.shields.io/badge/mod%20loader-tModLoader-1976d2?style=flat-square&labelColor=0d1117&color=brightgreen)

> there are chronicles among us

---

vin — 02/14/2023 12:06 AM
project rundown time
fuck im so bad at formal things
the projects goal is to provide an experience that feels authentic and pays
respect to the original game but also experimenting with the possibilities that
Terraria has provided, generally speaking itd be as if Terraria itself has made
a sequel where playing the game would still have the same feeling as it always
does but with a fresh new coat of paint

## Copying

Assets are All Rights Reserved and remaining parts of the project are GPLv3.

See [COPYING](COPYING) for specific details.

## Building

```sh
# `cd` into your tModLoader `ModSources` folder.
$ cd /path/to/tModLoader/ModSources

# Important to clone into the `ChroniclesMod` folder instead of leaving it as
# the default `chronicles-mod` folder. tModLoader does not play nicely with the
# default name as it expects the mod's assembly name to match the folder name.
$ git clone https://github.com/steviegt6/chronicles-mod ChroniclesMod

# Build the mod (you can also build directly through tModLoader, though I would
# not recommend this).
$ dotnet build -c "Release"
```
