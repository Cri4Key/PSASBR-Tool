PlayStation All-Stars Battle Royale Tool
=======
PSASBR Tool is a tool aiming to simplify the process of modding/ripping the game PlayStation All-Stars Battle Royale. It may work with other BluePoint Engine games for PS Vita as well.

Requirements
============
* [.NET Framework 4.5.2](https://www.microsoft.com/en-US/download/details.aspx?id=42642)
* __Basic PS Vita modding knowledge__: You can check this useful [link](https://github.com/TheRadziu/NoNpDRM-modding/wiki) to learn stuff about assets decrypting, NoNpDRM modding and rePatch usage

Basic Usage
=====
The use of the tool should be straightforward in most cases so I'll just give a brief explenation for each feature. All the mentioned folders are created within the folder the program is located in.

* __EXTRACT PSARC__: Extracts a PSARC Archive inside EXTRACTED PSARC folder. Game uses PSARC for everything, and assets are contained inside them (requires psp2psarc)
* __REPACK PSARC__: Repacks the contents of the chosen folder inside a PSARC, needed for building a PSARC again in order to be used with the game (requires psp2psarc)
* __Analyse CTXR__: Returns various info about the selected PSASBR Texture file(s) in CTXR format
* __Extract Textures__: Extracts the PSASBR (CTXR) or PS Vita (GXT) Texture file(s) to images inside EXTRACTED TEXTURES folder
* __Convert CTXR to GXT__: Converts PSASBR Texture file(s) to GXT inside GXT folder. GXT is the standard format for PS Vita textures: if you have the means to work with them, you can use this.
* __Repack CTXR__: Repacks a texture into a CTXR file in order to be used with the game. The program will ask for two files: the first file must be an existent, working CTXR of the texture you want to repack (original or already modded doesn't matter); the second file must be a DDS file (requires psp2gxt) or a GXT file containing the texture you want to repack. New repacked textures will be saved inside the folder REPACKED TEXTURES.
* __Extract Animation Bundle__: Extracts the animation files of the game from the bundle files in which they are stored. Original paths and names are mantained.

Notes
=======
* In order to use everything the tool has to offer, you need the external components __psp2gxt.exe__ and __psp2psarc.exe__ (or __psarc.exe__ which must be renamed to __psp2psarc.exe__)
	* For obvious reasons I cannot provide means to obtain them: just know they are components of the leaked PS Vita SDK, so all you have to be good at is using Google. Once you retrieve them, put them inside the __Resources__ folder, which is located inside the folder of the program. *The components mentioned must come from a SDK for firmware 1.80 or higher in order to work with PSASBR*

* You can export DDS textures using [GIMP](https://www.gimp.org/) with its [DDS Plugin](https://code.google.com/archive/p/gimp-dds/downloads)
	* More infos about the used compression and mipmaps will come later in a wiki. In the meantime you can use __Analyse Textures__ if you want to know the properties and be the closest possible to the original files.

To Do
=====
* Support for CSND files (inside PSASBR they would allow partial audio modding)
* Make a wiki for the PSASBR files in the repository

Acknowledgements
================
* Scarlet is a game data conversion helper by [xdanieldzd](https://github.com/xdanieldzd), and it's used by the tool during the process of managing the PSASBR textures. Check LICENSE.md for more info.
	* PSASBR Tool in specific uses [my Scarlet fork](https://github.com/Cri4Key/Scarlet) which slightly improves support for the PS Vita texture files, enabling full compatibility with PSASBR Textures.
* "psp2gxt.exe" is used during the process of texture repack of a DDS file
* "psp2psarc.exe" is used for managing the PSARC archives (both extraction and repack)
