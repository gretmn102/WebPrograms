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

let calcSpiral alpha t =
    let t1 = math.cos(t)
    let t2 = math.sin(t)
    let t5 = basicCircleR * (t * t2 + t1)
    let t6 = math.cos(alpha)
    let t10 = basicCircleR * (-t * t1 + t2)
    let t11 = math.sin(alpha)
    t5 * t6 - t10 * t11, t10 * t6 + t11 * t5

let drawItem (item: Item) =
    item.Element.setAttribute("style",
        sprintf "position: absolute; width: %dpx; height: %dpx; left: %fpx; top: %fpx"
            itemSize
            itemSize
            (centerX - itemR - item.X)
            (centerX - itemR - item.Y)
    )

let tMax = sqrt(float maxCircleR**2.0 - basicCircleR**2.0) / basicCircleR

let initItem isRevert i (item: Browser.Types.Element) =
    let alpha = (2.0 * math.PI) * float i / float count
    let x, y =
        if isRevert then
            calcSpiral alpha tMax
        else
            calcSpiral alpha 0.0
    let item =
        {
            X = x
            Y = y
            Alpha = alpha
            Element = item
        }
    items.[i] <- item

    drawItem item

let start isRevert revert =
    let mutable t =
        if isRevert then tMax else 0.0

    mainloop.setUpdate (fun delta ->
        t <-
            if isRevert then
                (t - (delta / 150.))
            else
                (t + (delta / 150.))

        items
        |> Array.iteri (fun i item ->
            let x', y' = calcSpiral item.Alpha t
            items.[i] <- { item with X = x'; Y = y' }
        )

        if not (0.0 < t && t < tMax) then
            mainloop.stop() |> ignore

            revert ()

    ) |> ignore

    mainloop.setDraw (fun interp ->
        items |> Array.iter drawItem
    ) |> ignore

    mainloop.setEnd (fun fps panic ->
        // TODO: fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore

    mainloop.start () |> ignore

type State =
    {
        IsRevert: bool
    }

type Msg =
    | Revert

open Elmish

let update (msg: Msg) (state: State) =
    match msg with
    | Revert ->
        let state =
            { state with IsRevert = not state.IsRevert }
        state, Cmd.none

let init (): State * Cmd<Msg> =
    let state = {
        IsRevert = false
    }

    state, Cmd.none


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
                        match item with
                        | null -> ()
                        | item ->
                            initItem state.IsRevert i item
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

                    Left (centerX - basicCircleR)
                    Top (centerY - basicCircleR)
                ]
                OnClick (fun _ -> start state.IsRevert (fun () -> dispatch Revert))
            ] [
                str "+"
            ]
        ]
    ]
