namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.CommandModels
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Domain
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors

module RegisterNewUser =
    
    type RegisterNewUserWorkflow(
                                    userManager: UserManager<IdentityUser>,
                                    jwtOption: IOptions<JwtConfiguration>
                                ) =
        member __.Execute(command: RegisterNewUserCommandModel) =
            let registerNewUser user password =
                async {
                    let! res = userManager.CreateAsync(user, password) |> Async.AwaitTask
                    
                    if res.Succeeded then
                        return Ok ()
                    else
                        let firstError = res.Errors |> Seq.head
                        return identityError firstError.Code firstError.Description |> expectUsersError 
                }
                
            asyncResult {
                let userId = Guid.NewGuid().ToString()              
                let! (user, password) = validateRegisterNewUserCommand userId command |> expectValidationError
                
                let identityUser = new IdentityUser (
                                        Id = user.Id.Value,
                                        UserName = user.Username.Value,
                                        Email = user.EmailAddress.Value
                                    )
            
                do! registerNewUser identityUser password.Value
                             
                let token = Authentication.createToken jwtOption.Value user
                
                return user |> toUserResponse token
            }