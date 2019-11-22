namespace RealWorldFSharp

open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Common

module Domain =
    
    type Username = private Username of string
        with
        member this.Value = match this with Username un -> un
        static member create fieldName username =
            if isNullOrEmpty username then
                validationError fieldName "username must not be null or empty"
            else
                Ok <| Username username
                
    type EmailAddress = private EmailAddress of string
        with
        member this.Value = match this with EmailAddress ea -> ea
        static member create fieldName email =
            if Common.isNullOrEmpty email then
                validationError fieldName "email must not be null or empty"
            else
                Ok <| EmailAddress email
                
    type UserId = private UserId of string
        with
        member this.Value = match this with UserId id -> id
        static member create fieldName userId =
            if Common.isNullOrEmpty userId then
                validationError fieldName "user id must not be null or empty"
            else
                Ok <| UserId userId
    
    type Password = private Password of string
        with
        member this.Value = match this with Password pass -> pass
        static member create fieldName password =
            if Common.isNullOrEmpty password then
                validationError fieldName "password must not be null or empty"
            else
                Ok <| Password password

    type User = {
        Id: UserId
        Username: Username
        EmailAddress: EmailAddress
    }