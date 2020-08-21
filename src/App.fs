module App

open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop // needed to call interop tools

type IMainLoop =
    abstract getFPS: unit -> float
    abstract getMaxAllowedFPS: unit -> int32
    abstract getSimulationTimestep: unit -> int32
    abstract isRunning: unit -> bool
    abstract resetFrameDelta: unit -> float
    /// `abstract setBegin(begin: (timestamp: number, delta: number) => void): MainLoop`
    abstract setBegin: ``begin``:(float -> int32 -> unit) -> IMainLoop
    /// `abstract setDraw(draw: (interpolationPercentage: number) => void): MainLoop`
    abstract setDraw: draw:(float -> unit) -> IMainLoop
    /// `abstract setUpdate(update: (delta: number) => void): MainLoop`
    abstract setUpdate: update:(float -> unit) -> IMainLoop
    /// `abstract setEnd(end: (fps: number, panic: boolean) => void): MainLoop`
    abstract setEnd: ``end``: (float -> bool -> unit) -> IMainLoop
    abstract setMaxAllowedFPS: ?fps: float -> IMainLoop
    abstract setSimulationTimestep: timestep:int32 -> IMainLoop
    abstract start: unit -> IMainLoop
    abstract stop: unit -> IMainLoop

[<ImportAll("mainloop.js")>]
let mainloop: IMainLoop = jsNative

let btnLeft = document.querySelector(".button-left") :?> Browser.Types.HTMLButtonElement
let btnRight = document.querySelector(".button-right") :?> Browser.Types.HTMLButtonElement
let myDisplay = document.querySelector(".my-display") :?> Browser.Types.HTMLElement
let btnRegen = document.querySelector(".button-regen") :?> Browser.Types.HTMLButtonElement
let canvas = document.querySelector(".my-canvas") :?> Browser.Types.HTMLCanvasElement
let fpsCounter = document.querySelector(".fps-counter") :?> Browser.Types.HTMLElement

let fps = document.getElementById("fps") :?> Browser.Types.HTMLInputElement
let fpsValue = document.getElementById("fpsvalue")
fps.addEventListener("input", fun _ ->
    fpsValue.textContent <- fps.value
)
fps.addEventListener("change", fun _ ->
    let value =
        match fps.value with
        | "60" -> System.Double.PositiveInfinity
        | v -> float v
    mainloop.setMaxAllowedFPS value
    |> ignore
)

let startPlasmaByCanvas () =
    let w, h = int canvas.width, int canvas.height
    let imgData = ImageData.Create(float w, float h)
    let canvasCtx = canvas.getContext_2d()
    let initialPlasma = Plasma.createPlasma w h
    let mutable plasma = initialPlasma
    let draw2 (plasma: _ [] []) =
        let imgDataBuff = imgData.data

        let mutable i = 0
        for y = 0 to h - 1 do
            for x = 0 to w - 1 do
                let hue = int(360. * plasma.[y].[x])
                let r, g, b = Utils.hsbToRgb(hue, 100, 100)
                imgDataBuff.[i] <- r
                imgDataBuff.[i + 1] <- g
                imgDataBuff.[i + 2] <- b
                imgDataBuff.[i + 3] <- 255uy // is alpha

                i <- i + 4
        canvasCtx.putImageData(imgData, 0., 0.)

    // btnRegen.onclick <- fun _ ->
    //     printfn "click"
    //     hueShift <- (hueShift + 0.02) % 1.
    //     let plasma' = Plasma.repaint hueShift plasma
    //     plasma <- plasma'
    //     draw2 plasma'

    let mutable hueShift = 0.

    mainloop.setUpdate (fun delta ->
        hueShift <- (hueShift + 0.02) % 5.
        let plasma' = Plasma.repaint hueShift initialPlasma
        plasma <- plasma'
        // Plasma.repaint' hueShift plasma
    ) |> ignore
    mainloop.setDraw (fun interp ->
        draw2 plasma
    ) |> ignore

    mainloop.setEnd (fun fps panic ->
        // System.Single.
        fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore
    mainloop.start () |> ignore
// startPlasmaByCanvas () |> ignore
do
    let duckSprite = document.getElementById("duck") :?> Browser.Types.HTMLImageElement
    let foxSprite = document.getElementById("fox") :?> Browser.Types.HTMLImageElement
    let x = FoxEscape.start duckSprite foxSprite canvas
    mainloop.setUpdate (fun _ -> x.Update ()) |> ignore
    mainloop.setDraw (fun _ -> x.Draw ()) |> ignore
    mainloop.setEnd (fun fps panic ->
        // System.Single.
        fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore
    mainloop.start () |> ignore

let startPlasmaByTable () =
    let w, h = 20, 20
    let grid =
        TableCanvas.createGrid w h myDisplay
    TableCanvas.fill grid

    let mutable plasma = Plasma.createPlasma w h
    let mutable hueShift = 0.
    let rec update () =
        window.requestAnimationFrame(fun i ->
            hueShift <- (hueShift + 0.02) % 1.
            let plasma' = Plasma.repaint hueShift plasma
            plasma <- plasma'
            TableCanvas.draw plasma' grid

            update ()
        )
        |> ignore
    update ()

// startPlasmaByTable ()

let game () =
    let worldLength = 15
    let world =
        Array.init worldLength (fun i ->
            // char(i % 10 + 48)
            string (i % 10)
        )
    let viewportRadius = 2
    let pan x = MapEngine.sandbox worldLength world viewportRadius 1. x worldLength

    let mutable idx = 0

    let render idx =
        let worldIdx, len, xs = pan idx
        // printfn "worldIdx = %A" worldIdx
        let idx' =
            if idx < viewportRadius + 1 then idx
            elif idx > worldLength - (viewportRadius + 1) then
                let d = viewportRadius + idx - (worldLength - viewportRadius - 1)
                d
            else viewportRadius
        xs.[idx'] <- "😱"

        let f xs =
            xs
            |> Array.map string
            |> String.concat "|" |> sprintf "|%s|"

        myDisplay.textContent <- f xs

    do
        render idx

    let res =
        {|
            Left = fun _ ->
                if idx > 0 then
                    idx <- idx - 1
                render idx
            Right = fun _ ->
                if idx < worldLength - 1 then
                    idx <- idx + 1
                render idx
        |}
    btnLeft.onclick <- res.Left
    btnRight.onclick <- res.Right
game ()