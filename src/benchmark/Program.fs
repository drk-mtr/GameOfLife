namespace GameOfLife.Benchmarks

open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Engines
open GameOfLife
open GameOfLife.Model

[<MemoryDiagnoser>]
type EvolveTests() = 

    let gameParameters = {
        Rows = 64
        Columns = 64
        Probability = 0.5
        Speed = 500
    }
    
    let initGrid = 
        Evolve.initialiseGrid gameParameters
        |> Array.map snd

    let numberOfIterations = 20

    [<Benchmark>]
    member _.SimpleArray() = 
        [|1..numberOfIterations|] 
        |> Array.fold 
            (fun acc e -> Evolve.asSequence gameParameters acc) 
            initGrid

    [<Benchmark>]
    member _.OptimisedArray() = 
        [|1..numberOfIterations|] 
        |> Array.fold 
            (fun acc e -> EvolveOptimised.nextGeneration gameParameters acc) 
            (EvolveOptimised.init gameParameters)

module Program = 

    [<EntryPoint>]
    let main argv =

        BenchmarkRunner.Run<EvolveTests>() |> ignore

        0
