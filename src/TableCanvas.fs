module TableCanvas
open Browser.Dom


let createGrid w h (node:Browser.Types.HTMLElement) =
    printfn "grid creating..."
    let table = document.createElement("table")
    node.appendChild table |> ignore
    
    Array.init h (fun y ->
        let tr = document.createElement("tr")
        table.appendChild tr |> ignore
        Array.init w (fun x ->
            let td = document.createElement("td")
            // td.textContent <- sprintf "%A" (y, x)
            tr.appendChild td |> ignore
            td
        )
    )

let draw (plasma: _ [] []) grid =
    grid
    |> Array.iteri (fun y ->
        Array.iteri (fun x (node:Browser.Types.HTMLElement) ->
            // 360. * 0.01
            let hue = int(360. * plasma.[y].[x])
            // printf "hue = %A" plasma.[y].[x]
            let r, g, b = Utils.hsbToRgb(hue, 100, 100)
            // font color="#00AABB"
            node.setAttribute ("style", sprintf "color: rgb(%d, %d, %d);" r g b)
        )
    )

let fill grid =
    grid
    |> Array.iteri (fun y ->
        Array.iteri (fun x (node:Browser.Types.HTMLElement) ->
            // node.textContent <- sprintf "%A" (x + 1, y + 1)
            node.textContent <- sprintf "x"
        )
    )

// let grid = createGrid w h
