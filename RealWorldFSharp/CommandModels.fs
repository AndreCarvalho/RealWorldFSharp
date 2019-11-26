namespace RealWorldFSharp

open FsToolkit.ErrorHandling
open Domain
open RealWorldFSharp.Common.Errors

module CommandModels =
   
    [<CLIMutable>]
    type RegisterNewUserCommandModel = {
        Username: string
        Email: string
        Password: string
    }
    
    [<CLIMutable>]
    type RegisterNewUserCommandModelEnvelope = {
        User: RegisterNewUserCommandModel
    }
    
    [<CLIMutable>]
    type AuthenticateUserCommandModel = {
        Email: string
        Password: string
    }        
    
    [<CLIMutable>]
    type AuthenticateUserCommandModelEnvelope = {
        User: AuthenticateUserCommandModel
    }
    
    [<CLIMutable>]
    type UpdateUserCommandModel = {
        Bio: string
        Image: string
        Email: string
        Username: string
    }
    
    [<CLIMutable>]
    type UpdateUserCommandModelEnvelope = {
        User: UpdateUserCommandModel
    }
    
     type AuthenticateUserCommand = {
        EmailAddress: EmailAddress
        Password: Password
    }
    
    type UpdateUserCommand = {
        Bio: Bio option
        Image: Image option
        EmailAddress: EmailAddress option
        Username: Username option
    }

    
    type ValidateRegisterNewUserCommand = RegisterNewUserCommandModel -> ValidationResult<UserInfo * Password>
    type ValidateAuthenticateUserCommand = AuthenticateUserCommandModel -> ValidationResult<AuthenticateUserCommand>
    type ValidateUpdateUserCommand = UpdateUserCommandModel -> ValidationResult<UpdateUserCommand>

    let validateRegisterNewUserCommand userId : ValidateRegisterNewUserCommand =
        fun command ->
            result {
                let! userId = UserId.create "userId" userId
                let! userName = Username.create "username" command.Username
                let! emailAddress = EmailAddress.create "email" command.Email
                let! password = Password.create "password" command.Password
                
                return ({
                    Username = userName
                    EmailAddress = emailAddress
                    Id = userId
                    Bio = None
                    Image = None
                }, password)
            }
            
    let validateAuthenticateUserCommand : ValidateAuthenticateUserCommand =
        fun command ->
            result {
                let! emailAddress = EmailAddress.create "email" command.Email
                let! password = Password.create "password" command.Password
                
                return {
                    EmailAddress = emailAddress
                    Password = password
                }
            }
            
    let validateUpdateUserCommand: ValidateUpdateUserCommand =
        fun command ->
            result {
                let! emailAddress = command.Email
                                    |> Option.ofObj 
                                    |> Option.map (EmailAddress.create "email")
                                    |> Option.sequenceResult
                let! username = command.Username
                                    |> Option.ofObj 
                                    |> Option.map (Username.create "username")
                                    |> Option.sequenceResult
                                    
                let bio = command.Bio |> Option.ofObj
                let image = command.Image |> Option.ofObj
                
                return {
                    Bio = bio
                    Image = image
                    EmailAddress = emailAddress
                    Username = username
                } 
            }