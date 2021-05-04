module Lissajous

open Elmish
open Feliz
open Browser

open Mainloop
type State =
    {
        Acceleration: float
        Resolution: float
        Cycles: int
        Freq: float
    }

type Msg =
    | SetFreq of float
    | SetCycles of int
    | SetAcceleration of float
    | SetResolution of float

let initState =
    {
        Acceleration = 0.0003
        Resolution = 0.007
        Cycles = 10
        Freq =
            // let r = System.Random()
            // 3.0 * r.NextDouble()
            1.02
    }

let init () =
    initState, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetFreq freq ->
        let state =
            { state with
                Freq = freq
            }
        state, Cmd.none
    | SetCycles x ->
        let state =
            { state with
                Cycles = x
            }
        state, Cmd.none
    | SetAcceleration x ->
        let state =
            { state with
                Acceleration = x
            }
        state, Cmd.none
    | SetResolution x ->
        let state =
            { state with
                Resolution = x
            }
        state, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Feliz

let mutable m_phase = 0.
let mutable m_lastPhase = 0.
let mutable m_theta = 0.

let containerBox (state : State) (dispatch : Msg -> unit) =
    Box.box' [] [
        Columns.columns [] [
            Column.column [
            ] [
                let canvas =
                    Html.canvas [
                        prop.style [
                            // Feliz.style.border(1, borderStyle.solid, "gray")
                        ]

                        prop.tabIndex -1
                        prop.ref (fun canvas ->
                            if isNull canvas then ()
                            else
                                let canvas = canvas :?> Types.HTMLCanvasElement

                                let mutable m_size = 200
                                let mutable m_width, m_height = m_size * 2 + 1, m_size * 2 + 1

                                let mutable m_field = Array.init m_height (fun _ -> Array.zeroCreate m_width)

                                let mutable m_imgData = ImageData.Create(float m_width, float m_height)

                                let updateSize () =
                                    match canvas.parentElement with
                                    | null -> ()
                                    | x ->
                                        let w = x.offsetWidth - 50.

                                        m_size <- int ((w - 1.0) / 2.0)
                                        let w = m_size * 2 + 1
                                        m_width <- w
                                        m_height <- w
                                        m_field <- Array.init m_height (fun _ -> Array.zeroCreate m_width)
                                        m_imgData <- ImageData.Create(float m_width, float m_height)

                                        canvas.width <- float w
                                        canvas.height <- float w
                                updateSize ()

                                window.onresize <- fun x ->
                                    updateSize ()

                                let create phase =
                                    for i = 0 to m_height - 1 do
                                        let xs = m_field.[i]
                                        for j = 0 to m_width - 1 do
                                            xs.[j] <- false

                                    let sizef = float m_size
                                    for t in 0.0..state.Resolution..float state.Cycles * 2.0 * System.Math.PI do
                                        let x, y = sin t, cos (t*state.Freq + phase)

                                        let x, y = m_size+int(x * sizef + 0.5), m_size+int(y * sizef + 0.5)
                                        m_field.[y].[x] <- true
                                    m_field


                                let canvasCtx = canvas.getContext_2d()
                                let draw (xss: _ [] []) =
                                    let imgDataBuff = m_imgData.data
                                    let mutable i = 0
                                    for y = 0 to m_height - 1 do
                                        for x = 0 to m_width - 1 do
                                            if xss.[y].[x] then
                                                imgDataBuff.[i] <- 0uy
                                                imgDataBuff.[i + 1] <- 255uy
                                                imgDataBuff.[i + 2] <- 0uy
                                                imgDataBuff.[i + 3] <- 255uy // is alpha
                                            else
                                                imgDataBuff.[i] <- 0uy
                                                imgDataBuff.[i + 1] <- 0uy
                                                imgDataBuff.[i + 2] <- 0uy
                                                imgDataBuff.[i + 3] <- 255uy // is alpha
                                            i <- i + 4
                                    canvasCtx.putImageData(m_imgData, 0., 0.)

                                mainloop.setUpdate (fun delta ->
                                    m_lastPhase <- m_phase
                                    m_theta <- m_theta + delta * state.Acceleration
                                    m_phase <- m_theta
                                ) |> ignore
                                mainloop.setDraw (fun interp ->
                                    create (m_lastPhase + (m_phase - m_lastPhase) * interp) |> draw
                                ) |> ignore

                                mainloop.setEnd (fun fps panic ->
                                    // TODO: fpsCounter.textContent <- sprintf "%A FPS" (round fps)
                                    if panic then
                                        let discardedTime = round(mainloop.resetFrameDelta())
                                        printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
                                ) |> ignore
                                mainloop.start () |> ignore
                        )
                    ]
                Html.div [
                    prop.style [
                            style.justifyContent.center
                            style.display.flex
                    ]
                    prop.children canvas
                ]
            ]
            Column.column [
            ] [
                let create description input =
                    Panel.panel [
                    ] [
                        Label.label [] [ str description ]
                        Control.div [] [
                            Field.div [ Field.HasAddons ] [
                                Control.p [
                                ] [
                                    input description
                                ]
                            ]
                        ]
                    ]

                create "Freq" (fun description ->
                    Input.number [
                        Input.Placeholder description
                        Input.Value (string state.Freq)
                        Input.OnChange (fun e ->
                            match e with
                            | null -> ()
                            | e ->
                                match System.Double.TryParse e.Value with
                                | true, v ->
                                    v
                                    |> SetFreq
                                    |> dispatch
                                | _ -> ()
                        )
                        Input.Option.Props [
                            Step "0.01"
                            Min "0"
                        ]
                    ]
                )

                create "Acceleration" (fun description ->
                    Input.number [
                        Input.Placeholder description
                        Input.Value (string state.Acceleration)
                        Input.OnChange (fun e ->
                            match e with
                            | null -> ()
                            | e ->
                                match System.Double.TryParse e.Value with
                                | true, v ->
                                    v
                                    |> SetAcceleration
                                    |> dispatch
                                | _ -> ()
                        )
                        Input.Option.Props [
                            Step "0.0001"
                            Min "0.0001"
                        ]
                    ]
                )

                create "Cycles" (fun description ->
                    Input.number [
                        Input.Placeholder description
                        Input.Value (string state.Cycles)
                        Input.OnChange (fun e ->
                            match e with
                            | null -> ()
                            | e ->
                                match System.Int32.TryParse e.Value with
                                | true, v ->
                                    v
                                    |> SetCycles
                                    |> dispatch
                                | _ -> ()
                        )
                        Input.Option.Props [
                            Step "1"
                            Min "1"
                        ]
                    ]
                )

                create "Resolution" (fun description ->
                    Input.number [
                        Input.Placeholder description
                        Input.Value (string state.Resolution)
                        Input.OnChange (fun e ->
                            match e with
                            | null -> ()
                            | e ->
                                match System.Double.TryParse e.Value with
                                | true, v ->
                                    v
                                    |> SetResolution
                                    |> dispatch
                                | _ -> ()
                        )
                        Input.Option.Props [
                            Step "0.001"
                            Min "0.001"
                        ]
                    ]
                )
            ]
        ]
    ]
