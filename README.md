# The Relhax Modpack
The official dedicated modpack installer for the RELIC Gaming Community
###Description and reason for development
  This entire effort is dedicated to the OMC ModPack team (which includes me, :P). O7 to them, and grumpelumpf, the work done of the modpack over the years will not be forgotten.
  
  When I looked at the current modpack installers, they all look the same: in size, in UI, and were all made with the inno setup creator. Many describe these installers as "clunkey and slow". Trying to get that perfect mod setup can sometimes take hours. Have you ever had a "modpack setup night"?
  
  The goal of this modpack is to redefine what a modpack installer can be, while keeping a simple and straightforward interface. Instead of using an inno setup template, I decided to make my own template in Microsoft's C# programming language. Some of the UI features in this modpack are new(tab category view), and some are kept in line with previous modpacks (right click to preview).
  
##Modpack Features
###Why would you use this modpack over Aslain's/OMC's/any other inno setup template modpack?
- Improved UI:
  - Instead of a giant unscrollable list of hard-to-find mods to select from, the mods are presented in tabs, each tab page being a mod catagory. Xvm has a tab page, garage stats have a page, damagelogs have a page, etc. Mods per tab are sorted alphabetically
  - Each Window can have it's font type changed. The font size, regardless of type you choose, can be increased as well.
  - The Mod selection window and mod preview window are resizeable.
  - The Mod preview window picture viewer has been re-designed, while keeping the familiar user interface:
    - Pictures load asynchronously. This means that The UI does not lock up waiting for the picture to load.
    - The picture viewer is web-based. You hard drive won't become cluttered with pictures.
- Mod selections can be saved:
  - Like omc, you can save and load your mod selections to and from a file.
  - You can even use this file to automate the install process (See "Automation Section")
- Performance:
  - The loading and installation times of this modpack vs. other major modpacks have been reduced by up to **86%** and **60%** respectively(1), on a standard hard drive, and make finding the configurations you like much quicker.
- Automation:
  - The modpack and be set at command line with a "/auto-install config_file_name.xml" switch to automatically install the modpack, with your preference of mods selected. In this situation, you could install without any interaction, and update all your mods in seconds.
  
###Additional Information
The modpack is currently in alpha, proof of concept. I am still adding minor features and adding finishing touches to the application. This means that bugs may still exist. To prevent possible loss of your "perfect modpack configuration" it is recommended to back it up editor manually or using the "backup mods" checkbox option.
  
**If you come across a bug or feature request please take one of these actions:**
- Record it here:
https://docs.google.com/spreadsheets/d/1LmPCMAx0RajW4lVYAnguHjjd8jArtWuZIGciFN76AI4/edit?usp=sharing (quickest)
- (if you're in relic gaming community) send me a slack message (quickest)
- Send me a form message
- Reply to this thread

**When you do, please attach two files:**
- a screenshot of the error (if applicable)
- your RelHaxLog.txt file

Fell free to leave feedback, tell me what you think, what should change, etc.

If you want to help develop the modpack, I would be glad for the help and will help set you up with an environment.

If you want to see current progress, take a look at this todo list:
https://github.com/Willster419/RelicModManager/blob/master/RelicModManager/bin/Debug/relicMod%20TODO.txt

Latest release notes can be found here:
https://github.com/Willster419/RelicModManager/blob/master/RelicModManager/bin/Debug/releaseNotes.txt

##Download
You can download the modpack from this link:

http://adf.ly/1l28oP (donate link)

http://willster419.atwebpages.com/Applications/RelHaxModPack/RelicModManager.exe (direct link)

If you can spare a few dollars and like the Modpack, please consider donating:
https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=76KNV8KXKYNG2

##Credits
 - OMC Modpack for giving me an internal structure to mirror and improve upon, along with premade zip files to start with.
 - Rkk1945 For helping with code edits and resource support, along with helping with the closed alpha test.

##FAQ

####*Why is there such a difference in install times?*
 While the major installers are single-threaded (as far as I can tell), mine is not. What does that mean? Looking at mod zip extraction, for example, when a file is extracted, it is reported to the GUI in a synchronous manner. This means that the modpack can only extract as fast as it can pump events to the GUI for each entry extracted in a zip file. This is where multi-threading come in. You can create a separate thread and have it only extract, and asynchronously report the progress to the GUI. This means three things:
  - The GUI is not laggy during install
  - The install is not frozen when the ui thread is blocked, like moving the window for example
  - The extraction is limited to your hard drive speed, not the processor GUI reporting speed.

####*I have a perfect selection of mods that I want. Can I save this selection?*
  Yes. Press the save config button. It will save your config file where-ever you tell it to save it. I recommend you save it in the default folder.

####*Do I have to install my personal mods/configs myself after this is done?*
  No :) You can put your mods in the "RelHaxUserMods folder", and the installer will add them to the "User Mods" tab. You can install them just like they were regular mods. You can even use it to patch files with the installer's patching system, and install fonts.

####*How can I use the "auto-install" option?*
  You need to create a shortcut to the application. Right click it, properties. In the target textbox, append "/auto-install config_file.xml", where:
  - "auto-install "(<--note the space, required!) is the command
  - "config_file.xml" is the filename of your saved config preference file you made from the mod selection window. The config MUST be in the folder "RelHaxUserConfigs" for this feature to work!

####*Why is the downloading so slow? I know I have a fast connection!*
  This is because I had to purchase a private file hosting server for the modpack. It's a low end service, and has a speed limitation of about .5 MB/s. There's not much I can do about it, because it is a cheap server and I didn't want to invest a large amount of money into a free/donate supported project. Thank you for understanding this.
  
####(1)Performance measurements:

#####Time from program execution to mod selection on a hard drive:

  OMC: 52 seconds
  
  Aslains: 46 seconds
  
  Relhax: 8 seconds - **5.75X (475%) faster**


#####Time from mod selection to install completion (installing the same number of similar mods) on a hard drive:

Mod Selection: Zoom 100m patch, Relhax Sound Mod (or equivalent)

  OMC: 10 seconds
  
  Aslains: 9 seconds
  
  Relhax: 4 seconds - **2.25X (125)% faster**
  
---
This puts a whole new meaning to your quote in my form sig ScoutCub, <3 you plz don`t kick me

The Relhax Modpack ~ *"Would you rather spend time on your mods, or your gameplay?"* ~

Disclaimer:

I am in contact with WarGaming about the modifications (mods) and their configurations (configs) to ensure that they are legal and allowed in the game. There are no "Hacks" in the relhax modpack, the name is used for artistic license.
