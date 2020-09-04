module MapEngine

let define worldLength viewportRadius pos =
    let viewportLength = viewportRadius * 2 + 1
    if worldLength < viewportLength then
        // мир нечетный
        // | | | | |   ->
        // |0|1|2| |
        // | | | | | | ->
        // | |0|1|2| |
        // мир четный
        // | | | | | ->
        // | |0|1| |
        // | | | | | | ->
        // | |0|1| | |
        let k = worldLength % 2
        let l1 = worldLength / 2 + k
        (0, viewportRadius - l1 + k, worldLength), l1 - 1
    elif worldLength = viewportLength then
        // |0|1|2|3|4| -> 2
        // |0|1|2|3| -> 1
        (0, 0, worldLength), worldLength / 2 + worldLength % 2 - 1
    else
        // |1|2|3|4|
        // |1|2|3| // pos = 0, 1
        // |2|3|4| // pos = 2, 3

        // |1|2|3|4|5|
        // |1|2|3| // pos = 0, 1 | worldIdx = 0
        // |2|3|4| // pos = 2    | worldIdx = 1
        // |3|4|5| // pos = 3, 4 | worldIdx = 2

        // |1|2|3|4|5|6|7| // worldLength = 7
        // |1|2|3|4|5| // pos = 0, 1, 2   | worldIdx = 0
        // |2|3|4|5|6| // pos = 3         | worldIdx = 1
        // |3|4|5|6|7| // pos = 4, 5, ... | worldIdx = 2
        let worldIdx =
            // let viewportRadius = 1
            // let worldLength = 6
            // [0..7] |> List.map (fun pos ->
            if pos < viewportRadius + 1 then 0
            elif pos > worldLength - (viewportRadius + 1) then
                worldLength - viewportLength // (viewportRadius * 2 + 1)
            else pos - viewportRadius
                // )
        (worldIdx, 0, viewportLength), 0

let toScale srcLength dstLength = (*) (float (dstLength - 1) / float (srcLength - 1))
let ofScale srcLength dstLength = (*) (float (srcLength - 1) / float (dstLength - 1))
let scale scaleFactor srcLength src =
    let dstLength =
        let f x y = int <| round (x * float y)
        f scaleFactor srcLength
    let ofScale = ofScale srcLength dstLength
    Array.init dstLength (fun i ->
        let f fn = float >> fn >> round >> int
        Array.get src (f ofScale i)
    )


let sandbox worldLength world viewportRadius  =
    let display viewportRadius world (worldIdx, viewportIdx, count) =
        let viewportLength = viewportRadius * 2 + 1
        let viewport = Array.zeroCreate viewportLength
        Array.blit world worldIdx viewport viewportIdx count
        viewport
    // let worldLength = 15
    // let viewportRadius = 3

    // let world = [|1..worldLength|]
    let round x = round x |> int
    let ofScale' worldLength' i' =
        round <| ofScale worldLength worldLength' (float i')
    let toScale' worldLength' i =
        round <| toScale worldLength worldLength' (float i)

    let f scaleFactor worldIdx' worldLength' =
        let worldLength'' = float worldLength * scaleFactor |> round |> int
        let pos =
            // worldIdx' + viewportRadius + 1
            worldIdx'
            |> float
            |> ofScale worldLength worldLength'
            |> toScale worldLength worldLength''
            |> round
        // pos
        let world'' = scale scaleFactor worldLength world
        let (worldIdx'', viewportIdx, count), _ =
            define worldLength'' viewportRadius pos
        worldIdx'' + viewportIdx, worldLength'', display viewportRadius world'' (worldIdx'', viewportIdx, count)

    // let (worldIdx', viewportIdx, count), _ = define worldLength viewportRadius 2
    // let pos = worldIdx' + viewportRadius + 1
    // let i, worldLength', xs = f 0.3 3 worldLength
    // let i, worldLength', xs = f 1.  (i + viewportRadius) worldLength
    // let i, worldLength', xs = f 2.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 3.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 4.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 3.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 2.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 1.  (i + viewportRadius) worldLength'
    // let i, worldLength', xs = f 0.3 (i + viewportRadius) worldLength'
    f
    // ()
    // f 3. 2 worldLength
