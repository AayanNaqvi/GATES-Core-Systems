# GATES
### 2D Infinite Dungeon Crawler
<div align="center">
  <img src="Media/GatesTitleScreen.png" alt="GATES gameplay preview" width="650" />
</div>

[![Play on GameJolt](https://img.shields.io/badge/Play_Now-GameJolt-2F7F6F?style=for-the-badge&logo=gamejolt&logoColor=white)](https://gamejolt.com/games/GATES/1079130)
![Unity](https://img.shields.io/badge/Unity-2D-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Status](https://img.shields.io/badge/Status-Completed-4c1?style=for-the-badge)

## Table of Contents
* [Overview](#overview)
* [Technical Specs](#technical-specs)
* [General Gameplay Showcase](#general-gameplay)
* [How to Play](#how-to-play)
* [Key Reflections](#key-reflections)
* [Conclusion](#conclusion)


## Overview
**Who knows how you got here?** All you can do now is fight, fight, fight! **GATES** is a 2D top-down dungeon crawler built in **Unity (C#)** where you push through an endless sequence of rooms packed with enemies and spawners. As you survive deeper into the run, rooms scale up in size and danger. Successfully defeating all enemies in a room rewards you with a safe-rooms containing item chests or an **Upgrade Totem** where you spend nuggets on gear upgrades. Be careful however, as dying means you restart from the start!

* **Core Loop:** Fight through randomized combat rooms > clear all enemies and spawners > enter a safe room > claim loot or spend nuggets at the Upgrade Totem > Go through one of many doors to advance to the next stage!
* **What This Repo Has:** This repository contains the main **C#** gameplay scripts, and doesn't include any game assets, art or music (to keep the repo lightweight and to also protect game assets!)
* **Final Result:** A completed playable release hosted on [GameJolt](https://gamejolt.com/games/GATES/1079130) 

## Technical Specs
* **Engine & Language:** Unity (2D), C#
* **Other Tools Used:**
  * LMMS (Music)
  * Piskel (Art)
  * Audacity/BFXR/Soundly (Sound Effects)
* **Core Systems:**
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

## How to Play!
### Gamejolt Download:
* Follow the [Link](https://gamejolt.com/games/GATES/1079130) to visit the GATES Gamejolt Page!
  * Download the current build listed as a .zip file (Only a Windows build exists as of now)
  * Unzip the file to a location on your computer
  * Run the .exe file in the folder
### Inspecting the Code:
* All important scripts are listed in (`Scripts`)
* (`Scripts`) contains many subfolders, each with names relating to the functions the scripts in the subfolder serve

## Key Reflections
There are a few key points/reflections regarding the development of this game and the repo:
* **Difficulty Progression:**
  * Balancing difficulty in a game where the order the player visits each room is variable means a lot of testing was required
  * Many rooms were cut, reworked, rebalanced in response to lots, lots, lots of play testing!
  * The end result is a game that (hopefully!) is balanced and fun to play through.
* **Controller Support:**
  * Designing UI/Controls that work well on both Controller and Keyboard/Mouse was very challenging
  * Creating specific button binds for many UI actions helped alleviate some of this challenge
  * The end result is a game that (hopefully!) works well with whatever input the player prefers
* **Items and Combat:**
  * Separating items to their own inherited classes allowed adding and removing items to be way more scalable and easier
* **Certain elements being omitted from this repo**
  * All art, music, sound effects, prefabs, and particle effects are all not present in this repo
  * Only the scripts were included to keep the repo lightweight, and to protect assets

## Conclusion
This project was very fun to work on and reinforced how much I love making and building games! Balancing and debugging at times was very tedious, but enjoyable nonetheless. Thank you for checking out this project! Feel free to download and play the game yourself on GameJolt, and leave feedback too!
