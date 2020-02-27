namespace GameOfLife

open Aardvark.Base
open GameOfLife.Model

module Evolve = 

    let asSequence (gp: GameParams) (cellMap: Cell[]) =

        let countNeighbours x y =
            [| -1,-1; 0,-1; 1,-1
               -1, 0;       1, 0
               -1, 1; 0, 1; 1, 1 |]
            |> Array.sumBy (
                fun (dx, dy) ->
                    let x, y = (x + dx), (y + dy)
                    if cellMap |> Array.exists (fun c -> (c.X = x) && (c.Y = y) && (c.IsAlive))
                    then 1
                    else 0
            ) 

        let life x y isAlive =       
            match isAlive, countNeighbours x y with
            | true, 2 -> true
            | _, 3 -> true
            | _, _ -> false
        
        cellMap |> Array.map ( fun c -> { c with IsAlive = life c.X c.Y c.IsAlive } )

    let initialiseGrid (gp: GameParams) = 

        Array.allPairs [|(1)..(gp.Columns)|] [|(1)..(gp.Rows)|]
        |> Array.mapi (fun i (x, y) ->
            let cellState = GameLogic.randomBoolWithProbability gp.Probability
            let cell = { 
                id = i; 
                X = x; Y = y; Z = 1; 
                Coord = V3d((x * 30), (y * 30), (1)); 
                IsAlive = cellState 
            }
            string i,cell
        )