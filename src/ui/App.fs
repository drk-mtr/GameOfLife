namespace GameOfLife

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.Base.Rendering
open GameOfLife
open GameOfLife.Model
open Operators
open State

module App =

    let inputField fieldName onUpdate =
        div [ style @"display: flex; font-family: Fira Code; border-top; 10px; flex-direction: row; color: white; width: 100%; height: 2.6em; justify-content: space-between; align-items: center;" ] [
            div [ style "padding-left: 5px; padding-right: 10px; flex: 1; display: flex; align-items: center;" ] [ 
                text fieldName 
            ]
            div [ style "display: flex; align-items: center;" ] [ 
                input [ (onChange onUpdate); style "width: 80px; height: 2.4em; padding-left: 5px; font-family: Fira Code; background-color: #333; border: 0; color: #ffaa21; border-radius: 2px;" ] 
            ]
        ]

    let view (m : MModel) =

        let frustum = 
            Frustum.perspective 60.0 0.1 600.0 1.0 
            |> Mod.constant

        let boxes = 
            aset {
                for (name: string),(cell: MCell) in m.Grid |> AMap.toASet do
                    let color = 
                        adaptive {
                            match! cell.IsAlive with
                            | true -> return C4b.Red
                            | false -> return C4b.White
                        }

                    let standardBox = Mod.constant (Box3d(-V3d.III, V3d.III))

                    let translateV3d = 
                        adaptive {
                            let! x = cell.X
                            let! y = cell.Y
                            let! z = cell.Z
                            return V3d ((2.5 * float x), (2.5 * float y), (float z))
                        }

                    yield
                        Sg.box color standardBox
                        |> Sg.translate' translateV3d 
                        |> Sg.noEvents
            } |> Sg.set

        let sg =
            m.CurrentModel |> Mod.map (fun v ->
                match v with
                | Box -> 
                    Sg.box (Mod.constant C4b.Red) (Mod.constant (Box3d(-V3d.III, V3d.III)))
                | BoxArray -> 
                    boxes
                | Sphere -> 
                    Sg.sphere 5 (Mod.constant C4b.Green) (Mod.constant 1.0)
            )
            |> Sg.dynamic
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.simpleLighting
            }

        let att = [
            style "position: fixed; left: 0; top: 0; width: 100%; height: 100%"
        ]

        let buttonDefault label action = 
            div [ style "padding-top: 5px; flex: 1;" ] [
                button [ action; style "width: 100%;" ] [ text label ]
            ]

        body [] [
            FreeFlyController.controlledControl m.CameraState CameraMessage frustum (AttributeMap.ofList att) sg

            div [style "position: fixed; left: 20px; top: 20px; width: 220px; display: flex; flex-direction: column;"] [

                buttonDefault "Toggle Model" (onClick (fun _ -> ToggleModel))
                buttonDefault "To Box Array" (onClick (fun _ -> ToBoxArray))
                buttonDefault "Start Timer" (onClick (fun _ -> StartTimer))

                div [style "flex: 1;"] [
                    let v2i = V2i(5,5)
                    let v3d = V3d(5,5,5)
                    let camView = CameraView(v3d,v3d,v3d,v3d,v3d)
                    buttonDefault 
                        "Camera Msg Test" 
                        (onClick (fun _ -> CameraMessage (FreeFlyController.JumpTo (camView)) ))
                ]

                inputField "Grid rows" (
                    fun txt -> 
                        let gameParams = m.GameModel.GameParametersConfig.Current |> Mod.force
                        UpdateGameParameters { gameParams with Rows = int txt }
                )

                inputField "Grid columns" (
                    fun txt -> 
                        let gameParams = m.GameModel.GameParametersConfig.Current |> Mod.force
                        UpdateGameParameters { gameParams with Columns = int txt }
                )

                inputField "Speed" (
                    fun txt -> 
                        let gameParams = m.GameModel.GameParametersConfig.Current |> Mod.force
                        UpdateGameParameters { gameParams with Speed = int txt }
                )

                inputField "Probability" (
                    fun txt -> 
                        let gameParams = m.GameModel.GameParametersConfig.Current |> Mod.force
                        UpdateGameParameters { gameParams with Probability = float txt }
                )
                
                buttonDefault "Apply game parameters" (onClick (fun _ -> ApplyGameParameters))

                buttonDefault "Kill workers" (onClick (fun _ -> KillWorkers))

                buttonDefault "Evolve" (onClick (fun _ -> Evolve))

                buttonDefault "Start evolving" (onClick (fun _ -> StartEvolving))

            ]

        ]

    let threads (model: Model) =
        model.Threads

    let app = {
        initial = initial
        update = update
        view = view
        threads = 
            fun mdl ->
                let camMsg = Model.Lens.CameraState.Get mdl |> FreeFlyController.threads |> ThreadPool.map CameraMessage
                let appMsg = threads mdl
                ThreadPool.union camMsg appMsg // TODO: Not sure this is right...
        unpersist = Unpersist.instance
    }
