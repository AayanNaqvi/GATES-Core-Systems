# GATES
### 2D Infinite Dungeon Crawler
<div align="center">
  <img src="Media/GatesTitleScreen.png" alt="GATES gameplay preview" width="650" />
</div>

[![Play on GameJolt](https://img.shields.io/badge/Play_Now-GameJolt-2F7F6F?style=for-the-badge&logo=gamejolt&logoColor=white)](https://gamejolt.com/games/GATES/1079130)
![Unity](https://img.shields.io/badge/Unity-2D-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Status](https://img.shields.io/badge/Status-Completed-4c1?style=for-the-badge)

## Overview
**Who knows how you got here?** All you can do now is fight, fight, fight! **GATES** is a 2D top-down dungeon crawler built in **Unity (C#)** where you push through an endless sequence of rooms packed with enemies and spawners. As you survive deeper into the run, rooms scale up in size and danger. Successfully defeating all enemies in a room rewards you with a safe-rooms containing item chests or an **Upgrade Totem** where you spend nuggets on gear upgrades. Be careful however, as dying means you restart from the start!

* **Core Loop:** Fight through randomized combat rooms > clear all enemies and spawners > enter a safe room > claim loot or spend nuggets at the Upgrade Totem > Go through one of many doors to advance to the next stage!
* **What This Repo Has:** This repository contains the main **C#** gameplay scripts, and doesn't include any game assets, art or music (to keep the repo lightweight and to also protect game assets!)
* **Final Result:** A completed playable release hosted on [GameJolt](https://gamejolt.com/games/GATES/1079130) 

## Technical Specs
* **Engine & Language:** Unity (2D), C#
* **Other Tools Used** LMMS (Music), Piskel (Art), Audacity/BFXR/Soundly (Sound Effects)
* **Core Systems & Keywords:**
  * Pseudo-Randomized Room progression
  * Autonomous enemy pathfinding (Made possible with [A* Pathfinding Project](https://arongranberg.com/astar/))
  * Inventory/Item system
  * Combat system (melee and ranged)
  * Custom sprite/animations
  * Controller support
* **Repository Scope:** C# source scripts (`Scripts/`) and documentation (`README.md`).Full compiled build available on GameJolt.

## General Gameplay
**GATES** is a top down game where you battle through infinite and random set of rooms full of enemies and spawners. As the player continues, there is a sense of difficulty progression as the rooms get bigger and more dangerous/lethal. After the player defeats all enemies and spawners in a room, they are rewarded with a safe room which either has a chest full of useful items or an **Upgrade Totem** to spend nuggets on useful upgrades to their gear. 

Here is a general gameplay video: (compressed to 720p 30fps to be under 10MB)

[Media/GATES Silent Gameplay-compressed.mp4](https://github.com/user-attachments/assets/317476eb-1851-46ad-aa27-31ff97bdc292)

