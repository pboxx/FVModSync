#FVModSync

A tool to install game mods for [Life is Feudal: Forest Village]. FVModSync enables users to organise their mods in a separate folder and copy them over automatically, rather than having to edit files in the game folder by hand. This ought to make it easier for players to use various different mods in parallel, at least until an official tool for this purpose is released. Also, it allows modders to include only the relevant data (i.e. content they have actually changed) in their mods, which improves compatibility between different mods.

v0.2beta, for game version **0.9.6008**..**0.9.6034**


Changelog
--
v0.2beta:
* Filter out tabs from modded CSV
* Output error messages when important files/directories can't be found
* Throw exception when quickbms fails to run
* Add support for cfg/dress.csv (using first + second field as key)
* Internal cleanup

v0.1.4beta:
* Fix: don't write include.lua when it has no content (duh)
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
* Copy content from multiple CSV snippets in modded files to a single target file in the game folder
* Copy entries from multiple mods to scripts/incude.lua (so custom scripts will be recognised)
* Copy all other modded files to the game folder as they are, keeping the directory structure intact
* Predictable overriding: sort mods alphanumerically, the last entry/file will override any conflicting entries/files
* Warn users about override conflicts from modded CSV files in the console output
* Ignore .txt/.TXT from mod folders so users can have readme files in there


Requirements
--
1. .NET Framework 3.5 or later required

2. You need to create a Mods folder in your game files manually (ought to be Program Files/Steam/steamapps/common/Life is Feudal Forest Village), top level. The actual name of that folder is irrelevant, it's only referred to as "Mods folder" here for sake of simplicity.

3. You need to download and unzip [quickbms] in your Mods folder, top level. The "life_is_feudal.bms" script also needs to be in the quickbms folder; make sure the file extension is actually .bms and not .txt. [1]

4. Mods that you want to manage with FVModSync need to be installed in Mods Folder/mods, and maintain the same directory structure and filenames that the game is using. For example, if I want to include a snippet of /cfg/normal/houses.csv with my mod, it needs to be in (Mods folder)/mods/pbox_nicemod/cfg/normal/houses.csv. [2] 


[1] quickbms is needed to export the unmodded CSV data from the game .pak files, to "fill in the blanks" so to speak (i.e. when you have mods that only modify three lines of a CSV, this unmodified data is copied over for the rest so we don't end up with incomplete files).

[2] I believe it's good practice to include author names in uploads, so that one can reliably distinguish between files from different creators. Downloaders can rename the top level folder, e.g. from pbox_nicemod to ugly-mod-by-random-person, but need to leave the rest of the directory structure intact.


Installation
--
Download the zip ([Releases], scroll down) and unzip its contents to your Mods folder, top level. This is what your Mods folder should look like: 

Mods
* mods
    * thismod
    * thatmod
    * ..
* quickbms
    * life_is_feudal.bms
    * quickbms.exe
* FVModSync.cfg
* FVModSync.exe
* FVModSync.exe.config
* FVModSync_exportedFiles (will be created by FVModSync)
* readme.md (optional)


How to use
--
1. Double click on the exe. 

It will put up a console window and tell you what it's doing; that window will remain open until you hit Enter (it'll tell you that as well).


Notes for modders
--
* Mods should only include data that you have edited -- **no unchanged game content**. Having unchanged game content in your mod means you're prone to override somebody else's mod with the original, thus effectively breaking their mod (or vice versa). 
* Leave the header intact in CSV files; FVModSync will ignore the first line
* When you have custom scripts, include a /scripts/include.lua snippet listing only your scripts; those entries will be added to (Game Folder)/scripts/include.lua 
* Mods should be distributed in a folder that is equivalent to the game root folder, using the same directory structure as the game. (Mod Folder)/mods/coolmod/cfg/funky/things.csv will be copied to (Game Folder)/cfg/funky/things.csv, for example.
* FVModSync sorts all modded files alphanumerically (culture independent) before it starts to copy; the sorting incudes the entire file path from the mod root folder downwards (mod root folder = the one that you distribute, like "pbox_nicemod"). This means the load order is predictable and can be used to deliberately override things (the last mod that loads will override the rest); be aware though that users may change the name of your root folder.
* FVModSync will ignore any .txt/.TXT files it finds in mod folders, so you can include readme files and the like as .txt


Known Issues
--
* CSV exceptions: Right now this does not handle the following files:

  * \cfg\LOD.csv
  * \cfg\names.csv
  * \cfg\tips.csv

This is because those have multiple entries with identical "names" (fields in the first column), we can't tell how the game distinguishes them, and we don't want to guess. So if you have or create mods that edit those files, you will need to move/copy them manually.

* FVModSync will create a folder named "FVModSync_exportedFiles" in your game files, which contains various files  from cfg.pak and scripts.pak (two of the game packages). You can delete it if it bothers you, but it'll reappear next time you run the program. If you empty it (but leave the folder intact), FVModSync will currently get a bit stuck, so don't do that. 

* When you patch the game, **delete FVModSync_exportedFiles** and let FVModSync regenerate it from the patched game files (it will do that automatically). Forgetting to do so may lead to missing strings and the like, since it will continue to use those (now outdated) files.

* FVModSync will copy anything that is in (Mods folder)/mods except txt. Far as I can tell, the game just ignores files that are not referenced anywhere, so it shouldn't hurt anything -- still, it is probably better not to dump random files into that folder.


Troubleshooting / Feedback
--
* You can post in [Issues] or in the [Feedback thread] (Forest Village modding group on Steam).

* If you run into issues, the text from the console window may be helpful to figure out the problem: you can copy it with Edit > Select All; Edit > Copy via the context menu (rightclick) on the title bar. The game dumps a log in (Game Folder)/Log.log that is also often quite helpful.

* FVModSync creates a backup of files that already exist in the game folder when it attempts to copy over a modded file (e.g. cfg/Localization.csv.backup). Those are simply renamed, so in order to revert them, just remove the ".backup" suffix.

* If you have all your mods installed with FVModSync, and then something goes wrong, reinstalling them should be quite simple: remove all the cfg/script/etc folders (those that have the same name as a .pak) from your game folder, and then run FVModSync.exe for a fresh install of all mods from /Mods. Of course if you have made additional manual edits, you need to re-do those.


To Do / Wishlist
--
* See the [Feedback thread].


Warranty / Copyleft
--
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but without any warranty; without even the implied warranty of merchantability or fitness for a particular purpose. See the [GNU General Public License v3] for more details.


[Life is Feudal: Forest Village]: http://steamcommunity.com/app/496460/
[quickbms]: http://aluigi.altervista.org/quickbms.htm
[GNU General Public License v3]: https://www.gnu.org/licenses/gpl-3.0.txt
[Releases]: https://github.com/pboxx/FVModSync/releases
[Issues]: https://github.com/pboxx/FVModSync/issues
[Feedback thread]: http://steamcommunity.com/groups/ForestVillageModding/discussions/0/154643249631885475/
