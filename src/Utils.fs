module Utils

/// ```
/// hue = [0..360]
/// sat = [0..100]
/// brig = [0..100]
/// ```
/// [sources](https://www.devx.com/tips/Tip/41581)
let hsbToRgb (hue, sat, brig) =
    let h = float hue / 60.
    let s = float sat * 255. / 100.
    let b = float brig * 255. / 100.
    let maxRgb = b
    if s = 0. then 0uy, 0uy, 0uy
    else
        let delta = s * maxRgb / 255.
        if h > 3. then
            let blue = byte(round maxRgb)
            if h > 4. then
                let green = (round(maxRgb - delta))
                let red = byte(round ((h - 4.) * delta) + green)
                red, byte green, blue
            else
                let red = (round(maxRgb - delta))
                let green = byte(red - round((h - 4.) * delta))
                byte red, green, blue
        else
            if h > 1. then
                let green = byte(round maxRgb)
                if h > 2. then
                    let red = (round(maxRgb - delta))
                    let blue = byte(round((h - 2.) * delta) + red)
                    byte red, green, blue
                else
                    let blue = (round(maxRgb - delta))
                    let red = byte(blue - round((h - 2.) * delta))
                    red, green, byte blue
            else
                // if h > -1. then
                let red = byte(round maxRgb)
                if h > 0. then
                    let blue = (round(maxRgb - delta))
                    let green = byte(round(h * delta) + blue)
                    red, green, byte blue
                else
                    let green = (round(maxRgb - delta))
                    let blue = byte(green - round(h * delta))
                    red, byte green, blue
let test () =
    [
        hsbToRgb(210, 57, 24) = (26uy, 43uy, 61uy)
        hsbToRgb(29, 57, 24) = (61uy, 43uy, 26uy)
        hsbToRgb(13, 1, 100) = (255uy, 253uy, 252uy)
        hsbToRgb(70, 1, 100) = (254uy, 255uy, 252uy)
        hsbToRgb(80, 1, 100) = (254uy, 255uy, 252uy)
    ]
// List.init 36 (fun i ->
//     printf "%d = %A" i (hsbToRgb(i * 10, 1, 100))
// )