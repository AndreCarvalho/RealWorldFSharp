namespace RealWorldFSharp.Api

open System.Collections.Generic
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Api.Models.Response

module ModelsMapping =
    
    let private singletonError field description =
        let errors = dict <| Seq.singleton (field, Array.singleton description)
        { Errors = new Dictionary<string, string array>(errors) }
    
    let mapValidationError { FieldPath = path; Message = message } =
        singletonError path message
        
    let mapUsersError error =
        match error with
        | IdentityError (code, description) -> singletonError code description
        | UserNotFound username -> singletonError "authentication" (sprintf "User '%s' not found" username)
        | WrongPassword username -> singletonError "authentication" (sprintf "Incorrect password for user '%s'" username)