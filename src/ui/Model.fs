namespace GameOfLife.Model

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

[<DomainType>]
type GameParams = {
    Rows: int
    Columns: int
    Probability: float
    Speed: int
}

type Primitive =
    | BoxArray
    | Box
    | Sphere

type Message =
    | ToggleModel
    | ToBoxArray
    | CameraMessage of FreeFlyController.Message
    | StartTimer
    | UpdateClock of string
    | UpdateGameParameters of GameParams
    | ApplyGameParameters
    | Reset
    | KillWorkers
    | Evolve
    | StartEvolving
    | Done of string * float

[<DomainType>]
type Cell = {
    [<PrimaryKey>]
    id: int
    X: int
    Y: int
    Z: int
    Coord: V3d
    IsAlive: bool
}

[<DomainType>]
type GameModel = {
    GameParametersState: GameParams
    GameParametersConfig: GameParams
}

[<DomainType>]
type Model =
    {
        Grid: hmap<string,Cell>
        GridBytes: byte array
        GameModel: GameModel
        CurrentModel: Primitive
        CameraState: CameraControllerState
        Threads: ThreadPool<Message>
        Timer: string
    }
