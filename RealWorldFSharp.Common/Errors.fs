namespace RealWorldFSharp.Common

module Errors =
    type ValidationError =
        { FieldPath: string
          Message: string }

    type DataRelatedError =
        | EntityAlreadyExists of entityName: string * id: string
        | EntityNotFound of entityName: string * id: string
        | EntityIsInUse of entityName: string * id: string
        | UpdateError of entityName:string * id: string * message:string

    type UsersError =
        | IdentityError of code: string * description: string
        | UserNotFound of username: string
        | WrongPassword of username: string
       
    type Error =
        | ValidationError of ValidationError
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

    let expectValidationError result = Result.mapError ValidationError result
    
    let expectUsersError result = Result.mapError UsersError result

//    let expectOperationNotAllowedError result = Result.mapError OperationNotAllowed result

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
    
