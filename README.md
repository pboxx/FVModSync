#FVModSync

A tool to install game mods for [Life is Feudal: Forest Village]. FVModSync enables users to organise their mods in a separate folder and copy them over automatically, rather than having to edit files in the game folder by hand. This ought to make it easier for players to use various different mods in parallel, at least until an official tool for this purpose is released. Also, it allows modders to include only the relevant data (i.e. content they have actually changed) in their mods, which improves compatibility between different mods.

v0.3.1beta, for game version **0.9.6042** (2017-01-19)

[Readme + Feedback auf Deutsch] (http://steamcommunity.com/groups/ForestVillageModding/discussions/0/144512875333095921/)

Note that some older mods (or older versions of mods) may not be compatible with v0.2.1beta and up due to the introduction of single field CSV parsing. If you run into issues with “unknown header fields”, redownload the mod; if the issues persist, let the mod author know.

Also, script mods are handled differently in 0.9.6042 -- most if not all script mods that existed prior will need an update. Check whether a mod is compatible with 0.9.6042 before attempting to install it.


Changelog
--
v0.3.1beta:
* Fix mod default lists for game version 0.9.6042
* Handle core/init entries more precisely to account for manual fiddling 
* Remove support for cfg/dress.csv (since that has been removed)

v0.3beta:
* Update script handling for game version 0.9.6042. This game version introduces breaking changes for script mods, hence the minor version increase. 
* Implement automatic addition of script mods to scripts/core/init.lua
* Update scheme/imageset handling for game version 0.9.6042 (which introduces custom imagesets for resources as well)

v0.2.2beta:
* Implement XML parsing (e.g. to add custom imagesets / icons -- see “Notes for modders”, below)
* Implement assignment list parsing (to add/edit individual entries in config.lua -- see “Notes for modders”) 
* Make quickbms use .bms.txt as input script if the user has renamed it
* Ignore .zip in mods
* Only copy generic files from mods when they are newer than those that already exist in game files
* Use a (now optional) external config to let people customise folder names and console verbosity
* Bugfix (single cell parsing, v0.2.1 only): Nuke tabs from single fields in modded CSV 
* Bugfix (single cell parsing, v0.2.1 only): let WrapDelimited check for double quotes

v0.2.1beta:
* Parse all CSV as single fields
* Add support for CSV modularisation (partial CSV files; custom ignore sequence)
* Additional error handling / CSV sanity checks

v0.2beta:
* Filter out tabs from modded CSV
* Output error messages when important files/directories can’t be found
* Throw exception when quickbms fails to run
* Add support for cfg/dress.csv (using first + second field as key)
* Internal cleanup

v0.1.4beta:
* Fix: don’t write include.lua when it has no content (duh)
* Ignore .txt files from mods 
* Console output now includes version info

v0.1.3beta:
* Fix DictHandler some more to nuke empty lines in CSV from mod files as well
* Support for adding custom scripts to include.lua
* Internal streamlining; no more redundant dict copies
* Implement warning about conflicting entries from modded CSV files

v0.1.2beta:
* Change target to .NET 3.5
* Fix DictHandler to skip empty lines in CSV  
* Sort file list internally after parsing modded files (predictable overriding; last one wins)
* Copy only those CSV that are actually modded

v0.1.1beta:
* Fixed bugs in DictHandler.cs (stream not closing; extra CRLF and duplicate headers in CSV)


Features
--
* Parse content from multiple CSV snippets in modded files to a single target file in the game folder
* When any CSV already exist in the game folder, use those to add new content to -- so users can still make manual changes if they wish (they will be preserved when new mods are added)
* Parse entries from multiple mods to scripts/core/init.lua (so custom scripts will be recognised)
* Parse entries in config.lua and elements in XML schemes
* Copy all other modded files to the game folder as they are, taking the directory structure into account
* Predictable overriding: sort mods alphanumerically, the last entry/file will override any conflicting entries/files
* Warn users about override conflicts from modded CSV files in the console output
* Warn users about malformed CSV (mismatching headers, wrong record length)
* Ignore .txt/.TXT from mod folders so users can have readme files in there, as well as .zip (for backups)
* Handles mod defaults by initialising them from a defaults folder


Requirements
--
1. [.NET Framework 3.5] or later required

2. You need to create a Mods folder in your game files manually (ought to be Program Files/Steam/steamapps/common/Life is Feudal Forest Village), top level. The actual name of that folder is irrelevant, it’s only referred to as “Mods folder” here for sake of simplicity.

3. You need to download and unzip [quickbms] in your Mods folder, in a sub-folder named “quickbms”. The [life_is_feudal.bms] script (rightclick > Save As) also needs to be in the quickbms folder. [1]

4. Mods that you want to manage with FVModSync need to be installed in (Mods Folder)/mods (you can change that location in the configuration if you want, see below), and maintain the same directory structure and filenames that the game is using. [2] 


[1] quickbms is needed to export the unmodded CSV data from the game .pak files, to “fill in the blanks” so to speak (i.e. when you have mods that only modify three lines of a CSV, this unmodified data is copied over for the rest so we don’t end up with incomplete files). FVModSync will automatically run quickbms when it needs to, so you don’t need to do anything except put it where FVModSync can find it. 

[2] I believe it’s good practice to include author names in uploads, so that one can reliably distinguish between files from different creators. Downloaders can rename the top level folder, e.g. from pbox_nicemod to ugly-mod-by-random-person, but need to leave the rest of the directory structure ( = everything inside that top level folder) intact.


Installation
--
Download the zip and unzip its contents to your Mods folder, top level. This is what your Mods folder should look like: 

Mods
* mods
    * thismod
    * thatmod
    * ..
* quickbms
    * life_is_feudal.bms
    * quickbms.exe
* FVModSync.exe
* FVModSync.exe.config
* FVModSync_Configuration.xml (optional)
* FVModSync_exportedFiles (will be created by FVModSync)
* readme.md (optional)


How to use
--
* Double click on the exe. 

It will put up a console window and tell you what it’s doing; that window will remain open until you hit Enter (it’ll tell you that as well).


Configuration
--
You can customise some parameters, like the names of subfolders and how much console output you want, in FVModSync_Configuration.xml. GameVersion doesn’t do anything yet. If the file goes missing, FVModSync will use default values (the same that are in the original file). 


Notes for modders
--

**CSV parsing:**

* As of 0.2.1beta, you can have partial CSV in mods: include only those columns that you have edited; existing values will be preserved for all other columns
* Additionally (also added in 0.2.1beta) you can use the escape sequence “fvms:ignore” for individual fields you want to be ignored ( = existing values will be preserved for those fields)
* Header fields in modded game files must be left intact as of 0.2.1beta, otherwise FVModSync will ignore the entire file (since there wouldn’t be any way to tell which column is which)

**Scripts:**

* As of 0.2.2beta, you can override individual entries in core/config.lua with a (Your mod)/scripts/core/config.lua snippet containing only your edits
* All other scripts should go in (YourModFolder)/scripts/; everything that isn't config.lua will be copied over to (Game Folder)/scripts/mods/(YourModFolder). FVModSync will also sanitize both the folder name and the include directive for Lua arithmetic operators (replacing them all with underscore, since their fancy new function will otherwise try to interpret them and fall over). You can still have subfolders in (YourModFolder)/scripts/; those will be maintained.
* As of 0.3beta, the necessary include directives will be handled automatically, i.e. the directory your mod ends up in will be added to /scripts/core/init.lua as requireMod('YourModFolder'). So you do not need to add a separate include file any more.

**XML:**

* As of 0.3beta, you can add your own imagesets to the XML schemes: put a version of the file you want (most likely GameLook1.scheme) with only the root element + the entry for your imageset in /gui/schemes. You can add other elements that way too (not sure if that would be useful).
* Note that as of game version 0.9.6042, custom resources can now get their icons from custom imagesets (this is new in resParams.csv, "icon"). So it's not necessary any more to add resource icons to GameLook1.imageset.
* You can have several imagesets with the same name (e.g. “YourName.imageset” in several different mods); FVModSync will compile them into one. The latest modified version of the accompanying image file will overwrite the other instances.

**Mod Defaults:**

* As of 0.3beta (scripts) / 0.3.1beta (prioritised lists), FVModSync maintains mod defaults: little frameworks that can be installed to make modding more flexible. For example, a default replacement for the “Worker.surviveUpdate” function that splits it up into smaller functions, so that those can be changed individually (instead of having to override the whole thing and thus clashing with every other mod that does the same). For functions/scripts this is technically no different from all other scripts, for lists it means FVModSync will check that the default is installed and then aggregate any new values into it (so the end result is a list compiled from all mods that modify it). This is useful e.g. for introducing new kinds of clothes, tools, drinks and so on (as opposed to adding new recipes for the existing ones).
* Note that the default lists are parsed like text, line by line. This is a bit basic but since they have no index, I’d like to keep the splitting as simple as possible (and not introduce CSV-style pomp and circumstance here).

**General:**

* Mods should only include data that you have edited -- **no unchanged game content**. Having unchanged game content in your mod means you’re prone to override somebody else’s mod with the original, thus effectively breaking their mod (or vice versa). 
* Mods should be distributed in a folder that is equivalent to the game root folder, using the same directory structure as the game. (Mod Folder)/mods/coolmod/cfg/funky/things.csv will be copied to (Game Folder)/cfg/funky/things.csv, for example; a script from /mods/coolmod/scripts/funscript.lua will be in (Game Folder)/scripts/mods/coolmod/funscript.lua (note the reverse folder hierarchy).
* FVModSync sorts all modded files alphanumerically (culture independent) before it starts to copy; the sorting incudes the entire file path from (YourModFolder) downwards ((YourModFolder) = the one that you distribute, like “pbox_nicemod”). This means the load order is predictable and can be used to deliberately override things (the last mod that loads will override the rest); be aware though that users may change the name of your root folder.
* FVModSync will ignore any .txt/.TXT files it finds in mod folders, so you can include readme files and the like as .txt. As a precaution, I recommend you include the word “readme” in your file name -- right now the game does not seem to use any .txt of its own, but if it ever does, I would filter out “readme” instead of “.txt”.
* It will ignore .zip too in case that is useful for something (other than end users being able to backup their mods that way)


Known Issues
--
* CSV exceptions: Right now this does not handle the following files:

  * \cfg\LOD.csv
  * \cfg\names.csv
  * \cfg\tips.csv

This is because those have multiple entries with identical “names” (fields in the first column), we can’t tell how the game distinguishes them, and we don’t want to guess. So if you have or create mods that edit those files, you will need to move/copy them manually. Let me know if there is a need to deal with those.

* FVModSync will create a folder named “FVModSync_exportedFiles” in your game files, which contains various files exported from the game packages. You can delete it if it bothers you, but it’ll reappear next time you run the program. If you empty any of the subfolders (but leave the actual folder intact), FVModSync will currently get a bit stuck, so don’t do that. 

* When you patch the game, **delete FVModSync_exportedFiles** and let FVModSync regenerate it from the patched game files (it will do that automatically). Forgetting to do so may lead to missing strings and the like, since it will continue to use those (now outdated) files.

* FVModSync will copy anything that is in (Mods folder)/mods except txt and zip. Far as I can tell, the game just ignores files that are not referenced anywhere, so it shouldn’t hurt anything -- still, it is probably better not to dump random files into that folder.


Troubleshooting / Feedback
--
* You can post in [Issues] or in the [Feedback thread] in the Forest Village modding group on Steam.

* If you run into issues, the text from the console window may be helpful to figure out the problem: you can copy it with Edit > Select All; Edit > Copy via the context menu (rightclick) on the title bar. The game dumps a log in (Game Folder)/Log.log that is also often quite helpful.

* FVModSync creates a backup of files that already exist in the game folder when it attempts to copy over a modded file (e.g. cfg/Localization.csv.backup). Those are simply renamed, so in order to revert them, just remove the “.backup” suffix.

* If you have all your mods installed with FVModSync, and then something goes wrong, reinstalling them should be quite simple: remove all the cfg/gui/etc folders (those that have the same name as a .pak) from your game folder, as well as scripts/mods and scripts/core (that’s the “core” folder, not core.pak!), and then run FVModSync.exe for a fresh install of all mods from (Mods folder)/mods. Of course if you have made additional manual edits, you need to re-do those.

Important: As of 0.9.6042, **do not remove the scripts folder** from your game files. This used to be mods only in earlier versions, but now the game comes with that folder by default -- if you delete it, you’re going to have to reinstall.


To Do / Wishlist
--
* See the [Feedback thread].


Warranty / Copyleft
--
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but without any warranty; without even the implied warranty of merchantability or fitness for a particular purpose. See the [GNU General Public License v3] for more details.


[Life is Feudal: Forest Village]: http://steamcommunity.com/app/496460/
[quickbms]: http://aluigi.org/quickbms.htm
[life_is_feudal.bms]: http://aluigi.org/bms/life_is_feudal.bms
[.NET Framework 3.5]: https://www.microsoft.com/en-us/download/details.aspx?id=21
[GNU General Public License v3]: https://www.gnu.org/licenses/gpl-3.0.txt
[Releases]: https://github.com/pboxx/FVModSync/releases
[Issues]: https://github.com/pboxx/FVModSync/issues
[Feedback thread]: http://steamcommunity.com/groups/ForestVillageModding/discussions/0/154643249631885475/
