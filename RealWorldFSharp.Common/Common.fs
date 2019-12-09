namespace RealWorldFSharp.Common
open System


[<AutoOpen>]
module Common =
    let inline (|HasLength|) x = 
      fun () -> (^a: (member Length: int) x)

    let inline (|HasCount|) x = 
      fun () -> (^a: (member Count: int) x)

    let inline length (HasLength f) = f()

    let inline isNullOrEmpty arg =
        if arg = null || (length arg) = 0 then true
        else false

    let bindAsync f a =
        async {
            let! a = a
            return! f a
        }
    
[<RequireQualifiedAccess>]
module Nullable =
    open System
    
    let from (a:'a) =
        new Nullable<'a>(a)
        
    let empty<'a when 'a: struct and 'a: (new: unit-> 'a) and 'a:> System.ValueType> =
        new Nullable<'a>()
        
    let defaultWith defaultVal (x: Nullable<'a>)  =
        if x.HasValue then x.Value
        else defaultVal

[<RequireQualifiedAccess>]
module Option =
    let valueOrException msg (option: Option<'a>): 'a =
        match option with
        | Some v -> v
        | None -> failwith msg
        
