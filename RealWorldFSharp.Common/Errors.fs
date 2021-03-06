﻿namespace RealWorldFSharp.Common

module Errors =
    type ValidationError =
        { FieldPath: string
          Message: string }

    type OperationNotAllowedError =
        { Operation: string
          Reason: string }
    
    type DataRelatedError =
        | EntityAlreadyExists of entityName: string * id: string
        | EntityNotFound of entityName: string * id: string
        | EntityIsInUse of entityName: string * id: string
        | UpdateError of entityName:string * id: string * message:string
        | DeleteError of entityName:string * id: string * message:string
        | GenericError of exn

    type UsersError =
        | IdentityError of code: string * description: string
        | UserNotFound of username: string
        | WrongPassword of username: string
       
    type Error =
        | ValidationError of ValidationError
        | OperationNotAllowed of OperationNotAllowedError
        | UsersError of UsersError
        | DataError of DataRelatedError
        | Bug of exn

    let validationError fieldPath message = { FieldPath = fieldPath; Message = message } |> Error

    let bug exc = Bug exc |> Error
    
    let identityError code description = IdentityError(code, description) |> Error
    let userNotFoundError username = UserNotFound username |> Error
    let wrongPasswordError username = WrongPassword username |> Error

    let notFound name id = EntityNotFound (name, id) |> Error

    let entityInUse name = EntityIsInUse name |> Error
    
    let operationNotAllowed operation reason = { Operation = operation; Reason = reason } |> Error

    let expectValidationError result = Result.mapError ValidationError result
    
    let expectUsersError result = Result.mapError UsersError result

    let expectOperationNotAllowedError result = Result.mapError OperationNotAllowed result

    let expectDataRelatedError result =
        Result.mapError DataError result

    let expectDataRelatedErrorAsync asyncResult =
        async {
            let! result = asyncResult
            return expectDataRelatedError result
        }
        
    let expectUsersErrorAsync asyncResult =
        async {
            let! result = asyncResult
            return expectUsersError result
        }
        
    let noneToError (a: 'a option) id =
        let error = EntityNotFound (sprintf "%sEntity" typeof<'a>.Name, id)
        Result.ofOption error a

    let noneToUserNotFoundError (a: 'a option) username =
        let error = UserNotFound username
        Result.ofOption error a

    // In here validation error means that invalid data was not provided by user, but instead
    // it was in our system. So if we have this error we throw exception
    let private throwOnValidationError entityName (err: ValidationError) =
        sprintf "Error reading entity [%s]. Field [%s]. Message: %s." entityName err.FieldPath err.Message
        |> failwith

    let valueOrException (result: Result< 'a, ValidationError>) : 'a =
        match result with
        | Ok v -> v
        | Error e -> throwOnValidationError typeof<'a>.Name e        
        
    (*
    Some type aliases for making code more readable and for preventing
    typo-kind of mistakes: so you don't devlare a validation function with
    plain `Error` type, for example.
    *)
    type AsyncResult<'a, 'error> = Async<Result<'a, 'error>>
    type ValidationResult<'a> = Result<'a, ValidationError>
    type IoResult<'a> = AsyncResult<'a, DataRelatedError>
    type UserIdentityResult<'a> = AsyncResult<'a, UsersError>
    type PipelineResult<'a> = AsyncResult<'a, Error>
    type IoQueryResult<'a> = Async<'a option>
    
