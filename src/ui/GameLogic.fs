namespace GameOfLife

open System
open GameOfLife.Model

module GameLogic = 

    let rnd = System.Random()
    let randomBool() = rnd.NextDouble() > 0.5
    let randomBoolWithProbability (p) = rnd.NextDouble() < p // TODO: Flawed at 0.5 point?