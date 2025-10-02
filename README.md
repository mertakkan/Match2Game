# Match 2 Game

This repository contains a Match-2 game developed as part of the Peak Mobile Case study. The project was built using **Unity 2021.3.18f1** and the C# programming language.

## 🚀 Overview

The core objective for the player is to match two or more adjacent cubes of the same color to clear them from the grid and complete level-specific goals. This project implements all the core mechanics, UI requirements, and special object behaviors (Rockets, Balloons, Ducks) outlined in the case study.

## ✨ Features

- **Dynamic Grid Structure**: The grid size (width and height) is fully configurable via the `GameConfig` file and supports rectangular layouts.
- **Editable Level Settings**: The number of moves, initial cube layout, and level goals (number of cubes, balloons, or ducks to collect) can be easily adjusted from the `GameConfig` for each level.
- **Responsive UI**: UI elements are dynamically anchored to support various screen resolutions and aspect ratios.
- **Core Game Mechanics**:
  - **Match**: Matching two or more adjacent cubes of the same color.
  - **Fill & Fall**: When cubes are cleared, the cubes above them fall to fill the empty space (Fill), and new cubes are generated from the top to fill the remaining grid (Fall).
- **Special Objects & Mechanics**:
  - **🚀 Rocket**: Matching 5 or more cubes creates a rocket at the tapped position with a random horizontal or vertical orientation. When activated, the rocket clears its entire row or column.
  - **🎈 Balloon**: An obstacle that falls with the cubes. It is destroyed when an adjacent match is made. Can be included as a level goal.
  - **🦆 Duck**: An object that can fall with the cubes. It is collected and removed from the grid upon reaching the bottom row. Can be included as a level goal.
- **Effects & Animations**:
  - Includes **particle and sound effects** for cube explosions, rocket activations, and goal collection.
  - Features animations for collected goals flying to the UI and for the rocket's movement.
- **Dynamic Frame**: The border frame around the grid dynamically scales based on the grid's dimensions.

## 🔧 Technical Details & Architecture

The project was developed with a focus on creating a clean, scalable, and extensible codebase. The main components are:

- **GameManager**: The central coordinator that manages the game state (moves, goals, active status) and orchestrates other managers.
- **GridManager**: Handles grid creation, cube placement, match detection, gravity simulation, and the logic for all special objects (Rockets, Balloons, Ducks).
- **UIManager**: Updates all UI elements, such as the move counter and goal progress, and displays the game-over screens (Win/Lose).
- **AudioManager**: Plays all in-game sound effects (explosions, collections, etc.).
- **EffectsManager**: Manages and spawns particle effects for explosions and other events.
- **GameConfig (ScriptableObject)**: A central asset that holds all game settings (grid size, moves, goals, sprites, audio clips, etc.). This allows for easy game balancing and level design without changing a single line of code.
- **Extensibility**: The architecture is designed to allow for easy integration of new mechanics. The `GridCell` class is structured to support various types of objects, making it simple to add new special items or obstacles.

## ⚙️ Setup & Running

1.  Open the project in Unity Hub. Ensure you are using **Unity version 2021.3.18f1**.
2.  Navigate to the `Assets/Scenes` folder and open the main game scene.
3.  Press the **Play** button in the Unity Editor to run the game.
4.  To create a build, go to **File > Build Settings** and select either **iOS** or **Android** as the target platform.

## 🎮 How to Change Level Settings

All game and level parameters can be modified through the **GameConfig** asset located in the `Assets/Resources` folder:

- **Grid Settings**: Set the grid's width and height.
- **Gameplay Settings**: Adjust the number of moves per level, the match size required to trigger a rocket, and the number of balloons and ducks in the level.
- **Level Goals**: Define the level's objectives, such as how many cubes of a certain color, balloons, or ducks need to be collected.
- **Sprites, AudioClips, etc.**: All visual and audio assets for the game can be assigned here.```
