module Lissajous
open Browser.Dom
open Fable.Core
open Browser.Types

let lissajousStart (document:Document) (gameDiv:HTMLDivElement) (mainloop:Mainloop.IMainLoop) (fpsCounter:HTMLElement) =
    let size = 200
    let w, h = size * 2 + 1, size * 2 + 1

    let canvas = document.createElement "canvas" :?> Browser.Types.HTMLCanvasElement
    canvas.width <- float w
    canvas.height <- float h
    gameDiv.appendChild canvas |> ignore
    // let disposes = [canvas :> Node]

    let table = document.createElement "table" :?> Browser.Types.HTMLTableElement
    // let disposes = table :> Node :: disposes
    gameDiv.appendChild table |> ignore

    let mutable res = 0.007
    do
        let row = table.insertRow ()
        let cell = row.insertCell ()
        document.createTextNode "Resolution" |> cell.appendChild |> ignore

        let resolutionNode = document.createElement "input" :?> Browser.Types.HTMLInputElement
        resolutionNode.``type`` <- "number"
        resolutionNode.step <- "0.001"
        resolutionNode.value <- string res

        resolutionNode.oninput <- fun x ->
            if resolutionNode.valueAsNumber > 0. then
                res <- resolutionNode.valueAsNumber
        let cell = row.insertCell()
        resolutionNode |> cell.appendChild |> ignore

    let mutable cycles = 10.0
    do
        let row = table.insertRow ()
        let cell = row.insertCell ()
        document.createTextNode "Cycles"
        |> cell.appendChild
        |> ignore

        let circlesCountNode = document.createElement "input" :?> Browser.Types.HTMLInputElement
        circlesCountNode.``type`` <- "number"
        circlesCountNode.step <- "1"
        circlesCountNode.value <- string cycles

        circlesCountNode.oninput <- fun x ->
            if circlesCountNode.valueAsNumber > 0. then
                cycles <- circlesCountNode.valueAsNumber
        gameDiv.appendChild circlesCountNode |> ignore
        let cell = row.insertCell()
        circlesCountNode |> cell.appendChild |> ignore

    let r = System.Random()
    let mutable freq = 3.0 * r.NextDouble()
    let row = table.insertRow ()
    do
        let cell = row.insertCell ()
        document.createTextNode "Freq"
        |> cell.appendChild
        |> ignore

        let freqNode = document.createElement "input" :?> Browser.Types.HTMLInputElement
        freqNode.``type`` <- "number"
        freqNode.step <- "0.01"
        freqNode.value <- string freq

        freqNode.oninput <- fun x ->
            if freqNode.valueAsNumber > 0. then
                freq <- freqNode.valueAsNumber
        let cell = row.insertCell()
        freqNode |> cell.appendChild |> ignore


    let mutable acceleration = 0.0003
    do
        let row = table.insertRow ()
        let cell = row.insertCell ()
        document.createTextNode "Acceleration"
        |> cell.appendChild
        |> ignore

        let accelerationNode = document.createElement "input" :?> Browser.Types.HTMLInputElement
        accelerationNode.``type`` <- "number"
        accelerationNode.step <- "0.0001"
        accelerationNode.value <- string acceleration

        accelerationNode.oninput <- fun x ->
            if accelerationNode.valueAsNumber > 0. then
                acceleration <- accelerationNode.valueAsNumber
        let cell = row.insertCell()
        accelerationNode |> cell.appendChild |> ignore

    let xs = Array.init h (fun _ -> Array.zeroCreate w)

    let create phase =
        for i = 0 to h - 1 do
            let xs = xs.[i]
            for j = 0 to w - 1 do
                xs.[j] <- false

        let sizef = float size
        for t in 0.0..res..cycles * 2.0 * System.Math.PI do
            let x, y = sin t, cos (t*freq + phase)

            let x, y = size+int(x * sizef + 0.5), size+int(y * sizef + 0.5)
            xs.[y].[x] <- true
        xs

    let imgData = ImageData.Create(float w, float h)
    let canvasCtx = canvas.getContext_2d()
    let draw (xss: _ [] []) =
        let imgDataBuff = imgData.data
        let mutable i = 0
        for y = 0 to h - 1 do
            for x = 0 to w - 1 do
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
        canvasCtx.putImageData(imgData, 0., 0.)

    let mutable phase = 0.
    let mutable lastPhase = 0.
    let mutable theta = 0.

    mainloop.setUpdate (fun delta ->
        // phase <- phase + (delta * speed)
        lastPhase <- phase
        theta <- theta + delta * acceleration
        phase <- theta
    ) |> ignore
    mainloop.setDraw (fun interp ->
        create (lastPhase + (phase - lastPhase) * interp) |> draw
        // create phase |> draw
    ) |> ignore

    mainloop.setEnd (fun fps panic ->
        fpsCounter.textContent <- sprintf "%A FPS" (round fps)
        if panic then
            let discardedTime = round(mainloop.resetFrameDelta())
            printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
    ) |> ignore
    mainloop.start () |> ignore
