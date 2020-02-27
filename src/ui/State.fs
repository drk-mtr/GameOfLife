namespace GameOfLife

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open GameOfLife
open GameOfLife.Model
open GameLogic
open Operators

module State = 

    let initGameParameters = {
        Rows = 15
        Columns = 15
        Probability = 0.5
        Speed = 500 // In ms
    }

    let initialiseGridBytes gp = EvolveOptimised.init gp

    let drawGrid (gp: GameParams) (grid: byte array) = 
        Array.allPairs [|(1)..(gp.Columns)|] [|(1)..(gp.Rows)|]
        |> Array.zip grid
        |> Array.mapi (
            fun i (bite,(x,y)) ->
                let cellState = (bite &&& 1uy = 1uy)
                let cell = { id = i; X = x; Y = y; Z = 1; Coord = V3d(x * 30,y * 30,1) ; IsAlive = cellState }
                string i,cell
        )
        |> HMap.ofArray

    let initGameModel = {
        GameParametersConfig = initGameParameters
        GameParametersState = initGameParameters
    }    

    let initial = 
        let initGridBytes = initialiseGridBytes initGameParameters
        let initGrid = initGridBytes |> drawGrid initGameParameters
        { 
            GameModel = initGameModel
            CurrentModel = BoxArray
            CameraState = FreeFlyController.initial' 50.0
            Threads = ThreadPool.empty
            Timer = "0"
            Grid = initGrid
            GridBytes = initGridBytes
        }

    let update (m : Model) (msg : Message) =
        match msg with
        | ToggleModel -> 
            match m.CurrentModel with
            | Box -> { m with CurrentModel = Sphere }
            | Sphere -> { m with CurrentModel = Box }
            | BoxArray -> { m with CurrentModel = Box }

        | ApplyGameParameters -> 
            let newGridBytes = initialiseGridBytes m.GameModel.GameParametersConfig
            let newGrid = newGridBytes |> drawGrid m.GameModel.GameParametersConfig
            { m with Grid = newGrid; GridBytes = newGridBytes }

        | UpdateGameParameters newGameParametersConfig -> 
            let newGameModel = { m.GameModel with GameParametersConfig = newGameParametersConfig }
            { m with GameModel = newGameModel }

        | ToBoxArray -> 
            { m with CurrentModel = BoxArray }

        | CameraMessage msg ->
            { m with CameraState = FreeFlyController.update m.CameraState msg }

        | UpdateClock (newTime: string) -> 
            { m with Timer = newTime }

        | Done(id,r) -> 
            { m with Threads = ThreadPool.remove id m.Threads }

        | StartTimer -> 
            // TODO: Understand how to make these as singletons
            let id = System.Guid.NewGuid().ToString()
            let worker =  
                proclist {
                    for i in 0 .. 1000 do
                        do! Proc.Sleep m.GameModel.GameParametersConfig.Speed
                        yield Reset
                        yield UpdateClock( string (System.Random().Next()) )
                    yield Done(id,System.Random().NextDouble())
                }
            { m with Threads = ThreadPool.start worker m.Threads }

        | StartEvolving -> 
            let id = System.Guid.NewGuid().ToString()
            let worker =  
                proclist {
                    for i in 0 .. 100 do
                        do! Proc.Sleep m.GameModel.GameParametersConfig.Speed
                        yield Evolve
                    yield Done(id,System.Random().NextDouble())
                }
            { m with Threads = ThreadPool.start worker m.Threads }

        | KillWorkers -> 
            let threads = ThreadPool.empty
            { m with Threads = threads }

        // Old version
        // | Evolve -> 
        //     let newGrid = 
        //         m.Grid 
        //         |> HMap.toArray
        //         |> Array.map snd
        //         |> Evolve.asSequence m.GameModel.GameParametersState
        //         |> Array.map (fun c -> string c.id,c)
        //         |> HMap.ofArray
        //     { m with Grid = newGrid }

        | Evolve -> 
            let nextGen = EvolveOptimised.nextGeneration m.GameModel.GameParametersConfig m.GridBytes
            let newGrid = nextGen |> drawGrid m.GameModel.GameParametersConfig
            { m with Grid = newGrid; GridBytes = nextGen }

        | Reset -> 
            let newGrid = 
                initialiseGridBytes m.GameModel.GameParametersConfig
                |> drawGrid m.GameModel.GameParametersConfig
            { m with Grid = newGrid }

