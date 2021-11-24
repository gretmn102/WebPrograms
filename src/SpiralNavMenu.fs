module SpiralNavMenu
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Feliz

open Mainloop

let maxCircleR = 60
let basicCircleLength = 35
let basicCircleR = float basicCircleLength / 2.
let itemSize = 30
let itemR = float itemSize / 2.

let canvasWidth = 2 * (maxCircleR + int itemR)
let canvasHeight = 2 * (maxCircleR + int itemR)
let centerX = float canvasWidth / 2.
let centerY = float canvasHeight / 2.

let count = 6

type Item =
    {
        X: float
        Y: float
        Alpha: float
        Element: Browser.Types.Element
    }

let mutable items: Item [] =
    Array.init count (fun _ ->
        { X = 0.0; Y = 0.0; Alpha = 0.0; Element = null }
    )

let math = Fable.Core.JS.Math

let initItem i (item: Browser.Types.Element) =
    items.[i] <- {
        X = 0.0
        Y = 0.0
        Alpha = (2.0 * math.PI) * float i / float count
        Element = item
    }

let start () =
    let mutable t = 0.0

    let tMax = sqrt(float maxCircleR**2.0 - basicCircleR**2.0) / basicCircleR

    let calcSpiral alpha t =
        let t1 = math.cos(t)
        let t2 = math.sin(t)
        let t5 = basicCircleR * (t * t2 + t1)
        let t6 = math.cos(alpha)
        let t10 = basicCircleR * (-t * t1 + t2)
        let t11 = math.sin(alpha)
        t5 * t6 - t10 * t11, t10 * t6 + t11 * t5

    mainloop.setUpdate (fun delta ->
        t <- (t + (delta / 150.)) // % tMax

        items
        |> Array.iteri (fun i item ->
            let x', y' = calcSpiral item.Alpha t
            items.[i] <- { item with X = x'; Y = y' }
        )

        if t > tMax then
            mainloop.stop() |> ignore

    ) |> ignore

    mainloop.setDraw (fun interp ->
        items
        |> Array.iter (fun item ->
            item.Element.setAttribute("style",
                sprintf "position: absolute; width: %dpx; height: %dpx; left: %fpx; top: %fpx"
                    itemSize
                    itemSize
                    (centerX - itemR - item.X)
                    (centerX - itemR - item.Y)
            )
        )
    ) |> ignore

    mainloop.setEnd (fun fps panic ->
        // TODO: fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore

    mainloop.start () |> ignore

type State = unit

type Msg = unit

open Elmish

let update (msg: Msg) (state: State) =
    state, Cmd.none

let init (): State * Cmd<Msg> =
    (), Cmd.none


let containerBox (state: State) (dispatch: Msg -> unit) =
    Box.box' [] [
        div [
            Style [
                Width canvasWidth
                Height canvasHeight
                // BackgroundColor "grey"
                Position PositionOptions.Relative
            ]
        ] [
            for i in [0..count - 1] do
                button [
                    Ref (fun item ->
                        initItem i item
                    )
                    Style [
                        Position PositionOptions.Absolute
                        Width itemSize
                        Height itemSize
                        Left (centerX - itemR)
                        Top (centerY - itemR)
                        BorderRadius "50%"
                    ]
                ] [
                    str (string i)
                ]
            button [
                Style [
                    Position PositionOptions.Absolute
                    Width basicCircleLength
                    Height basicCircleLength

                    do printfn "%f" centerX
                    do printfn "basicCircleR = %f" basicCircleR
                    Left (centerX - basicCircleR)
                    Top (centerY - basicCircleR)
                ]
                OnClick (fun _ -> start ())
            ] [
                str "+"
            ]
        ]
    ]
