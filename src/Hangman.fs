module Hangman


module Game =
    type State =
        {
            OpenedChars: (char * bool) list
            AttemptsLeft: int
        }
    // type SuccessResult =
    //     | Win
    //     | Continue of (unit -> MainLoop)

    type GuessCharResult =
        | Success of int list
        | Failure of secretWord:string

    and Act = GuessChar of (char -> GuessCharResult * (unit -> MainLoop))

    and MainLoop =
        | Act of Act
        | EndGame of isWin:bool

    let rec mainLoop (state: State) =
        if state.AttemptsLeft > 0 then
            if List.forall snd state.OpenedChars then
                EndGame true
            else
                Act(
                    GuessChar(fun guessChar ->
                        let openedChars, (_, res) =
                            state.OpenedChars
                            |> List.mapFold
                                (fun (i, acc) (c, isOpened) ->
                                    if isOpened then
                                        (c, isOpened), (i + 1, acc)
                                    else
                                        if guessChar = c then
                                            (c, true), (i + 1, i::acc)
                                        else
                                            (c, isOpened), (i + 1, acc)
                                )
                                (0, [])
                        if List.isEmpty res then
                            let attemptsLeft = state.AttemptsLeft - 1
                            let state =
                                { state with
                                    AttemptsLeft = attemptsLeft
                                }
                            let secretWord =
                                state.OpenedChars
                                |> List.map fst
                                |> System.String.Concat
                            Failure secretWord, (fun () -> mainLoop state)
                        else
                            let state =
                                { state with
                                    OpenedChars = openedChars
                                }
                            Success res, (fun () -> mainLoop state)
                    )
                )
        else
            EndGame false

    let init attemptsLeft (secretWord:string) =
        let last = secretWord.Length - 1
        {
            OpenedChars =
                secretWord
                |> Seq.mapi (fun i c -> c, i = 0 || i = last)
                |> List.ofSeq
            AttemptsLeft = attemptsLeft
        }
        |> mainLoop

type T =
    | MainLoop of Game.MainLoop
    | GameOver of {| IsWin: bool; SecretWord: string |}

type State =
    {
        AttemptsLeft: int
        GameState: T
        Word: char option []
        Input: char option
        UsedChars: char Set
    }

type Msg =
    | GuessChar
    | UpdateInput of char option
    | StartGame

open Elmish

let init () =
    let r = System.Random()
    let word =
        // "работа"
        Words.rusFreqNouns.[r.Next(0, Words.rusFreqNouns.Length)]
    let wordLength = word.Length
    let firstChar = word.[0]
    let lastChar = word.[wordLength - 1]

    let attemptsLeft = 5

    let initState = {
        AttemptsLeft = attemptsLeft
        GameState =
            MainLoop (Game.init attemptsLeft word)
        Input = None
        Word =
            let xs = Array.create wordLength None

            word
            |> String.iteri (fun i c ->
                if firstChar = c || lastChar = c then
                    xs.[i] <- Some c
            )

            xs
        UsedChars =
            Set.empty
            |> Set.add firstChar
            |> Set.add lastChar
    }
    initState, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | GuessChar ->
        let state =
            match state.Input with
            | Some guessChar ->
                let state =
                    { state with
                        Input = None
                        UsedChars = Set.add guessChar state.UsedChars
                    }
                match state.GameState with
                | MainLoop m ->
                    match m with
                    | Game.Act (Game.GuessChar f) ->
                        let res, f = f guessChar
                        match res with
                        | Game.Success indexes ->
                            let word =
                                let word = state.Word
                                indexes
                                |> List.iter (fun i ->
                                    word.[i] <- Some guessChar
                                )
                                word
                            { state with
                                GameState =
                                    match f () with
                                    | Game.EndGame isWin ->
                                        {|
                                            IsWin = isWin
                                            SecretWord =
                                                word
                                                |> Array.map (
                                                    function
                                                    | Some x -> x
                                                    | None -> failwith "state.Word has None"
                                                )
                                                |> System.String.Concat
                                        |}
                                        |> GameOver
                                    | res ->
                                        MainLoop res
                                Word = word
                            }
                        | Game.Failure secretWord ->
                            { state with
                                AttemptsLeft = state.AttemptsLeft - 1
                                GameState =
                                    match f () with
                                    | Game.EndGame isWin ->
                                        {|
                                            IsWin = isWin
                                            SecretWord = secretWord
                                        |}
                                        |> GameOver
                                    | res ->
                                        MainLoop res
                            }
                    | Game.EndGame isWin ->
                        failwith "Abstract.EndGame isWin"
                | GameOver(isWin) -> failwith "GameOver(isWin)"
            | None -> failwith "state.Input is empty"
        state, Cmd.none
    | UpdateInput c ->
        let state =
            { state with
                Input = c
            }
        state, Cmd.none
    | StartGame ->
        init ()

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Feliz

let containerBox' (state : State) (dispatch : Msg -> unit) =
    Box.box' [
    ] [
        match state.GameState with
        | MainLoop(_) ->
            Html.div [
                prop.text (sprintf "Attempts left: %d" state.AttemptsLeft)
            ]

            let word =
                state.Word
                |> Array.map (function
                    | Some c -> string c
                    | None -> "_"
                )
                |> String.concat " "

            Html.div [
                prop.text word
            ]

            let description = "One char"
            Panel.panel [] [
                Field.div [] [
                    let isError =
                        match state.Input with
                        | Some x ->
                            Set.contains x state.UsedChars
                        | None -> false

                    Label.label [] [ str description ]
                    Control.div [] [
                        Field.div [ Field.HasAddons ] [
                            let isValid =
                                Option.isSome state.Input && not isError

                            Control.p [
                                Control.IsExpanded
                                if isError then
                                    Control.HasIconRight
                            ] [
                                Input.input [
                                    if isError then
                                        Input.Color IsDanger
                                    Input.Placeholder description
                                    Input.Value (match state.Input with Some x -> string x | None -> "")
                                    Input.OnChange (fun e ->
                                        let str = e.Value
                                        if str.Length > 0 then
                                            UpdateInput (Some str.[str.Length - 1])
                                        else
                                            UpdateInput None
                                        |> dispatch

                                    )

                                    Input.Props [
                                        OnKeyDown (fun e ->
                                            if isValid && e.key = "Enter" then
                                                GuessChar
                                                |> dispatch
                                        )
                                    ]
                                ]
                                if isError then
                                    Icon.icon [ Icon.Size IsSmall; Icon.IsRight ]
                                        [ Fa.i [ Fa.Solid.ExclamationTriangle ] [] ]
                            ]
                            Control.p [] [
                                Button.button [
                                    Button.Disabled (not isValid)
                                    if isValid then
                                        Button.OnClick (fun e ->
                                            GuessChar
                                            |> dispatch
                                        )
                                ] [
                                    Fa.i [ Fa.Solid.Check ] []
                                ]
                            ]
                        ]
                    ]
                    if isError then
                        Help.help [ Help.Color IsDanger ]
                            [ str "This letter has already been used" ]
                ]
            ]
        | GameOver res ->
            Html.div [
                if res.IsWin then
                    str "You win!"
                else
                    str "You lose!"
            ]
            Html.div [
                str (sprintf "Secret number is %s" res.SecretWord)
            ]

            Html.div [
                Button.button [
                    Button.OnClick (fun e ->
                        dispatch StartGame
                    )
                ] [
                    str "Start"
                ]
            ]
    ]
let containerBox state dispatch =
    Column.column [
        Column.Width (Screen.All, Column.Is6)
        Column.Offset (Screen.All, Column.Is3)
    ] [
        containerBox' state dispatch
    ]