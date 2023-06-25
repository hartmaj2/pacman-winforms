# Pacman
Jan Hartman, 1st year, group 38  
Summer Semester 2022/23  
Programming NPRG031

# Introduction
This program is supposed to be a simplified Pacman game for my university project. I am hoping to be able to implement all the well knows features such as Pacman preemptive direction setting and some simple ghost AI. The game is made in ***C# using Windows Forms***. That is because I didn't have time to think about anything better suited for this task. Later I realized maybe I could have used WPF but I would have to learn to use that first and there is unfortunately no time.

# Project overview
The project is composed of four main code files. These are:
- Form1.cs
- GameManager.cs
- GameObjects.cs
- HelperClasses.cs
- InputManger.cs

## File Separation
The files are meant to separate logically different parts of the program. The main reason I did this was to not have to scroll all the way through the code to get to some parts of the program. This is also the reason why the classes should be separated logically. That will result in clearer orientation which in turn will make my code editing more efficient.

## Class list
The classes used in the program are:  
- InputManager
- GameManager
- GameObject
- StaticGameObject
- DynamicGameObject
- MovableGameObject
- StaticBlank
- DynamicBlank
- Wall
- Hero
- Pellet
- Ghost
- Map
- Painter

## Class intercommunication
The `GameManager` class is the one that puts most of all the other classes together. It takes care of the game logic such as player lives, score, etc. It also has reference to the `Map` object which stores two grids. One for the `StaticGameObject` instances and the second for `DynamicGameObject` instances. The static game objects are the ones, that don't ever change throughout the game like walls etc. The dynamic ones are things that don't move but are going to change like pellets or other power-ups.

## Specific class information
### InputManager
Provides abstraction on the input data. Prepares all the needed data and then provides an interface through which the game classes can fetch this data.

### GameManager
Handles all the game logic and provides means of communication with the `Form`.

### DynamicBlank and StaticBlank
I didn't like the idea of filling the grids with null values so I had to include two different objects that indicate that the cells are blank. The idea of having to be explicit about what kind of blank space it is should be helpful for future debugging. It will tell me more than just encountering a null value if there is something wrong with it.

### Map
Takes care of almost everything that has to do with the topological locations of different objects. Stores two 2D grids of `GameObject` instances and also a 'List' of 'MovableGameObjects'. The first grid is only formed of static non-movable objects and is used to determine if a certain place on the grid is free to be moved to or not. The second grid is used by non-movable objects that change throughout the game like `Pellet` instances or other power-ups.  

On the other hand, the list of movable characters is meant to be changing their locations constantly. This is one thing that the `Map` class doesn't take care of - the locations of these movable objects. They store their position themselves.

# Input data
## Map
The map is input as a text file. To be concrete it is a character grid where each represents a different game object. The file where this map is stored is called `map.txt`

## Game object sprites
The sprites of the game objects are simple 24x24 pixel PNG images.

