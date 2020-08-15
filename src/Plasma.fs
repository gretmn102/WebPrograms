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
