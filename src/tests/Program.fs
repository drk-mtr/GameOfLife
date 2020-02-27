open System
open System.Collections
open GameOfLife
open GameOfLife.Model


[<EntryPoint>]
let main argv =
    
    let gameParameters = {
        Rows = 5
        Columns = 5
        Probability = 0.2
        Speed = 500
    }
    
    let initGrid = 
        Evolve.initialiseGrid gameParameters
        |> Array.map snd

    let getBit (b: byte) (bitNumber: int) = 
        b &&& (1uy <<< (bitNumber - 1))

    let isAlive (bite: byte) =
        if (bite &&& 1uy) = 1uy
        then "X"
        else "-"

    let toBitArray (bite: byte) : BitArray = BitArray [| bite |]

    let formatByte (bite: byte) = 
        seq { for b in (toBitArray bite) -> if b then "1" else "0" }
        |> Seq.rev
        |> String.concat ""

    let printByte (bite: byte) = 
        printfn "%s\n" (formatByte bite )

    let printGridWith (rows,cols) title (grid: byte array) = 
        printfn "\n--- %s ---" title
        for r in [0..(rows - 1)] do
            printfn ""
            for c in [0..(cols - 1)] do
                printf "%s" (grid.[(r * cols) + c] |> isAlive)
        printfn ""

    let printByteGridWith (rows,cols) title (grid: byte array)  = 
        printfn "\n--- %s ---" title
        for r in [0..(rows - 1)] do
            printfn ""
            for c in [0..(cols - 1)] do
                printf "%A" (grid.[(r * cols) + c] |> formatByte)
        printfn ""

    let simpleThreeByThree = [| 
        0uy; 0uy; 0uy;
        1uy; 1uy; 0uy;
        0uy; 0uy; 0uy;
    |]

    let simpleFiveByFive = [| 
        0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 1uy; 1uy; 1uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 
    |]

    let simpleSevenBySeven = [| 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 1uy; 1uy; 1uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
        0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 
    |]

    // let initCellMap = 
    //     EvolveOptimised.initPlanned 
    //         gameParameters 
    //         // simpleThreeByThree
    //         simpleFiveByFive
    //         // simpleSevenBySeven
    // // let initCellMap = (EvolveOptimisedLess.init gameParameters)
    // // printByteGrid "initCellMap" initCellMap

    let printGrid = printGridWith (gameParameters.Rows,gameParameters.Columns)
    let printByteGrid = printByteGridWith (gameParameters.Rows,gameParameters.Columns)

    // // printByteGrid "initCellMap" initCellMap

    // let doGen (inputCellMap: byte array) i= 
    //     let nextGen = EvolveOptimisedLess.nextGeneration gameParameters (inputCellMap)
    //     printGrid (sprintf "nextGen%i" i) nextGen
    //     // printByteGrid (sprintf "nextGen%i" i) nextGen
    //     nextGen

    // [1..2] |> List.fold doGen initCellMap |> ignore

    let initCellMap = 
        EvolveOptimised.initPlanned 
            gameParameters 
            // simpleThreeByThree
            simpleFiveByFive

    printByteGrid "initCellMap" initCellMap
    printGrid "initCellMap" initCellMap

    let doGenOptimisedMore (inputCellMap: byte array) i = 
        let nextGen = EvolveOptimised.nextGeneration gameParameters (inputCellMap)
        printGrid (sprintf "nextGen%i" i) nextGen
        printByteGrid (sprintf "nextGen%i" i) nextGen
        nextGen

    [1..3] |> List.fold doGenOptimisedMore initCellMap |> ignore

    0
