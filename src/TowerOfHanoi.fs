module TowerOfHanoi

open Elmish
open Feliz
open Browser

type Disc = { Length: int; }

type Pole =
    {
        MaxLength: int
        Discs: Disc list
    }

type MovementState =
    | HasNotSelectedYet
    | SelectedPoleId of int

type State =
    {
        Poles: Pole []

        MovementState: MovementState
    }

type Msg =
    | Select of int

let init () =
    let maxLength = 5
    let state =
        {
            Poles =
                [|
                    {
                        Discs = List.init maxLength (fun i -> { Length = i + 1 })
                        MaxLength = maxLength
                    }

                    {
                        Discs = [] // List.init 7 (fun i -> { Length = i + 1 })
                        MaxLength = maxLength
                    }

                    {
                        Discs = [] // List.init 8 (fun i -> { Length = i + 1 })
                        MaxLength = maxLength
                    }
                |]

            MovementState = HasNotSelectedYet
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | Select poleId ->
        assert
            0 <= poleId && poleId < state.Poles.Length
        match state.MovementState with
        | HasNotSelectedYet ->

            let state =
                { state with
                    MovementState = SelectedPoleId poleId }
            state, Cmd.none
        | SelectedPoleId lastPoleId ->
            assert
                lastPoleId <> poleId

            let poles = state.Poles
            let getPoleById i =
                // function
                // | 0 -> state.Pole1
                // | 1 -> state.Pole2
                // | 2 -> state.Pole3
                // | _ -> failwith ""
                poles.[i]

            let from = getPoleById lastPoleId
            let to' = getPoleById poleId

            match from.Discs, to'.Discs with
            | x::xs, [] ->
                poles.[lastPoleId] <- { from with Discs = xs }
                poles.[poleId] <- { to' with Discs = [x] }
            | x::xs, y::ys ->
                if x < y then
                    poles.[lastPoleId] <- { from with Discs = xs }
                    poles.[poleId] <- { to' with Discs = x::y::ys }
            | _ -> ()
            let state =
                { state with
                    Poles = poles

                    MovementState = HasNotSelectedYet }
            state, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome

let poleRender (pole:Pole) =
    [
        yield! List.replicate (pole.MaxLength - pole.Discs.Length) (td [] [unbox (char 0x200B)])
        for disc in pole.Discs do
            td [] [
                str (string disc.Length)
            ]
    ]

module List =
     // let zip xs ys =
    //     List.map2 (fun x y -> x::y) xs ys
    let rec map3 fn xs ys zs =
        let rec f acc = function
            | x::xs, y::ys, z::zs ->
                f (fn x y z :: acc) (xs, ys, zs)
            | [], [], [] -> List.rev acc
            | _ -> failwith "lists has different lengths"
        f [] (xs, ys, zs)


let containerBox (state : State) (dispatch : Msg -> unit) =
    Box.box' [] [
        Table.table [
        ] [
            tbody [] [
                tr [] [
                    let f i isActive =
                        td [] [
                            Button.button [
                                Button.IsActive isActive
                                if isActive then
                                    Button.OnClick (fun e ->
                                        Select i
                                        |> dispatch
                                    )
                            ] [
                                Fa.i [ Fa.Solid.Minus ] []
                            ]
                        ]
                    yield!
                        match state.MovementState with
                        | HasNotSelectedYet ->
                            List.init state.Poles.Length
                                (fun i ->
                                    let isEmpty i =
                                        let pole = state.Poles.[i]
                                        List.isEmpty pole.Discs
                                    f i (not (isEmpty i)))
                        | SelectedPoleId i ->
                            List.init state.Poles.Length (fun i' -> f i' (i <> i'))
                ]
                yield!
                    List.map3
                        (fun x y z ->
                            tr [] [x; y; z]
                        )
                        (poleRender state.Poles.[0])
                        (poleRender state.Poles.[1])
                        (poleRender state.Poles.[2])
            ]
        ]
    ]
