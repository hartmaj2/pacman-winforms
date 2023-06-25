# Pacman
Jan Hartman, 1st year, group 38  
Summer Semester 2022/23  
Programming NPRG031

# Introduction
This program is supposed to be a simplified Pacman game for my university project. I am hoping to be able to implement all the well knows features such as Pacman preemptive direction setting and some simple ghost AI. The game is made in ***C# using Windows Forms***. That is because I didn't have time to think about anything better suited for this task. Later I realized maybe I could have used WPF but I would have to learn to use that.

# Class overview
The program is composed of two main files. The first one is `Form1.cs` and the second one is `Game.cs`. The former is a necessary file used by Windows Forms to create the form. The second one takes care of all the internal game logic. 

# Input data

## Map
The map is input as a text file. To be concrete it is a character grid where each represents a different game object. The file where this map is stored is called `map.txt`

## Game object sprites
The sprites of the game objects are simple 24x24 pixel PNG images.

