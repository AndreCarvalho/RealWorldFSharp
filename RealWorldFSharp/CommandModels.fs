namespace RealWorldFSharp

open FsToolkit.ErrorHandling
open Domain
open RealWorldFSharp.Common.Errors

module CommandModels =
    
    type AuthenticateUserCommand = {
        EmailAddress: EmailAddress
        Password: Password
    }
    
    [<CLIMutable>]
    type NewUserInfoModel = {
        Username: string
        Email: string
        Password: string
    }
    
    [<CLIMutable>]
    type RegisterNewUserCommandModel = {
        User: NewUserInfoModel
    }
    
    [<CLIMutable>]
    type AuthenticationData = {
        Email: string
        Password: string
    }        
    
    [<CLIMutable>]
    type AuthenticateUserCommandModel = {
        User: AuthenticationData
    }
    
    type ValidateRegisterNewUserCommand = RegisterNewUserCommandModel -> ValidationResult<UserInfo * Password>
    type ValidateAuthenticateUserCommand = AuthenticateUserCommandModel -> ValidationResult<AuthenticateUserCommand>

    let validateRegisterNewUserCommand userId : ValidateRegisterNewUserCommand =
        fun command ->
            result {
                let! userId = UserId.create "userId" userId
                let! userName = Username.create "username" command.User.Username
                let! emailAddress = EmailAddress.create "email" command.User.Email
                let! password = Password.create "password" command.User.Password
                
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
                let! emailAddress = EmailAddress.create "email" command.User.Email
                let! password = Password.create "password" command.User.Password
                
                return {
                    EmailAddress = emailAddress
                    Password = password
                }
            }