namespace Api.Workflows

open Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open Api.Models.Response
open Api.Authentication
open Api.Domain.Users
open Api.Settings
open Api.Errors
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
