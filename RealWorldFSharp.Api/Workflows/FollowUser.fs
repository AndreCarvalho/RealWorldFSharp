namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api.DataAccess
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Common.Errors

module FollowUser =
    
    type FollowUserWorkflow(
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<IdentityUser>
                           ) =
        member __.Execute(currentUserName, userNameToFollow) =
            let getUser userName =
                async {
                    let! user = userManager.FindByNameAsync(userName) |> Async.AwaitTask
                    
                    if isNull user then
                        return userNotFoundError userName |> expectUsersError
                    else
                        return Ok user
                }
            let addUserFollowing userFollowing =
                async {
                    let! res = (dbContext.UsersFollowing.AddAsync userFollowing).AsTask() |> Async.AwaitTask
                    let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                    return ()
                }
            asyncResult {
                let! (identityUserToFollow: IdentityUser) = getUser userNameToFollow
                let! (currentIdentityUser: IdentityUser) = getUser currentUserName
                
                let query = query {
                    for f in dbContext.UsersFollowing do
                    where (f.FollowedId = identityUserToFollow.Id && f.FollowerId = currentIdentityUser.Id)
                    select f
                    count
                }
                
                if query = 0 then                   
                    let userFollowing = { FollowerId = currentIdentityUser.Id; FollowedId = identityUserToFollow.Id } 
                    do! addUserFollowing userFollowing
                
                return {
                    Profile = {
                        Username = userNameToFollow
                        Bio = null
                        Image = null
                        Following = true
                    }
                }
            }