namespace RealWorldFSharp.Api.Workflows

open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api.Models.Request
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Api.Models
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Domain.Users
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors

module AuthenticateUser =
    type AuthenticateUserWorkflow(
                                    userManager: UserManager<IdentityUser>,
                                    jwtOptions: IOptions<JwtConfiguration>
                                ) =
        member __.Execute(command: Request.AuthenticateUser) =
            let authenticateUser email password =
                async {
                    let! user = userManager.FindByEmailAsync email |> Async.AwaitTask
                    
                    if user = null then
                        return userNotFoundError email |> expectUsersError
                    else
                        let! res = userManager.CheckPasswordAsync(user, password) |> Async.AwaitTask
                        
                        if res then
                            return Ok user
                        else
                            return wrongPasswordError email |> expectUsersError
                }
                
            let mapToUser (identityUser: IdentityUser) =
                result {
                    let! userName = Username.create "username" identityUser.UserName
                    let! emailAddress = EmailAddress.create "email" identityUser.Email
                    let! userId = UserId.create "id" identityUser.Id
                    
                    return {
                        Username = userName 
                        EmailAddress = emailAddress
                        Id = userId
                    }
                }
                
            asyncResult {
                let! emailAddress = EmailAddress.create "email" command.User.Email |> expectValidationError
                let! password = Password.create "password" command.User.Password |> expectValidationError
                
                let! identityUser = authenticateUser emailAddress.Value password.Value
                let! user = mapToUser identityUser |> expectValidationError
                let token = Authentication.createToken jwtOptions.Value user
                
                return {
                    User = {
                        Username = user.Username.Value
                        Email = user.EmailAddress.Value
                        Bio = null
                        Image = null
                        Token = token                        
                    }
                }
            }