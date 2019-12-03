namespace RealWorldFSharp.Api

open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Common
open RealWorldFSharp.Api.QueryModels

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
        | OperationNotAllowed ona -> controller.Unauthorized(singletonError ona.Operation ona.Reason) :> IActionResult
        | DataError derr ->
            match derr with
            | EntityNotFound _ -> controller.NotFound() :> IActionResult
            | _ -> failwith "Unexpected data error"
        | Bug ex -> failwith (ex.ToString())
    
    let resultToActionResult (controller: ControllerBase) result =
        match result with
        | Ok x -> controller.Ok(x) :> IActionResult
        | Error err -> errorToActionResult controller err
        

    let private tryGetFromUserClaims (httpContext: HttpContext) claim =
        httpContext.User.Claims
        |> Seq.tryFind (fun x -> x.Type = claim)
        |> Option.map (fun x -> x.Value)
        
    let getUserNameOption (httpContext: HttpContext) =
        tryGetFromUserClaims httpContext ClaimTypes.Name
        
    let getUserName (httpContext: HttpContext) =
        httpContext
        |> getUserNameOption
        |> Option.valueOrException "could not find username in Claims"
        
    let getUserIdOption (httpContext: HttpContext) =
        tryGetFromUserClaims httpContext ClaimTypes.NameIdentifier
        
    let getUserId (httpContext: HttpContext) =
        httpContext
        |> getUserIdOption
        |> Option.valueOrException "could not find user Id in Claims"
