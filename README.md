# Custom Fixed Update

## Description
The CustomFixedUpdate repository contains scripts that extend Unity's PlayerLoop to implement a custom fixed update system with adjustable update periods. This system allows developers to define their own fixed update logic and dynamically change the update period during runtime.

## Features
- **CustomFixedUpdate Script:** Implements a custom fixed update system that triggers at specified intervals.
- **PeriodClr Script:** Provides a MonoBehaviour script for toggling between two different update periods for the custom fixed update system.
- **PlayerLoopExt Class:** Extends Unity's PlayerLoop to integrate the custom fixed update system seamlessly into the game's update loop.
- **Optimized Code:** Scripts are optimized for performance and readability to ensure efficient execution and easy maintenance.

## Usage
1. Clone or download the repository.
2. Import the provided scripts into your Unity project.
3. Attach the `PeriodClr` script to a GameObject in your scene to control the update period dynamically.
4. Customize the fixed update logic in the `PlayerLoopExt` class according to your project requirements.
5. Use `PlayerLoopExt.SetUpdatePeriod(period)` to set the update period programmatically if needed.

## How to Use PeriodClr Script
1. Attach the `PeriodClr` script to a GameObject in your scene.
2. Press the space key during gameplay to toggle between two different update periods for the custom fixed update system.
