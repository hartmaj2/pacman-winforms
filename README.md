# Pacman
Jan Hartman, 1st year, group 38  
Summer Semester 2022/23  
Programming NPRG031

# Things to improve
- Implement state machine design pattern so I can better handle different game states (IMPLEMENT USING INTERFACES)
- Animations should be handled by objects themselves in some way (ADD ANIMATION CLASS AND SPRITE CLASS)
- Load images using their name from a file
- Automatic intersection identification

# Introduction
This program is supposed to be a simplified Pacman game for my university project. I am hoping to be able to implement all the well knows features such as Pacman preemptive direction setting and some simple ghost AI. The game is made in ***C# using Windows Forms***.

# Project overview
The project is composed of four main code files. These are:
- GameForm.cs
- GameManager.cs
- GameObjects.cs
- HelperClasses.cs
- InputManger.cs

## File Separation
The files are meant to separate logically different parts of the program. The main reason I did this was to not have to scroll all the way through the code to get to some parts of the program. This is also the reason why the classes should be separated logically. That will result in clearer orientation which in turn will make my code editing more efficient.

## Class list
The classes used in the program are: 
- GameForm.cs
  - FormConstantsManager
  - GameForm 
- InputManager.cs
  - InputManager
- GameManager.cs
  - GameManager
- GameObjects.cs
  - GameObject
  - StaticObject
  - DynamicObject
  - MovableObject
  - DiscreteMovableObject
  - TweeningMovableObject
  - StaticBlank
  - DynamicBlank
  - Wall
  - Hero
  - Pellet
  - Ghost
- HelperClasses.cs
  - Direction
  - Map
  - Painter

## Class intercommunication
The `GameManager` class is the one that puts most of all the other classes together. It takes care of the game logic such as player lives, score, etc. It also has reference to the `Map` object which stores two grids. One for the `StaticObject` instances and the second for `DynamicObject` instances. The static game objects are the ones, that don't ever change throughout the game like walls etc. The dynamic ones are things that don't move but are going to change like pellets or other power-ups.

## Specific class information
### GameForm.cs
#### FormConstantsManager
Holds the constants used by the `GameForm` class. These include form labels, button size settings, timer intervals, etc.

### GameForm
Takes care of the `Timer` to synchronize tick signals sent to `GameManager` which it also holds. Also takes care of the buttons. In the future, I may switch from using a Windows Forms `Timer` to implement a game loop to some hack I found on the internet.

### InputManager.cs
#### InputManager
Provides abstraction on the input data. Prepares all the needed data and then provides an interface through which the game classes can fetch this data. It has to take care of loading data from the `map.txt` file so they are ready to be passed to some other classes in the program. I am using a simple `bool` variable to check if the data is ready or not and if it is not I call a corresponding function to prepare the data.

### GameManager.cs
#### GameManager
Handles all the game logic and provides means of communication with the `Form`. It contains a `Map` reference so it can access all `GameObjects` that live on the game map. It also contains a `Painter` reference which it uses to paint on the form at predefined intervals.

### GameObject.cs
#### DiscreteMovableObject
Represents an object which can only move by one grid cell at a time and nothing in between. I may obsolete this class in the future as I am not sure if I will use it.

### TweeningMovableObject
These objects can move in between cells by a ratio of the cell size. It is important that the objects always finish one tween cycle exactly inside a certain cell and not in between two cells. Possible collisions with walls are calculated before a tween move cycle starts.

#### DynamicBlank and StaticBlank
I didn't like the idea of filling the grids with null values so I had to include two different objects that indicate that the cells are blank. The idea of having to be explicit about what kind of blank space it is should be helpful for future debugging. It will tell me more than just encountering a null value would if there is something wrong with the code.

### HelperClasses.cs
#### Direction
This one is actually a struct but I thought it would be wasteful to dedicate a special struct section just for this one. This helper class is especially in `Map` and the `GameObjects`. It is meant to provide means to work with simple 2D vectors with unit sizes. There is also an abstraction for the directions up, right, down, and left. For example, you can write `Direction.Left` to get the vector `new Direction(-1,0)`.

#### Map
Takes care of almost everything that has to do with the topological locations of different objects. Stores two 2D grids of `GameObject` instances and also a list of tweening movable objects.  

The first grid is only formed of static non-movable objects and is used to determine if a certain place on the grid is free to be moved to or not. The second grid is used by non-movable objects that change throughout the game like `Pellet` instances or other power-ups.  

On the other hand, the list stores instances of `TweeningMovableObject` which can move in between cells but always have to end their tweening cycle at a certain cell.

#### Painter
Provides means to paint objects that live on the game map to the screen. It uses buffering to draw the game world. When writing game objects to the screen, it doesn't do it directly. It first writes them to a private bitmap and only after all game objects are drawn it pushes this buffer to the actual screen and really draws it.

# Input data
## Map
The map is input as a text file. To be concrete it is a character grid where each represents a different game object. The file where this map is stored is called `map.txt`

## Game object sprites
The sprites of the game objects are simple 24x24 pixel PNG images.

