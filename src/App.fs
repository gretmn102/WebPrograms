module App

open Browser.Dom
open Fable.Core
open Fable.Core.JsInterop // needed to call interop tools


let fpsCounter = document.querySelector(".fps-counter") :?> Browser.Types.HTMLElement
let gameDiv = document.getElementById("game") :?> Browser.Types.HTMLDivElement
let dispose () =
    while gameDiv.hasChildNodes () do
        gameDiv.removeChild gameDiv.firstChild |> ignore

let fps = document.getElementById("fps") :?> Browser.Types.HTMLInputElement
let fpsValue = document.getElementById("fpsvalue")
fps.addEventListener("input", fun _ ->
    fpsValue.textContent <- fps.value
)
let mainloop = Mainloop.mainloop

fps.addEventListener("change", fun _ ->
    let value =
        match fps.value with
        | "60" -> System.Double.PositiveInfinity
        | v -> float v
    mainloop.setMaxAllowedFPS value
    |> ignore
)

type T =
    | NoOne
    | Lissajous
    | Duckhunting
    | Roguelike
    | PlasmaByCanvas
    | PlasmaByTable
let mutable currentNode : Browser.Types.HTMLElement = null
let mutable current = NoOne

let startPlasmaByCanvas () =
    let width, height = 100, 200
    let canvas = document.createElement "canvas" :?> Browser.Types.HTMLCanvasElement
    canvas.width <- float width
    canvas.height <- float height
    gameDiv.appendChild canvas |> ignore

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
        fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore
    mainloop.start () |> ignore

document.getElementById("plasmabycanvas") :?> Browser.Types.HTMLHRElement
|> fun node ->
    node.onclick <- fun _ ->
        if current <> PlasmaByCanvas then
            mainloop.stop() |> ignore
            dispose ()

            startPlasmaByCanvas ()
            if not <| isNull currentNode then
                currentNode.removeAttribute "class"
            node.setAttribute("class", "active")
            currentNode <- node
            current <- PlasmaByCanvas

document.getElementById("lissajous") :?> Browser.Types.HTMLHRElement
|> fun node ->
    node.onclick <- fun _ ->
        if current <> Lissajous then
            mainloop.stop() |> ignore
            dispose ()

            Lissajous.lissajousStart document gameDiv mainloop fpsCounter
            |> ignore

            if not <| isNull currentNode then
                currentNode.removeAttribute "class"
            node.setAttribute("class", "active")
            currentNode <- node
            current <- Lissajous

let startPlasmaByTable () =
    let w, h = 10, 10
    let grid =
        TableCanvas.createGrid w h gameDiv
    TableCanvas.fill grid

    let mutable plasma = Plasma.createPlasma w h
    let mutable hueShift = 0.

    mainloop.setUpdate (fun _ ->
        hueShift <- (hueShift + 0.02) % 5.
        // let plasma' = Plasma.repaint hueShift plasma
        Plasma.repaint' hueShift plasma
        // plasma <- plasma'
    ) |> ignore
    mainloop.setDraw (fun _ -> TableCanvas.draw plasma grid) |> ignore
    mainloop.setEnd (fun fps panic ->
        fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore
    mainloop.start () |> ignore

document.getElementById("plasmabytable") :?> Browser.Types.HTMLHRElement
|> fun node ->
    node.onclick <- fun _ ->
        if current <> PlasmaByTable then
            mainloop.stop() |> ignore
            dispose ()

            startPlasmaByTable()

            if not <| isNull currentNode then
                currentNode.removeAttribute "class"
            node.setAttribute("class", "active")
            currentNode <- node
            current <- PlasmaByTable

let game () =
    let myDisplay = document.createElement "p" :?> Browser.Types.HTMLParagraphElement
    myDisplay.setAttribute("style", "font-family: Consolas,monaco,monospace;")
    gameDiv.appendChild myDisplay |> ignore

    let table = document.createElement "table" :?> Browser.Types.HTMLTableElement
    gameDiv.appendChild table |> ignore

    let row = table.insertRow()

    let cell = row.insertCell()
    let btnLeft = document.createElement "button" :?> Browser.Types.HTMLButtonElement
    btnLeft.innerText <- "⬅️"
    cell.appendChild btnLeft |> ignore

    let cell = row.insertCell()
    let btnRight = document.createElement "button" :?> Browser.Types.HTMLButtonElement
    btnRight.innerText <- "➡️"
    cell.appendChild btnRight |> ignore

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

document.getElementById("roguelike") :?> Browser.Types.HTMLHRElement
|> fun node ->
    node.onclick <- fun _ ->
        if current <> Roguelike then
            mainloop.stop() |> ignore
            dispose ()

            game ()

            if not <| isNull currentNode then
                currentNode.removeAttribute "class"
            node.setAttribute("class", "active")
            currentNode <- node
            current <- Roguelike
