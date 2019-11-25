namespace RealWorldFSharp.Data

open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

module EntityToDomainMapping =
    
    // In here validation error means that invalid data was not provided by user, but instead
    // it was in our system. So if we have this error we throw exception
    let private throwOnValidationError entityName (err: ValidationError) =
        sprintf "Error deserializing entity [%s]. Field [%s]. Message: %s." entityName err.FieldPath err.Message
        |> failwith

    let valueOrException (result: Result< 'a, ValidationError>) : 'a =
        match result with
        | Ok v -> v
        | Error e -> throwOnValidationError typeof<'a>.Name e
    
    let private validateApplicationUser =
        fun (applicationUser: ApplicationUser) ->
            result {
                let! userName = Username.create "username" applicationUser.UserName
                let! emailAddress = EmailAddress.create "email" applicationUser.Email
                let! userId = UserId.create "id" applicationUser.Id
                
                return {
                    Username = userName 
                    EmailAddress = emailAddress
                    Id = userId
                    Bio = Option.ofObj applicationUser.Bio
                    Image = Option.ofObj applicationUser.ImageUrl
                }
            }
    
    let mapApplicationUserToUserInfo (applicationUser: ApplicationUser) : UserInfo =
        validateApplicationUser applicationUser |> valueOrException
