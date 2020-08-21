module Plasma
// https://rosettacode.org/wiki/Plasma_effect#Java
let createPlasma w h =
    Array.init h (fun y ->
        Array.init w (fun x ->
            let x, y = float x, float y
            sin(x / 16.0)
            + sin(y / 8.0)
            + sin((x + y) / 16.0)
            + sin(sqrt(x * x + y * y) / 8.0)
            + 4.
            / 8.
        )
    )

let repaint hueShift (plasma:float [] []) =
    plasma
    |> Array.mapi (fun y ->
        Array.mapi (fun x e ->
            let hue = hueShift + e % 1.
            // img.setRGB(x, y, Color.HSBtoRGB(hue, 1, 1));
            hue
        )
    )
let repaint' hueShift (plasma:float [] []) =
    let w = plasma.[0].Length
    for i = 0 to plasma.Length - 1 do
        for j = 0 to w - 1 do
            let e = plasma.[i].[j]
            let hue = hueShift + e % 1.
            plasma.[i].[j] <- hue
