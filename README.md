# GREEDY GAME

_The only things that I **regret** in doing this project are **the risks** that I did **not** want to **take**._   

## Table of Content
- [Overview](#Overview)
- [Game Instruction](#Game-Instruction)
- [Installation](#Installation)
- [Notes](#Notes)
- [Acknowledgement](#Acknowledgement)

## Overview
- **Greedy Game** is a distributed application that implement ~~one of the most famous games of all time~~, the [Greedy Game](#Game-Instruction) 😉.
- The application does not support, and does not need any chatting methods to operate.  
- The application is designated to prevent all possible cheating methods, including _turning-off-your-opponent's-computer_ method.

## Game Instruction
A short instruction and game play can be found at https://youtu.be/-BoNUtau978

- A greedy game requires 3 - 7 players.
- There is a target score needed to be set before starting the game. The target score is used to determine when the game ends.
- In a round, each player is provided with the same set of cards. The number of cards is equal to the number of players divided by 2.
  - If the number of players is odd then the result is rounded up.  
  - The values of the cards are unique, starting from 1 and counting up by 1.
  - A player can only pick 1 cards, and keep it secret from other players.
- After every player has picked their cards, they will show their cards to the others. 
- The one who picked a unique card wins the round. The value of his/her picked card is counted toward his/her total point.  
  - If there are many unique cards, then who picked the highest value among these unique cards is the winner.  
  - If there is no unique card, there is no winner.
- If the round winner's total point is equal to or greater than the target score then that person is the game winner. The game ends. Otherwise, a new round starts.

> **TL;DR**: Just pick a random card, it all **depends on luck** 😂.

## Installation
- Clone this repo
- Change the settings in the `App.config` files in the [GreedyGameService](./GreedyGameService/App.config) and [GreedyGameClient](./GreedyGameClient/App.config) projects to accommodate your configuration.
- Build the solution with Visual Studio, which requires .NET Framework 4.8.
  - GreedyGameLibrary project provides the library for both GreedyGameService and GreedyGameClient projects.
  - GreedyGameService project provides the service to host the game as a console application.
  - GreedyGameClient project provides the GUI to play the game as a desktop application.

## Notes
- The application is written in C#, targetting exclusively Windows platform.
- The service is built with [WCF][wcf-link], located in the **GreedyGameService** folder.
- The GUI is built with [WPF][wpf-link] (before it dies 🙃), located in the **GreedyGameClient** folder.
- The library is built with .NET Framework 4.8, located in the **GreedyGameLibrary** folder.

## Acknowledgement
- Greedy Game application is a project of the INFO-5060 - Component-based Programming with .NET course at Fanshawe College.
- Special thanks to Professor Tony Haworth for his detailed instruction.
- Special thanks to [MYTH & ROID][myth-and-roid-wiki] and [OxT][oxt-wiki] to keep me sane while doing the project.

[wcf-link]: https://docs.microsoft.com/en-us/dotnet/framework/wcf/whats-wcf
[wpf-link]: https://docs.microsoft.com/en-us/visualstudio/designers/getting-started-with-wpf?view=vs-2019
[myth-and-roid-wiki]: https://en.wikipedia.org/wiki/Myth_%26_Roid
[oxt-wiki]: https://en.wikipedia.org/wiki/OxT