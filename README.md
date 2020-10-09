# The Relhax Modpack
The fastest WoT modpack installer in the world. A refresh of OMC modpack

**[Skip to download link](https://github.com/Willster419/RelhaxModpack#download)**

**[Virustotal scan](https://www.virustotal.com/#/file/f4e7b13d8e188fff7e604802a96acc43842394cf5d7d94ad82ffce0d20e86b6e/detection)**

[Visit our Website!](https://relhaxmodpack.com/)

[License Information](https://github.com/Willster419/RelhaxModpack/blob/master/LICENSE)

[Discord Support Server](https://discordapp.com/invite/58fdPvK)

### Description and reason for development
  This project is in dedication of the RELIC Gaming Community, as well as the OMC modpack dev team (which includes me, :)). A big thank you to them and their original work, especially to grumpelumpf. He gave the idea and design of the database driver approach, and is the reason why this modpack is where it is today. His work done on the OMC modpack over the years will not be forgotten.

When I looked at the current modpack installers, they all look the same: in size, in UI, and were all made with the inno setup creator. Many describe these installers as "clunky and slow". Trying to get that perfect mod setup can sometimes take hours. Have you ever had a "modpack setup night"?

The goal of this project is to redefine what a modpack installer is, from the templace UI to the install engine. Instead of using inno setup, I decided to create my own install engine from scratch in Microsoft's C# programming language. Some of the UI features in this modpack are new(tab category view), and some are kept in line with previous modpacks (right click to preview).
  
## Modpack Features
### Why would you use this modpack over Aslain's/OMC's/any other inno setup template modpack?
- Improved UI:
  - Instead of a giant unscrollable list of hard-to-find mods to select from, the mods are presented in tabs, each tab page being a mod catagory. Xvm has a tab page, garage stats have a page, damagelogs have a page, etc. Mods per tab are sorted alphabetically
  - For the first time in modpack history, there is a search feature where you can search for "that one mod" you want.
  - There are multiple views to display the mod selection list in. Currently we have OMC legacy view and the Relhax default view
  - The application allows for DPI and font based application scaling. It is also 4K display ready.
  - The Mod selection window and mod preview window are resizeable. The application will remember your last window settings and apply them upon loading the selection list
  - The font can be changed to comic sans. This is a critical feature.
  - The modpack can inform you if your local installation is out of date. This saves you from running a useless installation.
  - The OMC mod preview window has been re-designed, while keeping the familiar user interface:
    - The preview window supports image links, sound file links, webpage links, and direct HTML code.
    - Pictures load asynchronously. This means that The UI does not lock up waiting for the picture to load.
    - The preview window is web-based, meaning you hard drive won't become cluttered with cached pictures.
    - Each mod or config can have up to 1.2 million pictures. Other modpacks have only a few, or only 1 picture​
- Mod selections can be saved:
  - Like OMC and Aslains, your mods selection can be saved.
  - Unlike Aslains, you can save as many mod selections as you want, and save them where ever you like
  - Unlike OMC, you can use this selection file to automate the install process (See Automation Section)
- Performance:
  - For the first time in modpack history, the installation process is multi-threaded, meaning that it can install multiple mods at once. The install process is optimized for 8 core systems.
  - The loading and installation times of this modpack vs. inno setup modpacks have been reduced by up to  **89%** and **60%** on a standard hard drive using the standard extraction mode. The times are even further reduced for those with WoT installed on an SSD using the multicore extraction mode.
- Automation:
  - The modpack and be set at command line with a "/auto-install config_file_name.xml" switch to automatically install the modpack, with your preference of mods selected. In this situation, you could install without any interaction, and update all your mods in seconds.
  
### What does the modpack look like?
![MainWindow](https://i.imgur.com/tR0Nn2M.png "Main Window")

![MainWindow](https://i.imgur.com/SQTj4pk.png "Main Window2")

![MainWindow](https://i.imgur.com/LsVbDkS.png "Main Window3")

![ModSelection](https://i.imgur.com/n1T8OQI.png "Mod Selection")

### Prefer the OMC style selection view?
![ModSelectionOld](https://i.imgur.com/gh3hdNO.png "Mod Selection Legacy")

![ModSelectionOld](https://i.imgur.com/5jafeBd.png "Preview")

![ModSelectionOld](https://i.imgur.com/A5zf8LI.png "Preview2")

### Additional Information
  
**If you come across a bug or feature request please take one of these actions:**
- Record it here:
http://forums.relhaxmodpack.com/
- (If you have a github account) open an issue.

**When you do, please attach the diagnostic zip file you create by clicking diagnostic utilities->create zip file**

If you want to help develop the modpack, I would be glad for the help and will help set you up with an environment.

Latest release notes can be found here:
https://github.com/Willster419/RelhaxModpack/commits/master

## Download
You can download the modpack from this link:

http://adf.ly/1l28oP (donate link)

http://bigmods.relhaxmodpack.com/RelhaxModpack/RelhaxModpack.exe (direct link)

https://wgmods.net/392/ (WG mods link)

If you can spare a few dollars and like the Modpack, please consider donating:
https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=76KNV8KXKYNG2

## Credits
 - OMC Modpack for giving me an internal structure to start with and improve upon, along with several premade zip files to start with.
 - Rkk1945 For helping with code edits and resource support, along with helping with the closed alpha test.
 - All the Modpack team members for helping to add mods as quickly as possible.
 - Those who helped provide feedback during open alpha and beta testing.

## FAQ

#### *Why is there such a difference in install times?*
 While inno setup single-threaded (as far as I can tell), mine is not. What does that mean? Looking at mod zip extraction, for example, when a file is extracted, it is reported to the GUI in a synchronous manner. This means that the modpack can only extract as fast as it can pump events to the GUI for each entry extracted in a zip file. This is where multi-threading come in. You can create a separate thread and have it only extract, and asynchronously report the progress to the GUI. This means three things:
 - The GUI is not laggy during install
 - The install is not frozen when the ui thread is blocked, like moving the window for example
 - The extraction is limited to your hard drive speed, not the processor GUI reporting speed.
This can be further exploited. If several mods do not interact with each other, (no file over-writes), why not install the mods at the same time? Concurrent installation can reduce install time by up to 80% (Depends on HDD/SSD performance).
If you want more information, I encourage you to review this presentation:
https://docs.google.com/presentation/d/1H-6YLcEP3XfxeEhF21grP7Ypfw2im300201bz0NAuJI/edit#slide=id.g7bf2f002c6_0_83

#### *I have a perfect selection of mods that I want. Can I save this selection?*
  Yes. Press the save selection button. It will save your config file wherever you tell it to save it. I recommend you save it in the default folder.

#### *Do I have to install my personal mods/configs myself after this is done?*
  No :) You can put your mods in the "RelHaxUserMods folder", and the installer will add them to the "User Mods" tab. You can install them just like they were regular mods. You can even use it to patch files with the installer's patching system, and install fonts.

#### *How can I use the "auto-install" option?*
   From the advanced settings tab, load a selection file and check the "auto installation" checkbox in advanced settings. The modpack will automatically check with the server (beta or stable database) for a new db version, and if one is found, an installation is started.
  
#### (1)Performance measurements:
Hard drive used is a Toshiba 5400 RPM 2.5 inch laptop hard drive, 8MB cache
##### Time from program execution to mod selection on a hard drive:

  OMC: 48 seconds
  
  Aslains: 24 seconds
  
  Relhax: 5 seconds - **Time reduced by 89% from OMC and 80% from Aslains**


##### Time from mod selection to install completion (installing the same number or similar mods) on a hard drive:

Mod Selection: Zoom 100m patch, Relhax Sound Mod (or equivalent)

  OMC: 10 seconds
  
  Aslains: 9 seconds
  
  Relhax: 4 seconds - **Time reduced by 60% from OMC and 56% from Aslains
  
  
---
The Relhax Modpack ~ *"Would you rather spend time on your mods, or your gameplay?"* ~

Disclaimer:

There are no "Hacks" or illegal mods in the modpack, the name is used for artistic license. This modpack complies with WG's fair play policy. We try to keep up to date as much as we can with their policy, but this is by no means an official guarantee.
