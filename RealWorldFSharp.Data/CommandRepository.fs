namespace RealWorldFSharp.Data

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Common.Errors

module CommandRepository =
    
    type RegisterNewUser = (ApplicationUser * Password) -> UserIdentityResult<unit>
    type AuthenticateUser = (EmailAddress * Password) -> UserIdentityResult<ApplicationUser>
    type UpdateUserEmail = (ApplicationUser * EmailAddress) -> UserIdentityResult<ApplicationUser>
    type UpdateUserUsername = (ApplicationUser * Username) -> UserIdentityResult<ApplicationUser>
    
    // TODO: this needs old and new password to work
    type UpdateUserPassword = (ApplicationUser * Password) -> UserIdentityResult<ApplicationUser>
    type UpdateUserInfo = ApplicationUser -> UserIdentityResult<ApplicationUser>
    
    let registerNewUser (userManager: UserManager<ApplicationUser>) : RegisterNewUser =
        fun (applicationUser, password) ->
            async {
                let! res = userManager.CreateAsync(applicationUser, password) |> Async.AwaitTask
                
                if res.Succeeded then
                    return Ok ()
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }
            
    let authenticateUser (userManager: UserManager<ApplicationUser>) : AuthenticateUser =
        fun (email, password) ->
            async {
                let! user = userManager.FindByEmailAsync email |> Async.AwaitTask
                
                if user = null then
                    return userNotFoundError email
                else
                    let! res = userManager.CheckPasswordAsync(user, password) |> Async.AwaitTask
                    
                    if res then
                        return Ok user
                    else
                        return wrongPasswordError email
            }
            
    let updateUserEmail (userManager: UserManager<ApplicationUser>) : UpdateUserEmail =
        fun (user, email) ->
            async {
                let! res = userManager.SetEmailAsync(user, email) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }
            
    let updateUserUsername (userManager: UserManager<ApplicationUser>) : UpdateUserUsername =
        fun (user, username) ->
            async {
                let! res = userManager.SetUserNameAsync(user, username) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }            
    let updateUserInfo (userManager: UserManager<ApplicationUser>) : UpdateUserInfo =
        fun user ->
            async {
                let! res = userManager.UpdateAsync(user) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }