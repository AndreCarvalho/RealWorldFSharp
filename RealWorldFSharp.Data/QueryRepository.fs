namespace RealWorldFSharp.Data

open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Common.Errors

module QueryRepository =
    
    type IoQueryResult<'a> = Async<'a option>

    type GetApplicationUser = UserManager<ApplicationUser> -> Username -> IoQueryResult<ApplicationUser>
    type GetUserFollowing = ApplicationDbContext -> UserId -> IoResult<UserFollowingEntity>
    
    let getApplicationUser : GetApplicationUser =
        fun userManager username ->
            async {
                let! user = userManager.FindByNameAsync username |> Async.AwaitTask
                return Option.ofObj user
            }
            
    let getUserFollowing : GetUserFollowing =
        fun dbContext userId ->
            async {
                try
                    let query = query {
                        for f in dbContext.UsersFollowing do
                        where (f.FollowerId = userId)
                        select f
                    }
                    
                    let userFollowing = {
                        Id = userId
                        Following = query |> Seq.map (fun x -> x.FollowedId) |> List.ofSeq
                    }
                    
                    return Ok userFollowing
                with
                | ex -> return GenericError ex |> Error //TODO: let it crash?
            }