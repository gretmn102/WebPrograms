module Convertors

module RomanDigits =
    let ofInt32 num =
        let mutable num = num
        let roman = System.Text.StringBuilder()

        let totalM = num / 1000
        num <- num % 1000
        let totalCM = num / 900
        num <- num % 900

        let totalD = num / 500
        num <- num % 500
        let totalCD = num / 400
        num <- num % 400

        let totalC = num / 100
        num <- num % 100
        let totalXC = num / 90
        num <- num % 90

        let totalL = num / 50
        num <- num % 50
        let totalXL = num / 40
        num <- num % 40

        let totalX = num / 10
        num <- num % 10
        let totalIX = num / 9
        num <- num % 9

        let totalV = num / 5
        num <- num % 5
        let totalIV = num / 4
        num <- num % 4
        let repeatString(s, count) = String.replicate count s
        roman.Append(repeatString("M", totalM))
             .Append(repeatString("CM", totalCM))
             .Append(repeatString("D", totalD))
             .Append(repeatString("CD", totalCD))
             .Append(repeatString("C", totalC))
             .Append(repeatString("XC", totalXC))
             .Append(repeatString("L", totalL))
             .Append(repeatString("XL", totalXL))
             .Append(repeatString("X", totalX))
             .Append(repeatString("IX", totalIX))
             .Append(repeatString("V", totalV))
             .Append(repeatString("IV", totalIV))
             .Append(repeatString("I", num))
             .ToString()

type State =
    {
        Input: int
        TranslatedText: string
    }
type Msg =
    | SetInput of int

open Elmish

let init () =
    let initNumber = 1
    let state =
        {
            Input = initNumber
            TranslatedText = RomanDigits.ofInt32 initNumber
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetInput num ->
        let state =
            { state with
                TranslatedText = RomanDigits.ofInt32 num
                Input = num
            }
        state, Cmd.none

open Feliz
open Browser
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome

let containerBox (state : State) (dispatch : Msg -> unit) =
    Box.box' [] [
        Columns.columns [] [
            Column.column [
            ] [
                Columns.columns [] [
                    Column.column [] [
                        Field.div [ Field.IsGrouped ] [
                            Control.p [ Control.IsExpanded ] [
                                Input.number [
                                    Input.Placeholder "Number"
                                    Input.Value (string state.Input)
                                    Input.OnChange (fun e ->
                                        match e with
                                        | null -> ()
                                        | e ->
                                            match System.Int32.TryParse e.Value with
                                            | true, v ->
                                                v
                                                |> SetInput
                                                |> dispatch
                                            | _ ->
                                                e.returnValue <- false
                                    )
                                    Input.Option.Props [
                                        Step "1"
                                        Min "1"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            Column.column [] [
                Columns.columns [] [
                    Column.column [] [
                        Content.content [] [
                            Field.div [ Field.IsGrouped ] [
                                Control.p [ Control.IsExpanded ] [
                                    Input.text [
                                        Input.Disabled true
                                        Input.Value state.TranslatedText
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Columns.columns [ Columns.IsCentered ] [
                    Column.column [] [
                        match Browser.Navigator.navigator.clipboard with
                        | Some clipboard ->
                            Button.button [
                                Button.OnClick (fun _ ->
                                    let text =
                                        state.TranslatedText
                                    clipboard.writeText text
                                    |> ignore
                                )
                            ] [
                                Fa.span [ Fa.Solid.Clipboard; Fa.FixedWidth] []
                            ]
                        | None -> ()
                    ]
                ]
            ]
        ]
    ]
