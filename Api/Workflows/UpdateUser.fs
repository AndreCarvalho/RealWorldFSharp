namespace Api.Workflows

open Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open Api.Models.Response
open Api.Authentication
open Api.Domain.Users
open Api.Settings
open Api.Errors
open Api.Models.Request
open Api.Models.Response
open FsToolkit.ErrorHandling

module UpdateUser =
    
    type UpdateUserWorkflow (
                                    userManager: UserManager<IdentityUser>,
                                    jwtOptions: IOptions<JwtConfiguration>                                
                            ) =
        member __.Execute(userName, updateUserData: UpdateUserData) =
            let getUser userName =
                async {
                    let! user = userManager.FindByNameAsync userName |> Async.AwaitTask
                    // TODO: could this be null??
                    return user
                }
                
            let updateEmail user newEmail =
                async {
                    let! res = userManager.SetEmailAsync(user, newEmail) |> Async.AwaitTask
                    if res.Succeeded then
                        return Ok ()
                    else
                        let firstError = res.Errors |> Seq.head
                        return identityError firstError.Code firstError.Description |> expectUsersError 
                }
                
            let updateUsername user newUsername =
                async {
                    let! res = userManager.SetUserNameAsync(user, newUsername) |> Async.AwaitTask
                    if res.Succeeded then
                        return Ok ()
                    else
                        let firstError = res.Errors |> Seq.head
                        return identityError firstError.Code firstError.Description |> expectUsersError 
                }
            
            asyncResult {
                let! identityUser = getUser userName

                if not <| isNull updateUserData.Username then
                    let! newUsername = Username.create "username" updateUserData.Username |> expectValidationError
                    do! updateUsername identityUser newUsername.Value
                    
                if not <| isNull updateUserData.Email then
                    let! newEmailAddress = EmailAddress.create "email" updateUserData.Email |> expectValidationError
                    do! updateEmail identityUser newEmailAddress.Value                
                    
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
            

