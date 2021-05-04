module BullsAndCows
// "быки и коровы"
/// символ правильный и стоит на правильном месте - bulls
/// символ правильный, но не на своем месте - cows
/// -> `bulls * cows`
let test pattern =
    let elems = Set.ofSeq pattern
    fun guess ->
        Seq.zip pattern guess
        |> Seq.fold
            (fun (bulls, cows) (p, g) ->
                if p = g then
                    bulls + 1, cows
                elif Set.contains g elems then
                    bulls, cows + 1
                else bulls, cows
            )
            (0, 0)

// Как считать повторения?
// допустим, загадана последовательность
// если задать 3141
// то возможно два варианта:
// * `test "2113" "3141"` -> 1 бык, 2 коровы
// * `test2 "2113" "3141"`
let testRep pattern =
    let elems = Seq.countBy id pattern |> Map.ofSeq
    fun guess ->
        Seq.zip pattern guess
        |> Seq.fold
            (fun ((bulls, cows), m) (p,g) ->
                if p = g then
                    let m =
                        let y = Map.find p m
                        Map.add p (y - 1) m
                    (bulls + 1, cows), m
                else
                    match Map.tryFind g m with
                    | None | Some 0 ->
                        (bulls, cows), m
                    | Some x ->
                        (bulls, cows + 1), Map.add g (x - 1) m
            )
            ((0, 0), elems)
        |> fst

let testRepTests () =
    let t : seq<int> -> _ = testRep [1;2;1;3;3]
    [
        t [5;5;5;5;5] = (0, 0)
        t [1;1;2;2;2] = (1, 2)
        t [1;2;3;4;4] = (2, 1)
        t [3;3;4;4;4] = (0, 2)

    ] |> List.forall id

let secretNumberGenerate length =
    let digits = ResizeArray [| 0..9 |]
    let r = System.Random()
    length
    |> List.unfold
        (fun i ->
            if i > 0 then
                let i' = r.Next(0, digits.Count)
                let x = digits.[i']
                digits.RemoveAt i'
                Some (x, (i - 1))
            else
                None
        )

let secretNumberGenerateTests () =
    Seq.init 300 (fun i -> secretNumberGenerate 4)
    |> Seq.tryFind (fun x ->
        List.distinct x <> x
    )
    |> (=) None

module Abstract =
    type Lose =
        | SecretNumberIs of string
    and Res =
        | TryAgain of (int * int) * (unit -> MainLoop)
        | Win
    and Act =
        {
            Guess: (string -> Res)
            GiveUp: (unit -> Lose)
        }
    and MainLoop =
        | Act of Act

    type State =
        { SecretNumber: string }

    let rec loop st =
        Act
            {
                Guess = fun guess ->
                    let secretNumber = st.SecretNumber
                    let bulls, cows = test secretNumber guess

                    if secretNumber.Length = bulls then
                        Win
                    else
                        TryAgain((bulls, cows), fun () ->
                            loop st
                        )
                GiveUp = fun () ->
                    SecretNumberIs st.SecretNumber
            }

type T =
    | MainLoop of Abstract.MainLoop
    | GameOver of {| IsWin: bool; SecretNumber: string |}

type State =
    {
        Attempts: (string * (int * int)) list
        GameState: T
        Input: string
    }

type Msg =
    | GiveUp
    | SetInput
    | UpdateInput of string
    | StartGame
open Elmish

let init () =
    let state =
        {
            Input = ""
            GameState =
                let gameState =
                    { Abstract.SecretNumber =
                        // "1234"
                        secretNumberGenerate 4
                        |> System.String.Concat
                    }
                MainLoop (Abstract.loop gameState)
            Attempts = []
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetInput ->
        let state =
            match state.GameState with
            | MainLoop x ->
                match x with
                | Abstract.Act act ->
                    let guess = state.Input
                    match act.Guess guess with
                    | Abstract.TryAgain(res, f) ->
                        { state with
                            Input = ""
                            Attempts = (guess, res) :: state.Attempts
                            GameState = MainLoop (f ())
                        }
                    | Abstract.Win ->
                        { state with
                            Input = ""
                            GameState =
                                GameOver
                                    {| IsWin = true
                                       SecretNumber = state.Input |} }
            | x -> failwithf "expected MainLoop but %A" x
        state, Cmd.none
    | GiveUp ->
        let state =
            match state.GameState with
            | MainLoop x ->
                match x with
                | Abstract.Act act ->
                    match act.GiveUp () with
                    | Abstract.SecretNumberIs secretNumber ->
                        { state with
                            GameState =
                                GameOver
                                    {| IsWin = false
                                       SecretNumber = secretNumber |}
                        }
            | x -> failwithf "expected MainLoop but %A" x
        state, Cmd.none
    | UpdateInput input ->
        let state =
            { state with
                Input = input
            }
        state, Cmd.none
    | StartGame ->
        init ()
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Feliz

let containerBox (state : State) (dispatch : Msg -> unit) =
    Box.box' [] [
        match state.GameState with
        | MainLoop _ ->
            Panel.panel [] [
                Button.button [
                    Button.OnClick (fun e ->
                        dispatch GiveUp
                    )
                ] [
                    Html.text "Give up"
                ]
            ]

            let description = "Four digits from 0 to 9 inclusive, no repetitions"
            Panel.panel [] [
                Field.div [] [
                    let isError = false

                    Label.label [] [ str description ]
                    Control.div [] [
                        Field.div [ Field.HasAddons ] [
                            let isValid = state.Input.Length = 4

                            Control.p [
                                Control.IsExpanded
                                if isError then
                                    Control.HasIconRight
                            ] [
                                Input.input [
                                    if isError then
                                        Input.Color IsDanger
                                    Input.Placeholder description
                                    Input.Value state.Input
                                    Input.OnChange (fun e ->
                                        let str = e.Value
                                        if String.forall System.Char.IsDigit str then
                                            str
                                            |> UpdateInput
                                            |> dispatch
                                    )

                                    Input.Props [

                                        OnKeyDown (fun e ->
                                            if isValid && e.key = "Enter" then
                                                SetInput
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
                                            SetInput
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
                            [ str "Something wrong with this image" ]
                ]
            ]

            state.Attempts
            |> List.map (fun (guess, (bulls, cows)) ->
                Html.li [
                    Html.span [ str guess ]
                    str " "
                    Html.span [
                        Fa.i [ ] [ str "🐂" ]
                        str (string bulls)
                    ]
                    str " "
                    Html.span [
                        // Fa.i [ ] [ str "🐮" ]
                        Fa.i [ ] [ str "🐄" ]
                        str (string cows)
                    ]
                ]
            )
            |> Html.div
        | GameOver res ->
            Html.div [
                if res.IsWin then
                    str (sprintf "You win at %d steps!" (state.Attempts.Length + 1))
                else
                    str "You lose!"
            ]

            Html.div [
                str (sprintf "Secret number is %s" res.SecretNumber)
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