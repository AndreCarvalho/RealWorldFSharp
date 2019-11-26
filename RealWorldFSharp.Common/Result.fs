namespace RealWorldFSharp.Common

[<RequireQualifiedAccess>]
module Result =

    let combine results =
        let rec loop acc results =
            match results with
            | [] -> acc
            | result :: tail ->
                match result with
                | Error e -> Error e
                | Ok ok ->
                    let acc = Result.map (fun oks -> ok :: oks) acc
                    loop acc tail
        loop (Ok []) results

    let ofOption err opt =
        match opt with
        | Some v -> Ok v
        | None -> Error err

