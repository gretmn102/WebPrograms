module GenerateWords

open Elmish
open Feliz
open Browser

type Deferred<'t> =
  | HasNotStartedYet
  | InProgress
  | Resolved of 't

type Request =
    {
        Url: string
        Method: string
        Body: string
        ContentType:string option
        MimeType: string option
    }

type Response = { StatusCode: int; Body: string }

let httpRequest (request: Request) (responseHandler: Response -> 'Msg) : Cmd<'Msg> =
    let command (dispatch: 'Msg -> unit) =
        let xhr = XMLHttpRequest.Create()
        xhr.``open``(method=request.Method, url=request.Url)
        match request.ContentType with
        | Some x ->
            xhr.setRequestHeader("Content-Type", x)
        | None -> ()
        match request.ContentType with
        | Some mimeType ->
            xhr.overrideMimeType mimeType
        | None -> ()
        xhr.onreadystatechange <- fun _ ->
            if xhr.readyState = Types.ReadyState.Done then
                let response = { StatusCode = xhr.status; Body = xhr.responseText }
                let messageToDispatch = responseHandler response
                dispatch messageToDispatch

        xhr.send(request.Body)

    Cmd.ofSub command

type Word =
    {
        Frenq: float
        Word: string
        Category: string
    }

type State =
    {
        Words: (Result<Word [], string>) Deferred
        CurrentWord: Word option
    }

type Msg =
    | SetWords of Result<string, string>
    | GenerateNoun

let init () =
    let state =
        {
            Words = InProgress
            CurrentWord = None
        }
    let req =
        {
            Url = "lemma.al"
            Method = "GET"
            Body = null
            ContentType = Some "text/plain; charset=windows-1251"
            MimeType = Some "text/plain; charset=windows-1251"
        }
    let cmd =
        httpRequest req (fun x ->
            if x.StatusCode = 200 || x.StatusCode = 0 then
                Ok x.Body
            else
                Error (sprintf "StatusCode is %d\nBody:\n%s" x.StatusCode x.Body)
            |> SetWords
        )
    state, cmd

let random = System.Random()

let update (msg: Msg) (state: State) =
    match msg with
    | SetWords words ->
        let state =
            { state with
                Words =
                    words
                    |> Result.map (fun words ->
                        words.Split "\r\n"
                        |> Array.choose
                            (fun line ->
                                match line.Split [|' '|] with
                                | [| idx; frenq; word; category |] ->
                                    {
                                        Frenq = float frenq
                                        Word = word
                                        Category = category
                                    }
                                    |> Some
                                | x ->
                                    printfn "error at %A" x
                                    None
                            )
                    )
                    |> Resolved
            }
        state, Cmd.none
    | GenerateNoun ->
        let state =
            match state.Words with
            | Resolved (Ok words) ->
                { state with
                    CurrentWord =
                        let nouns =
                            words
                            |> Array.filter (fun word ->
                                word.Frenq > 5.0 && word.Category = "noun")
                        let idx =
                            random.Next(0, nouns.Length)
                        Some nouns.[idx]
                }
            | _ -> state
        state, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Feliz

let spinner =
    Html.div [
        prop.style [
            style.textAlign.center
            style.marginLeft length.auto
            style.marginRight length.auto
        ]
        prop.children [
            Fa.i [ Fa.Spin; Fa.Solid.Spinner; ] []
        ]
    ]

let containerBox' (state : State) (dispatch : Msg -> unit) =
    Box.box' [
    ] [
        match state.Words with
        | Resolved words ->
            match words with
            | Ok words ->
                // Html.orderedList
                //     (words.[0..10]
                //     |> Array.map (fun word ->
                //         Html.li [ str (sprintf "%A" word) ]
                //     ))
                // ()
                match state.CurrentWord with
                | Some currentWord ->
                    Html.div [
                        prop.style [
                                style.justifyContent.center
                                style.display.flex
                        ]
                        prop.text (sprintf "%A" currentWord)
                    ]
                | None -> ()

                Control.p [
                    Control.Props [
                        Style [
                            JustifyContent "center"
                            Display DisplayOptions.Flex
                        ]
                    ]
                ] [
                    Button.button [
                        Button.OnClick (fun e ->
                            GenerateNoun
                            |> dispatch
                        )
                    ] [
                        str "Generate"
                    ]
                ]
            | Error errMsg -> str errMsg
        | InProgress ->
            Html.div [
                prop.style [
                        style.justifyContent.center
                        style.display.flex
                ]
                prop.children [
                    Html.span [
                        prop.text "The dictionary is loading..."
                    ]
                    Fa.i [ Fa.Spin; Fa.Solid.Spinner; ] []
                ]
            ]
        | HasNotStartedYet ->
            str "The dictionary has not started loading yet"
    ]

let containerBox state dispatch =
    Column.column [
        Column.Width (Screen.All, Column.Is6)
        Column.Offset (Screen.All, Column.Is3)
    ] [
        containerBox' state dispatch
    ]