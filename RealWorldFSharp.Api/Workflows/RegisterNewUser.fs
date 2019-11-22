namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Api.Models.Request
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Api.Models
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Domain.Users
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors

module RegisterNewUser =
    
    type RegisterNewUserWorkflow(
                                    userManager: UserManager<IdentityUser>,
                                    jwtOption: IOptions<JwtConfiguration>
                                ) =
        member __.Execute(command: RegisterNewUser) =
            let registerNewUser user password =
                async {
                    let! res = userManager.CreateAsync(user, password) |> Async.AwaitTask
                    
                    if res.Succeeded then
                        let! userId = userManager.GetUserIdAsync(user) |> Async.AwaitTask
                        return UserId.create "userId" userId |> expectValidationError
                    else
                        let firstError = res.Errors |> Seq.head
                        return identityError firstError.Code firstError.Description |> expectUsersError 
                }
                
            asyncResult {
                let! userName = Username.create "username" command.User.Username |> expectValidationError
                let! emailAddress = EmailAddress.create "email" command.User.Email |> expectValidationError
                let! password = Password.create "password" command.User.Password |> expectValidationError
                
                let identityUser = new IdentityUser (
                                        Id = Guid.NewGuid().ToString(),
                                        UserName = userName.Value,
                                        Email = emailAddress.Value
                                    )
            
                let! userId = registerNewUser identityUser password.Value
                
                let user = {
                    Username = userName
                    EmailAddress = emailAddress
                    Id = userId
                }
                
                let token = Authentication.createToken jwtOption.Value user
                
                return {
                    User = {
                        Username = userName.Value
                        Email = emailAddress.Value
                        Bio = null
                        Image = null
                        Token = token
                    }
                }
            }