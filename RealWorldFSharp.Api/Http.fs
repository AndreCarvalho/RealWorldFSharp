namespace RealWorldFSharp.Api

open System.Collections.Generic
open Microsoft.AspNetCore.Mvc
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.QueryModels

module Http =
    
    let private singletonError field description =
        let errors = dict <| Seq.singleton (field, Array.singleton description)
        { Errors = new Dictionary<string, string array>(errors) }
    
    let private mapValidationError { FieldPath = path; Message = message } =
        singletonError path message
    
    let private errorToActionResult (controller: ControllerBase) err =
        match err with
        | UsersError ue ->
            match ue with
            | IdentityError (code, desc) -> controller.UnprocessableEntity(singletonError code desc) :> IActionResult
            | UserNotFound _ -> controller.NotFound() :> IActionResult
            | WrongPassword username -> controller.BadRequest(singletonError "authentication" (sprintf "Incorrect password for user '%s'" username)) :> IActionResult
        | ValidationError er -> controller.BadRequest(mapValidationError er) :> IActionResult
        | DataError derr ->
            match derr with
            | EntityNotFound _ -> controller.NotFound() :> IActionResult
            | _ -> failwith "Unexpected data error"
        | Bug ex -> failwith (ex.ToString())
    
    let resultToActionResult (controller: ControllerBase) result =
        match result with
        | Ok x -> controller.Ok(x) :> IActionResult
        | Error err -> errorToActionResult controller err
