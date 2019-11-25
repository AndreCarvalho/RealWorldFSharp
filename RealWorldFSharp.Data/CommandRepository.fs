namespace RealWorldFSharp.Data

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Common.Errors

module CommandRepository =
    
    type RegisterNewUser = (ApplicationUser * string) -> UserIdentityResult<unit>
    type AuthenticateUser = (string * string) -> UserIdentityResult<ApplicationUser>
    
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
