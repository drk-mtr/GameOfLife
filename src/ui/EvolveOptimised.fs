namespace GameOfLife

open Aardvark.Base
open System
open FSharp.Core
open GameOfLife.Model

[<RequireQualifiedAccess>]
module EvolveOptimised = 

    let initCellmap (gp: GameParams) =
        let cellMapLength = gp.Rows * gp.Columns
        if cellMapLength > 1048576 
        then failwith "Array was too long for ArrayPool i.e. greater than 1048576 bytes" // TODO: Redundant
        else Array.zeroCreate<byte> cellMapLength

    let setCellImmutable (gp: GameParams) (x: int, y: int) (cells: byte array) =
        let w,h = gp.Columns,gp.Rows
        let cellPtr = (y * w) + x

        let xoleft =  if (x = 0) then w - 1 else -1
        let yoabove = if (y = 0) then (gp.Rows * gp.Columns) - w else -w
        let xoright = if (x = (w - 1)) then -(w - 1) else 1
        let yobelow = if (y = (h - 1)) then -((gp.Rows * gp.Columns) - w) else w

        cells.[cellPtr] <- cells.[cellPtr] ||| 1uy
        cells.[cellPtr + yoabove + xoleft] <- (cells.[cellPtr + yoabove + xoleft]) + 2uy
        cells.[cellPtr + yoabove] <- (cells.[cellPtr + yoabove]) + 2uy
        cells.[cellPtr + yoabove + xoright] <- (cells.[cellPtr + yoabove + xoright]) + 2uy
        cells.[cellPtr + xoleft] <- (cells.[cellPtr + xoleft]) + 2uy
        cells.[cellPtr + xoright] <- (cells.[cellPtr + xoright]) + 2uy
        cells.[cellPtr + yobelow + xoleft] <- (cells.[cellPtr + yobelow + xoleft]) + 2uy
        cells.[cellPtr + yobelow] <- (cells.[cellPtr + yobelow]) + 2uy
        cells.[cellPtr + yobelow + xoright] <- (cells.[cellPtr + yobelow + xoright]) + 2uy
        cells

    // Calculates and displays the next generation of current_map 
    let nextGeneration (gp: GameParams) (readOnlyCellMap: byte array) =

        let cellMapLength = gp.Rows * gp.Columns // TODO: Safer to use readOnlyCellMap?
        let mutable cells = Array.zeroCreate cellMapLength
        Array.Copy(readOnlyCellMap,cells,cellMapLength)

        let w,h = gp.Columns,gp.Rows
        let maximumCellPtr = cellMapLength - 1

        let mutable cellPtr = 0
        let mutable x = 0
        let mutable y = 0

        // Turns an on-cell off, decrementing the neighbour count for the eight neighbouring cells
        let clearCell() =
            let cellPtr : int = ((y * w) + x)

            let xoleft = if (x = 0) then w - 1 else -1
            let yoabove = if (y = 0) then (gp.Rows * gp.Columns) - w else -w
            let xoright = if (x = (w - 1)) then -(w - 1) else 1
            let yobelow = if (y = (h - 1)) then -((gp.Rows * gp.Columns) - w) else w
            
            cells.[cellPtr] <- cells.[cellPtr] &&& 254uy
            cells.[cellPtr + yoabove + xoleft] <- (cells.[cellPtr + yoabove + xoleft]) - 2uy
            cells.[cellPtr + yoabove] <- (cells.[cellPtr + yoabove]) - 2uy
            cells.[cellPtr + yoabove + xoright] <- (cells.[cellPtr + yoabove + xoright]) - 2uy
            cells.[cellPtr + xoleft] <- (cells.[cellPtr + xoleft]) - 2uy
            cells.[cellPtr + xoright] <- (cells.[cellPtr + xoright]) - 2uy
            cells.[cellPtr + yobelow + xoleft] <- (cells.[cellPtr + yobelow + xoleft]) - 2uy
            cells.[cellPtr + yobelow] <- (cells.[cellPtr + yobelow]) - 2uy
            cells.[cellPtr + yobelow + xoright] <- (cells.[cellPtr + yobelow + xoright]) - 2uy

        // Turns an off-cell on, incrementing the neighbour count for the eight neighbouring cells
        let setCell() =
            let cellPtr = (y * w) + x

            let xoleft = if (x = 0) then w - 1 else -1
            let yoabove = if (y = 0) then (gp.Rows * gp.Columns) - w else -w
            let xoright = if (x = (w - 1)) then -(w - 1) else 1
            let yobelow = if (y = (h - 1)) then -((gp.Rows * gp.Columns) - w) else w

            cells.[cellPtr] <- cells.[cellPtr] ||| 1uy
            cells.[cellPtr + yoabove + xoleft] <- (cells.[cellPtr + yoabove + xoleft]) + 2uy
            cells.[cellPtr + yoabove] <- (cells.[cellPtr + yoabove]) + 2uy
            cells.[cellPtr + yoabove + xoright] <- (cells.[cellPtr + yoabove + xoright]) + 2uy
            cells.[cellPtr + xoleft] <- (cells.[cellPtr + xoleft]) + 2uy
            cells.[cellPtr + xoright] <- (cells.[cellPtr + xoright]) + 2uy
            cells.[cellPtr + yobelow + xoleft] <- (cells.[cellPtr + yobelow + xoleft]) + 2uy
            cells.[cellPtr + yobelow] <- (cells.[cellPtr + yobelow]) + 2uy
            cells.[cellPtr + yobelow + xoright] <- (cells.[cellPtr + yobelow + xoright]) + 2uy
        
        for i = 0 to maximumCellPtr do

            while cellPtr <= maximumCellPtr && readOnlyCellMap.[cellPtr] = 0uy do

                if ((x + 1) >= w) 
                then 
                    y <- y + 1
                    x <- 0
                    cellPtr <- (cellPtr + 1)
                else
                    x <- x + 1
                    cellPtr <- (cellPtr + 1)

            if (cellPtr) <= maximumCellPtr
            then
                // Number of neighbouring on-cells
                let count = readOnlyCellMap.[cellPtr] >>> 1

                if ((readOnlyCellMap.[cellPtr] &&& 1uy) = 1uy) 
                then 
                    // Cell is on; turn it off if it doesn't have 2 or 3 neighbours
                    if (count <> 2uy) && (count <> 3uy)
                    then clearCell()
                else 
                    // Cell is off; turn it on if it has exactly 3 neighbours
                    if (count = 3uy) 
                    then setCell()

                cellPtr <- cellPtr + 1
                
                if ((x + 1) >= w) 
                then 
                    y <- y + 1
                    x <- 0
                else
                    x <- x + 1

        cells

    let init (gp: GameParams) =
        let cells = initCellmap gp
        Array.allPairs [|0..(gp.Columns - 1)|] [|0..(gp.Rows - 1)|] 
        |> Array.iter (fun (col,row) ->
            if GameLogic.randomBoolWithProbability gp.Probability
            then setCellImmutable gp (row,col) cells |> ignore
        )
        cells

    let initPlanned (gp: GameParams) (arr: byte array) =
        let cells = initCellmap gp
        Array.allPairs [|0..(gp.Columns - 1)|] [|0..(gp.Rows - 1)|] 
        |> Array.iteri (fun i (col,row) ->
            if arr.[i] = 1uy 
            then setCellImmutable gp (row,col) cells |> ignore
        )
        cells