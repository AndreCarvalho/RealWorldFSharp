namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Domain
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open FsToolkit.ErrorHandling

module RetrieveUser =
    
    type RetrieveUserWorkflow (
                                    userManager: UserManager<IdentityUser>,
                                    jwtOptions: IOptions<JwtConfiguration>
                              ) =
        member __.Execute(userName) =
            let getUser userName =
                async {
                    let! user = userManager.FindByNameAsync userName |> Async.AwaitTask
                    // TODO: could this be null??
                    return user
                }
            
            asyncResult {
                let! identityUser = getUser userName
                
                let! userName = Username.create "username" identityUser.UserName |> expectValidationError
                let! emailAddress = EmailAddress.create "email" identityUser.Email |> expectValidationError
                let! userId = UserId.create "id" identityUser.Id |> expectValidationError

                let user = {
                    Username = userName
                    EmailAddress = emailAddress
                    Id = userId
                }
                
                let token = Authentication.createToken jwtOptions.Value user
                
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
