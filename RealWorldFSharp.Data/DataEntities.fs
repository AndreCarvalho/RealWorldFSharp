namespace RealWorldFSharp.Data

open Microsoft.AspNetCore.Identity

module DataEntities =
    [<AllowNullLiteral>]
    type ApplicationUser() =
        inherit IdentityUser()
        
        member val Bio:string = null with get, set
        member val ImageUrl:string = null with get, set

    type EmailAddress = { Value: string }
    type Password = { Value: string }