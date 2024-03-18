# Knights Duel

## Table Of Contents
- [About](#about)
- [Installation](#installation)
- [Technologies](#technologies)
- [General Architecture](#general-architecture)
- [Difficulties In The Development](#difficulties-in-the-development)

## About
Knights Duel is a portfolio game based on the usage and optimization of fishnet in Unity. You take control of a Knight fighting for his honor in a 1v1 arena where only the best one will leave alive

## Installation
```bash
git clone https://github.com/santinocasolati/Knights-Duel.git
```

## Technologies
1. Unity 2022.3.2f1
2. FishNet Framework
3. Cinemachine Package
4. Shapes 2D Package

## General Architecture
### Player
Each spawned player has 4 main components that control everything about them:
1. Player Controller: handles user inputs and the actions that players make. It implements Client Side Prediction to ensure an excellent client-side experience
2. Player Health Controller: handles all health-related things during the game: updating the hp bar, the damage dealt by a player and when a player is killed
3. Player Animations Controller: is called when an animation is suposed to fire, changes Animator parameters and sends the audio related to them to all clients (Example: when you swing the sword)
4. Sword Controller: handles the damage dealt to other players: area, activation, duration and damage amount

Inside a Player, components communicate using events and handlers. For example: when a player is killed, the Health Controller fires an event to the Animations Controller to play the death sound. There are also some FishNet components to syncronize the Transform and Animator. Both are using a client authority

### Managers
There are 5 main managers in the game
1. Game Manager: handles scores and win conditions
2. Server Connection Manager: handles the connection's calls between client and server (Connect, Host, Disconnect)
3. Audio Manager: it's main function is to play audios in the most efficient way possible and store audio referenes
4. Network Manager: contains all FishNet scripts

The managers use a singleton pattern to ensure that only one instance of each one is called. To call specific functionalities of them, static methods are used for a better code legibility. There are some exceptions to this rule: for example, the Game Manager fires an event when the win condition is fullfilled and the Server Connection Manager is subscribed to it to kill the client's connection to the server

## Difficulties In The Development
The intention with this project was to challenge myself: I've worked with multiplayer frameworks before this (like Netcode for GameObjects, Photon and Mirror) but this was my first time using FishNet. Much of my knowledge in Mirror was applied here: both frameworks share some things such as ClientRPC/ObserversRPC to clients, Commands/ServerRpc to the server and SyncVars. However, I had to learn a lot about Ownership and Client Side Prediction that I've never heard before. The official documentation was amazing with this: FishNet has everything arranged to be found with few clicks. Online resources of the community were also great, I found a lot of people that use this framework in many different ways that made me realize mistakes that were made. After all, this project was a fun way to learn a new technology and concepts that are going to be useful in the near future
