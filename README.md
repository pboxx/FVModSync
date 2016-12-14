#FVModSync

A tool to install game mods for [Life is Feudal: Forest Village]. FVModSync enables users to organise their mods in a separate folder and copy them over automatically, rather than having to edit files in the game folder by hand. This ought to make it easier for players to use various different mods in parallel, at least until an official tool for this purpose is released. Also, it allows modders to include only the relevant data (i.e. content they have actually changed) in their mods, which enhances compatibility between different mods.

Version 0.1b written by pbox for **game version 0.9.6005** (2016-12-09).


##Features

* Copy content from multiple CSV snippets in /Mods to a single target file in the game folder
* Copy files other than CSV (scripts, images ...) as they are, keeping the directory structure intact


##Requirements

0. MS .NET Framework 4.5 is required for this version (I've changed this to .NET 3.5 for future updates)

1. You need to create a Mods folder in your game files manually (ought to be Program Files/Steam/steamapps/common/Life is Feudal Forest Village), top level. The actual name of that folder is irrelevant, it's only referred to as "Mods folder" here for sake of simplicity.

2. You need to download and unzip [quickbms] in your Mods folder, top level. The "life_is_feudal.bms" script also needs to be in the quickbms folder. [1]

3. You need to download and unzip FVModSync and  to your Mods folder, top level 

4. Mods that you want to manage with FVModSync need to be installed in Mods Folder/mods, and maintain the same directory structure and filenames that the game is using. For example, if I want to include a snippet of /cfg/normal/houses.csv with my mod, it needs to be in (Mods folder)/mods/pbox_nicemod/cfg/normal/houses.csv. [2] 

**TL;DR:** this is what your Mods folder should look like: 

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
* FVModSync_exportedCSV (will be created by FVModSync)


[1] quickbms is needed to export the unmodded CSV data from the game .pak files, to "fill in the blanks" so to speak (i.e. when you have mods that only modify three lines of a CSV, this unmodified data is copied over for the rest so we don't end up with incomplete files).  
[2] I believe it's good practice to include author names in uploads, so that one can reliably distinguish between files from different creators. Downloaders can rename the top level folder, e.g. from pbox_nicemod to ugly-mod-by-random-person, but need to leave the rest of the directory structure intact.


##How to use

1. Double click on the exe. 

It will put up a console window and tell you what it's doing. If you run into issues, the text from that console window may be helpful to figure out the problem; you can copy it with Edit > Select All; Edit > Copy via the context menu (rightclick) on the title bar.



##Known Issues / Notes

* CSV exceptions: Right now this does not handle the following files:

  * \cfg\dress.csv
  * \cfg\LOD.csv
  * \cfg\names.csv
  * \cfg\tips.csv

This is because those have multiple entries with identical "names" (fields in the first column), we can't tell how the game distinguishes them, and we don't want to guess. So if you have or create mods that edit those files, you will need to move/copy them manually.

* Work folders: FVModSync will create a folder named "fvmodsync_exported" in your game files, which contains various files exported from cfg.pak (one of the game packages). You can delete it if it bothers you, but it'll reappear next time you run the program. If you empty it (but leave the folder intact), FVModSync will currently get a bit stuck, so don't do that.

* Excess files: Right now FVModSync will copy every CSV it knows about to the game folder, regardless of whether a file actually contains modded content. Doesn't hurt anything (those files are not large) but certainly is not pretty.  



##Troubleshooting / Feedback

You can post here in Issues or in [this thread] (http://steamcommunity.com/groups/ForestVillageModding/discussions/0/154643249631885475/) (Forest Village modding group on Steam).

FVModSync creates a backup of CSV files that already exist in the game folder when it attempts to copy over a modded file (e.g. cfg/Localization.csv.backup). Those are simply renamed, so in order to revert them, just remove the ".backup" suffix.

If you have all your mods installed with FVModSync, and then something goes wrong, reinstalling them should be quite simple: remove all the cfg/script/etc folders (those that have the same name as a .pak) from your game files, and then run FVModSync.exe for a fresh install of all mods from /Mods. Of course if you have made additional manual edits, you need to re-do those.



##Warranty / Copyleft

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but without any warranty; without even the implied warranty of merchantability or fitness for a particular purpose. See the [GNU General Public License v3] for more details.


[Life is Feudal: Forest Village]: http://steamcommunity.com/app/496460/
[quickbms]: http://aluigi.altervista.org/quickbms.htm
[GNU General Public License v3]: https://www.gnu.org/licenses/gpl-3.0.txt
