module Index


// open Browser.Dom
// open Fable.Core
// open Fable.Core.JsInterop // needed to call interop tools


// let fpsCounter = document.querySelector(".fps-counter") :?> Browser.Types.HTMLElement
// let gameDiv = document.getElementById("game") :?> Browser.Types.HTMLDivElement
// let dispose () =
//     while gameDiv.hasChildNodes () do
//         gameDiv.removeChild gameDiv.firstChild |> ignore

// let fps = document.getElementById("fps") :?> Browser.Types.HTMLInputElement
// let fpsValue = document.getElementById("fpsvalue")
// fps.addEventListener("input", fun _ ->
//     fpsValue.textContent <- fps.value
// )
// let mainloop = Mainloop.mainloop

// fps.addEventListener("change", fun _ ->
//     let value =
//         match fps.value with
//         | "60" -> System.Double.PositiveInfinity
//         | v -> float v
//     mainloop.setMaxAllowedFPS value
//     |> ignore
// )

// type T =
//     | NoOne
//     | Lissajous
//     | Duckhunting
//     | Roguelike
//     | PlasmaByCanvas
//     | PlasmaByTable
// let mutable currentNode : Browser.Types.HTMLElement = null
// let mutable current = NoOne

// let startPlasmaByCanvas () =
//     let width, height = 100, 200
//     let canvas = document.createElement "canvas" :?> Browser.Types.HTMLCanvasElement
//     canvas.width <- float width
//     canvas.height <- float height
//     gameDiv.appendChild canvas |> ignore

//     let w, h = int canvas.width, int canvas.height
//     let imgData = ImageData.Create(float w, float h)
//     let canvasCtx = canvas.getContext_2d()
//     let initialPlasma = Plasma.createPlasma w h
//     let mutable plasma = initialPlasma
//     let draw2 (plasma: _ [] []) =
//         let imgDataBuff = imgData.data

//         let mutable i = 0
//         for y = 0 to h - 1 do
//             for x = 0 to w - 1 do
//                 let hue = int(360. * plasma.[y].[x])
//                 let r, g, b = Utils.hsbToRgb(hue, 100, 100)
//                 imgDataBuff.[i] <- r
//                 imgDataBuff.[i + 1] <- g
//                 imgDataBuff.[i + 2] <- b
//                 imgDataBuff.[i + 3] <- 255uy // is alpha

//                 i <- i + 4
//         canvasCtx.putImageData(imgData, 0., 0.)

//     // btnRegen.onclick <- fun _ ->
//     //     printfn "click"
//     //     hueShift <- (hueShift + 0.02) % 1.
//     //     let plasma' = Plasma.repaint hueShift plasma
//     //     plasma <- plasma'
//     //     draw2 plasma'

//     let mutable hueShift = 0.

//     mainloop.setUpdate (fun delta ->
//         hueShift <- (hueShift + 0.02) % 5.
//         let plasma' = Plasma.repaint hueShift initialPlasma
//         plasma <- plasma'
//         // Plasma.repaint' hueShift plasma
//     ) |> ignore
//     mainloop.setDraw (fun interp ->
//         draw2 plasma
//     ) |> ignore

//     mainloop.setEnd (fun fps panic ->
//         fpsCounter.textContent <- sprintf "%A FPS" (round fps)
//         if panic then
//             let discardedTime = round(mainloop.resetFrameDelta())
//             printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
//     ) |> ignore
//     mainloop.start () |> ignore

// document.getElementById("plasmabycanvas") :?> Browser.Types.HTMLHRElement
// |> fun node ->
//     node.onclick <- fun _ ->
//         if current <> PlasmaByCanvas then
//             mainloop.stop() |> ignore
//             dispose ()

//             startPlasmaByCanvas ()
//             if not <| isNull currentNode then
//                 currentNode.removeAttribute "class"
//             node.setAttribute("class", "active")
//             currentNode <- node
//             current <- PlasmaByCanvas

// let startPlasmaByTable () =
//     let w, h = 10, 10
//     let grid =
//         TableCanvas.createGrid w h gameDiv
//     TableCanvas.fill grid

//     let mutable plasma = Plasma.createPlasma w h
//     let mutable hueShift = 0.

//     mainloop.setUpdate (fun _ ->
//         hueShift <- (hueShift + 0.02) % 5.
//         // let plasma' = Plasma.repaint hueShift plasma
//         Plasma.repaint' hueShift plasma
//         // plasma <- plasma'
//     ) |> ignore
//     mainloop.setDraw (fun _ -> TableCanvas.draw plasma grid) |> ignore
//     mainloop.setEnd (fun fps panic ->
//         fpsCounter.textContent <- sprintf "%A FPS" (round fps)
//         if panic then
//             let discardedTime = round(mainloop.resetFrameDelta())
//             printfn "Main loop panicked, probably because the browser tab was put in the background. Discarding %A ms" discardedTime
//     ) |> ignore
//     mainloop.start () |> ignore

// document.getElementById("plasmabytable") :?> Browser.Types.HTMLHRElement
// |> fun node ->
//     node.onclick <- fun _ ->
//         if current <> PlasmaByTable then
//             mainloop.stop() |> ignore
//             dispose ()

//             startPlasmaByTable()

//             if not <| isNull currentNode then
//                 currentNode.removeAttribute "class"
//             node.setAttribute("class", "active")
//             currentNode <- node
//             current <- PlasmaByTable

// let game () =
//     let myDisplay = document.createElement "p" :?> Browser.Types.HTMLParagraphElement
//     myDisplay.setAttribute("style", "font-family: Consolas,monaco,monospace;")
//     gameDiv.appendChild myDisplay |> ignore

//     let table = document.createElement "table" :?> Browser.Types.HTMLTableElement
//     gameDiv.appendChild table |> ignore

//     let row = table.insertRow()

//     let cell = row.insertCell()
//     let btnLeft = document.createElement "button" :?> Browser.Types.HTMLButtonElement
//     btnLeft.innerText <- "⬅️"
//     cell.appendChild btnLeft |> ignore

//     let cell = row.insertCell()
//     let btnRight = document.createElement "button" :?> Browser.Types.HTMLButtonElement
//     btnRight.innerText <- "➡️"
//     cell.appendChild btnRight |> ignore

//     let worldLength = 15
//     let world =
//         Array.init worldLength (fun i ->
//             // char(i % 10 + 48)
//             string (i % 10)
//         )
//     let viewportRadius = 2
//     let pan x = MapEngine.sandbox worldLength world viewportRadius 1. x worldLength

//     let mutable idx = 0

//     let render idx =
//         let worldIdx, len, xs = pan idx
//         // printfn "worldIdx = %A" worldIdx
//         let idx' =
//             if idx < viewportRadius + 1 then idx
//             elif idx > worldLength - (viewportRadius + 1) then
//                 let d = viewportRadius + idx - (worldLength - viewportRadius - 1)
//                 d
//             else viewportRadius
//         xs.[idx'] <- "😱"

//         let f xs =
//             xs
//             |> Array.map string
//             |> String.concat "|" |> sprintf "|%s|"

//         myDisplay.textContent <- f xs

//     do
//         render idx

//     let res =
//         {|
//             Left = fun _ ->
//                 if idx > 0 then
//                     idx <- idx - 1
//                 render idx
//             Right = fun _ ->
//                 if idx < worldLength - 1 then
//                     idx <- idx + 1
//                 render idx
//         |}
//     btnLeft.onclick <- res.Left
//     btnRight.onclick <- res.Right

// document.getElementById("roguelike") :?> Browser.Types.HTMLHRElement
// |> fun node ->
//     node.onclick <- fun _ ->
//         if current <> Roguelike then
//             mainloop.stop() |> ignore
//             dispose ()

//             game ()

//             if not <| isNull currentNode then
//                 currentNode.removeAttribute "class"
//             node.setAttribute("class", "active")
//             currentNode <- node
//             current <- Roguelike



open Elmish
open Feliz
open Browser

type Page =
    | LissajousPage
    | BullsAndCowsPage
    | TowerOfHanoiPage
    | ConvertorsPage
    | HangmanPage
    | GenerateWordsPage

type State =
    {
        LissajousState: Lissajous.State
        BullsAndCowsState: BullsAndCows.State
        TowerOfHanoiState: TowerOfHanoi.State
        ConvertorsState: Convertors.State
        HangmanState: Hangman.State
        GenerateWordsState: GenerateWords.State
        CurrentPage: Page
    }

type Msg =
    | LissajousMsg of Lissajous.Msg
    | BullsAndCowsMsg of BullsAndCows.Msg
    | TowerOfHanoiMsg of TowerOfHanoi.Msg
    | ConvertorsMsg of Convertors.Msg
    | HangmanMsg of Hangman.Msg
    | GenerateWordsMsg of GenerateWords.Msg
    | ChangePage of Page
    | ChangeUrl of segments:string list

let changePage state =
    Mainloop.mainloop.stop() |> ignore
    function
    | LissajousPage ->
        let pageState, cmd = Lissajous.init ()
        let state =
            { state with
                LissajousState = pageState
                CurrentPage = LissajousPage }
        state, cmd
    | BullsAndCowsPage ->
        let pageState, cmd = BullsAndCows.init ()
        let state =
            { state with
                BullsAndCowsState = pageState
                CurrentPage = BullsAndCowsPage }
        state, cmd
    | TowerOfHanoiPage ->
        let pageState, cmd = TowerOfHanoi.init ()
        let state =
            { state with
                TowerOfHanoiState = pageState
                CurrentPage = TowerOfHanoiPage }
        state, cmd
    | ConvertorsPage ->
        let pageState, cmd = Convertors.init ()
        let state =
            { state with
                ConvertorsState = pageState
                CurrentPage = ConvertorsPage }
        state, cmd
    | HangmanPage ->
        let pageState, cmd = Hangman.init ()
        let state =
            { state with
                HangmanState = pageState
                CurrentPage = HangmanPage }
        state, cmd
    | GenerateWordsPage ->
        let pageState, cmd = GenerateWords.init ()
        let state =
            { state with
                GenerateWordsState = pageState
                CurrentPage = GenerateWordsPage }

        state, Cmd.map GenerateWordsMsg cmd

open Feliz.Router

[<Literal>]
let LissajousRoute = "lissajous"
[<Literal>]
let BullsAndCowsRoute = "BullsAndCows"
[<Literal>]
let TowerOfHanoiRoute = "TowerOfHanoi"
[<Literal>]
let ConvertorsRoute = "Convertors"
[<Literal>]
let HangmanRoute = "Hangman"
[<Literal>]
let GenerateWordsRoute = "GenerateWords"

let parseUrl state segments =
    match segments with
    | LissajousRoute::_ ->
        changePage state LissajousPage
    | BullsAndCowsRoute::_ ->
        changePage state BullsAndCowsPage
    | TowerOfHanoiRoute::_ ->
        changePage state TowerOfHanoiPage
    | ConvertorsRoute::_ ->
        changePage state ConvertorsPage
    | HangmanRoute::_ ->
        changePage state HangmanPage
    | GenerateWordsRoute::_ ->
        changePage state GenerateWordsPage
    | _ ->
        state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with

    | ChangePage page -> changePage state page
    | LissajousMsg msg ->
        let lissajousState, cmd =
            Lissajous.update msg state.LissajousState
        let state =
            { state with
                LissajousState = lissajousState }
        state, cmd
    | ChangeUrl segments ->
        parseUrl state segments
    | BullsAndCowsMsg msg ->
        let state', cmd =
            BullsAndCows.update msg state.BullsAndCowsState
        let state =
            { state with
                BullsAndCowsState = state' }
        state, cmd
    | TowerOfHanoiMsg msg ->
        let state', cmd =
            TowerOfHanoi.update msg state.TowerOfHanoiState
        let state =
            { state with
                TowerOfHanoiState = state' }
        state, cmd
    | ConvertorsMsg msg ->
        let state', cmd =
            Convertors.update msg state.ConvertorsState
        let state =
            { state with
                ConvertorsState = state' }
        state, cmd
    | HangmanMsg msg ->
        let state', cmd =
            Hangman.update msg state.HangmanState
        let state =
            { state with
                HangmanState = state' }
        state, cmd
    | GenerateWordsMsg msg ->
        let state', cmd =
            GenerateWords.update msg state.GenerateWordsState
        let state =
            { state with
                GenerateWordsState = state' }
        state, Cmd.map GenerateWordsMsg cmd

let init () =
    let state =
        {
            LissajousState = Lissajous.initState
            CurrentPage = LissajousPage
            BullsAndCowsState = fst (BullsAndCows.init ())
            TowerOfHanoiState = fst (TowerOfHanoi.init ())
            ConvertorsState = fst (Convertors.init ())
            HangmanState = fst (Hangman.init ())
            GenerateWordsState = fst (GenerateWords.init ())
        }
    Router.currentUrl()
    |> parseUrl state

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome


let navBrand (state : State) (dispatch : Msg -> unit) =
    Navbar.Brand.div [] [
        Navbar.Item.a [
            Navbar.Item.Props [
                Href "https://gretmn102.github.io/"
                Target "_blank"
            ]
        ] [
            Fa.i [ Fa.Solid.Home ] []
        ]

        Navbar.Item.a [
            let isActive = state.CurrentPage = LissajousPage
            Navbar.Item.IsActive isActive
            // if not isActive then
            Navbar.Item.Props [ Href (Router.format LissajousRoute) ]
        ] [
            // Fa.i [ Fa.Solid.FileAlt ] []
            str "Lissajous"
        ]

        Navbar.Item.a [
            let isActive = state.CurrentPage = BullsAndCowsPage
            Navbar.Item.IsActive isActive
            Navbar.Item.Props [ Href (Router.format BullsAndCowsRoute) ]
        ] [
            str "BullsAndCows"
        ]
        Navbar.Item.a [
            let isActive = state.CurrentPage = TowerOfHanoiPage
            Navbar.Item.IsActive isActive
            Navbar.Item.Props [ Href (Router.format TowerOfHanoiRoute) ]
        ] [
            str "TowerOfHanoi"
        ]
        Navbar.Item.a [
            let isActive = state.CurrentPage = ConvertorsPage
            Navbar.Item.IsActive isActive
            Navbar.Item.Props [ Href (Router.format ConvertorsRoute) ]
        ] [
            str "Convertors"
        ]
        Navbar.Item.a [
            let isActive = state.CurrentPage = HangmanPage
            Navbar.Item.IsActive isActive
            Navbar.Item.Props [ Href (Router.format HangmanRoute) ]
        ] [
            str "Hangman"
        ]
        Navbar.Item.a [
            let isActive = state.CurrentPage = GenerateWordsPage
            Navbar.Item.IsActive isActive
            Navbar.Item.Props [ Href (Router.format GenerateWordsRoute) ]
        ] [
            str "GenerateWords"
        ]
    ]

let view (state : State) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.IsFullHeight
    ] [
        Hero.head [] [
            Navbar.navbar [] [
                Container.container [] [
                    navBrand state dispatch
                ]
            ]
        ]

        Hero.body [] [
            Feliz.React.router [
                router.onUrlChanged (ChangeUrl >> dispatch)
                router.children [
                    Container.container [] [
                        Column.column [
                        ] [
                            match state.CurrentPage with
                            | LissajousPage ->
                                Lissajous.containerBox state.LissajousState (LissajousMsg >> dispatch)
                            | BullsAndCowsPage ->
                                BullsAndCows.containerBox state.BullsAndCowsState (BullsAndCowsMsg >> dispatch)
                            | TowerOfHanoiPage ->
                                TowerOfHanoi.containerBox state.TowerOfHanoiState (TowerOfHanoiMsg >> dispatch)
                            | ConvertorsPage ->
                                Convertors.containerBox state.ConvertorsState (ConvertorsMsg >> dispatch)
                            | HangmanPage ->
                                Hangman.containerBox state.HangmanState (HangmanMsg >> dispatch)
                            | GenerateWordsPage ->
                                GenerateWords.containerBox state.GenerateWordsState (GenerateWordsMsg >> dispatch)
                        ]
                    ]
                ]
            ]
        ]
    ]