
Table of Contents
•Technology
•Introduction
•Background
?Housekeeping
?Motivation
?Scope
•How it works - The Big Picture
?Delegates
?Threading
•Examples
•Initialization
•Game Loop
•Cycle Managers
•Summary

Technology
•OpenTK
•.NET Framework 3.5 SP1

Introduction
Migration is a simulation strategy game. The goal of the game is to build a community of workers that perform individual tasks of building a new colony. The game is controlled by a mouse-operated point-and-click interface. The player cannot directly control workers, but instead places orders to construct buildings, manage the manufacture and distribution of goods and attack opponents. 

There are 32 kinds of building resources and 11 types of migrants. Idle migrants are recruited to specialized roles when new buildings are finished. For example, a blacksmith appears once a smithy is constructed. Some migrants require specific tools for these roles. A toolmaker's building can create additional tools, and the proportions of each tool being created is controlled by the player.

At the start of the game the player chooses the location of their castle, which houses the initial settlers and stockpiles. If placement of buildings and roads is not carefully planned it may lead to traffic congestion. If no counter action is taken (re-routing the goods, constructing more warehouses, better placement of buildings), such single bottlenecks can have a distributed effect across the network, leading to shortages because goods can not reach their destination fast enough.


Background

Housekeeping
Ultimately, first, and foremost, I need to acknowledge the original author of this codebase, Christoph Husse. The original C# code can be found here[^]. It was originally titled Monostrategy[^] His project was intended to be an OpenSource remake of one of the best multiplayer real time strategy games in the world, "The Settlers". [^]. 


Motivation
For years I've admired games like Maxis' SimCity and Sid Meier's Civilization?. I've spent countless hours playing and strategizing with those games. However, I have always wanted to 'tweak' them just a bit. 

I have always desired to have my own version of these titles. I just never knew where to start. Coming from a business programming background I could never understand how to get the game loops right. There were never any good examples in VB which is my language of choice. Therefore, I decided to open source this game in VB.NET in an attempt to help others, and perhaps spur some ideas, and understanding about how to create your own Empire Strategy type game. 


Scope
The scope for this article is Gaming Loops. In particular, I am going to talk about Game Loops as it relates to VB.net. Maybe in a follow up article I will talk about the details, classes, and the interrelationships between those classes.

However, for now, I will stick to the biggest hiccup in creating strategy games. The central component of any game, from a programming standpoint, is the game loop. The game loop is what allows the game to run smoothly regardless of a user's of a user's input or lack thereof.

Of course, in business software programming we never really have to worry much about this concept. For the most part we design systems that 'React' to user events. (i.e. Button push, text box entered, etc..). Most traditional business software programs respond to user input and do nothing without it. For example, a word processor formats words and text as a user types. If the user doesn't type anything, the word processor doesn't do anything. Functions may take a long time to complete, but they are all initiated by a user telling the program to 'do' something. Games, on the other hand, continue to operate regardless of a user's input.

This is what the game loop allows. That was a tremendous learning curve for me. Hopefully, this article will help to ease that learning curve for you as you move into or think about moving into game programming.

Note: Please note that this work is in no way complete. There is much work left to be done. 
Bookmark: I will try and update this article as I get around to fixing certain bugs.


Installation
Step 1
Initially once you download the Migration Source and unzip you will end up with the following directory layout:
 
Step 2
Next you need to make sure you Download *Required* Resources and the support files. Unzip the content of both files into your "Build" directory. 
 
Step 3
Ensure that your "Build" directory has all of the files necessary. Ignore the "textures.cache" file it will be regenerated on program startup. It should look similar to the picture below:
 
Step 4
Ensure that your "Resources" directory looks like below. If not be sure and Download *Required* Resources
 

Summary
A game loop has more to it than you think. The game loop is the heartbeat of every game, no game can run without it. But unfortunately for every new game programmer, there aren?t any good VB.NET articles on the internet that provide information on this topic. We've reviewed an implementation of using delegates and threading in VB.NET for a strategy game.

Hopefully, this article has inspired some VB.NET coders to go out and start coding that next generation fantastic game. 


Copyrights
In addition to all previous copyrights I also acknowledge the graphic/media contributions of Ubisoft and BlueByte.


License
This article, along with any associated source code and files, is licensed under GNU General Public License version 3.0 (GPLv3).
