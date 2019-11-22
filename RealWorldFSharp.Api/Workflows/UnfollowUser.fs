namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api.DataAccess
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Common.Errors

module UnfollowUser =
    type UnfollowUserWorkflow(
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<IdentityUser>
                           ) =
        member __.Execute(currentUserName, userNameToUnfollow) =
            let getUser userName =
                async {
                    let! user = userManager.FindByNameAsync(userName) |> Async.AwaitTask
                    
                    if isNull user then
                        return userNotFoundError userName |> expectUsersError
                    else
                        return Ok user
                }
            let removeUserFollowing userFollowing =
                async {
                    do (dbContext.UsersFollowing.Remove userFollowing) |> ignore
                    let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                    return ()
                }
            asyncResult {
                let! (identityUserToUnfollow: IdentityUser) = getUser userNameToUnfollow
                let! (currentIdentityUser: IdentityUser) = getUser currentUserName
                
                let query = query {
                    for f in dbContext.UsersFollowing do
                    where (f.FollowedId = identityUserToUnfollow.Id && f.FollowerId = currentIdentityUser.Id)
                    select f
                    count
                }
                
                if query = 1 then                   
                    let userFollowing = { FollowerId = currentIdentityUser.Id; FollowedId = identityUserToUnfollow.Id } 
                    do! removeUserFollowing userFollowing
                
                return {
                    Profile = {
                        Username = userNameToUnfollow
                        Bio = null
                        Image = null
                        Following = false
                    }
                }
            }

