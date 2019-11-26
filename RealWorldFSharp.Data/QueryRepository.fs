namespace RealWorldFSharp.Data
open DataEntities
open Microsoft.AspNetCore.Identity

module QueryRepository =
    
    type IoQueryResult<'a> = Async<'a option>

    type GetApplicationUser = UserManager<ApplicationUser> -> Username -> IoQueryResult<ApplicationUser>
    
    let getApplicationUser : GetApplicationUser =
        fun userManager username ->
            async {
                let! user = userManager.FindByNameAsync username |> Async.AwaitTask
                return Option.ofObj user
            }