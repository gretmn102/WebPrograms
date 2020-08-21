module FoxEscape

let width = 800
let height = 650
let radius = 300.0
let mutable goblin = 0.0
let mutable boatx = 0.1
let mutable boaty = 0.0
let bspeed = 1.0
let gspeeds = [3.5; 4.0; 4.2; 4.4; 4.6]
let gspeed_ix = 0
let speed_mult = 3.0

type IsWin = bool
let mutable gameOver : IsWin option = None
let inline None< ^T> = unbox (box null) : ^T // как ни странно, но `None` не помогает
let restart () =
    goblin <- 0.0
    boatx <- 0.1
    boaty <- 0.0
    gameOver <- None
open Browser.Types
module CanvasRenderingContext2D =
    let circle (x, y) r lineWidth strokeStyle fillStyle (ctx:CanvasRenderingContext2D) =
        ctx.beginPath()
        ctx.lineWidth <- lineWidth
        ctx.arc(x, y, r, 0., 2. * System.Math.PI, false)
        match fillStyle with
        | Some fillStyle ->
            ctx.fillStyle <- fillStyle
            ctx.fill()
        | _ -> ()
        match strokeStyle with
        | Some strokeStyle ->
            ctx.strokeStyle <- strokeStyle
            ctx.stroke()
        | _ -> ()

open Fable.Core
let rgb (r:byte, g:byte, b:byte) = U3.Case1 (sprintf "rgb(%d, %d, %d)" r g b)
let clear (ctx:CanvasRenderingContext2D) =
    let radius_mult = bspeed / gspeeds.[gspeed_ix]

    ctx.clearRect(0., 0., float width, float height);

    ctx
    |> CanvasRenderingContext2D.circle (float width/2., float height/2.) (radius * 1.0) 0. (Some(rgb (0uy, 80uy, 0uy))) None
    // ctx
    // |> CanvasRenderingContext2D.circle (float width/2., float height/2.) (radius*radius_mult) 1. (Some(rgb (200uy, 200uy, 200uy))) None
let redraw (duckSprite:Browser.Types.HTMLImageElement) (foxSprite:Browser.Types.HTMLImageElement) ctx =
    clear ctx

    let w, h = duckSprite.width / 7., duckSprite.height / 7.
    ctx.drawImage(U3.Case1 duckSprite,
                  float width/2. + boatx - w / 2.,
                  float height/2. + boaty - h / 2.,
                  w,
                  h)

    let w, h = foxSprite.width / 6., foxSprite.height / 6.
    ctx.drawImage(U3.Case1 foxSprite,
                  float width/2. + radius * cos goblin - w / 2.,
                  float height/2. + radius * sin goblin - h / 2.,
                  w,
                  h)
    // let boatr = 6.
    // let goblinr = 6.
    // ctx
    // |> CanvasRenderingContext2D.circle
    //     (float width/2. + boatx, float height/2. + boaty)
    //     boatr
    //     1.
    //     (Some (U3.Case1 "black"))
    //     None
    // ctx
    // |> CanvasRenderingContext2D.circle
    //     (float width/2. + radius * cos goblin, float height/2. + radius * sin goblin)
    //     goblinr
    //     1.
    //     (Some (U3.Case1 "red"))
    //     None

    match gameOver with
    | Some isWin ->
        ctx.font <- "bold 48px serif"
        let centerX, centerY = float width / 2., float height / 2.

        if isWin then
            ctx.fillStyle <- U3.Case1 "black"
            let str = "Escaped!"
            let m = ctx.measureText str
            ctx.fillText(str, centerX - m.width / 2.0, centerY)
        else
            let str = "You Were Eaten"
            ctx.fillStyle <- U3.Case1 "red"
            let m = ctx.measureText str
            ctx.fillText(str, centerX - m.width / 2.0, centerY)
    | None -> ()

open Fable.Core.JS

// Задана произвольная точка и окружность через центр и радиус. Прямая проходит через центр окружности и точку. Нужно найти пересечение окружности и прямой... два пересечения.
// Из этого:
// ```maple
// line := y = (a__x*b__y-a__y*b__x+a__y*x-b__y*x)/(-b__x+a__x);
// circleParametric := { x = r*sin(t) - a__x, y = r*cos(t) - a__y }
// t = map(x -> simplify(x, 'size'), [solve(eval(line, circleParametric), t)]);
// ```
// получилось два гигантских уравнения, которые даже приводить сюда не хочется, не то что использовать. Ума не приложу, как можно иначе решить. Да, это аналитическое решение, а численное черт знает, как получить.
// Собственно, а зачем нам задавать центр окружности, если можно сказать, что она лежит в центре координат?
// ```maple
// line := y = x*a__y/a__x
// t = map(x -> simplify(x, 'size'), [solve(eval(line, { x = r*sin(t) - a__x, y = r*cos(t) - a__y }), t)]);
// ```
// Дает одно единственное решение: `t = [arctan(a__x/a__y)]`!
// Отлично, что дальше?

let updateGoblin radius speed_mult (boatx, boaty) gspeed goblin =
    let newang = atan2 boaty boatx // <=> atan (boatx / boaty)
    let diff = newang - goblin

    let diff = diff + if diff < System.Math.PI then System.Math.PI * 2. else 0.
    let diff = diff - if diff > System.Math.PI then System.Math.PI * 2. else 0.
    let goblin' =
        if abs diff * radius <= gspeed * speed_mult then
            newang
        else
            if diff > 0.0 then
                goblin + gspeed * speed_mult / radius
            else
                goblin - gspeed * speed_mult / radius
    let goblin' = goblin' + if goblin' < System.Math.PI then System.Math.PI * 2. else 0.
    let goblin' = goblin' - if goblin' > System.Math.PI then System.Math.PI * 2. else 0.
    goblin'

    // let mutable diff = newang - goblin
    // if diff < Math.PI then
    //     diff <- diff + Math.PI * 2.
    // if diff > Math.PI then
    //     diff <- diff - Math.PI * 2.
    // if abs diff * radius <= gspeed * speed_mult then
    //     goblin <- newang
    // else
    //     goblin <- goblin + if diff > 0. then gspeed * speed_mult / radius else -gspeed * speed_mult / radius
    // if goblin < Math.PI then
    //     goblin <- goblin + Math.PI * 2.
    // if goblin > Math.PI then
    //     goblin <- goblin - Math.PI * 2.

let test () =
    let gspeeds = 3.5
    let width, height = 1024, 768
    let boatx, boaty = float width / 2., float height / 2.
    let radius = 300.0
    let speed_mult = 3.0
    let goblin = 0.0

    goblin
    |> Seq.unfold (fun goblin ->
        let x = updateGoblin radius speed_mult (boatx, boaty) gspeeds goblin
        Some(x, x)
        )
    |> Seq.take 100
    |> List.ofSeq
let moveBoat (x, y) =
    let dx, dy = x - boatx, y - boaty
    let mag = sqrt (dx*dx + dy*dy)
    if mag <= bspeed * speed_mult then
        boatx <- x
        boaty <- y
    else
        boatx <- boatx + bspeed * speed_mult * dx/mag
        boaty <- boaty + bspeed * speed_mult * dy/mag


let start duckSprite foxSprite (canvas:HTMLCanvasElement) =
    let ctx = canvas.getContext_2d()
    let mutable mouseX, mouseY = 0., 0.
    let mutable isMouseButtonDown = false
    canvas.onmousemove <- fun e ->
        mouseX <- e.offsetX; mouseY <- e.offsetY

    canvas.onmousedown <- fun _ -> isMouseButtonDown <- true
    canvas.onmouseup <- fun _ -> isMouseButtonDown <- false

    {|
        Update = fun () ->
            if Option.isSome gameOver then
                if isMouseButtonDown then
                    restart()
            elif boatx*boatx + boaty*boaty > radius*radius then
                let diff = atan2 boaty boatx - goblin
                let diff = diff + if diff < System.Math.PI then System.Math.PI * 2. else 0.
                let diff = diff - if diff > System.Math.PI then System.Math.PI * 2. else 0.

                let is_win = abs(diff) > 0.000001
                gameOver <- Some is_win
                isMouseButtonDown <- false
            else
                if isMouseButtonDown then
                    moveBoat(mouseX - float width / 2., mouseY - float height / 2.)
                goblin <- updateGoblin radius speed_mult (boatx, boaty) gspeeds.[gspeed_ix] goblin

        Draw = fun () ->
            redraw duckSprite foxSprite ctx
    |}