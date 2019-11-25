namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Api.DataAccess
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities

module RetrieveProfile =
    
    type RetrieveProfileWorkflow(
                                    dbContext: ApplicationDbContext,
                                    userManager: UserManager<ApplicationUser>
                                ) =
        member __.Execute(currentUserNameOption, profileUserName) =
            let getUser userName =
                async {
                    let! user = userManager.FindByNameAsync userName |> Async.AwaitTask
                    
                    if isNull user then
                        return userNotFoundError userName |> expectUsersError
                    else
                        return Ok user
                }
            asyncResult {
                let! (profileIdentityUser: ApplicationUser) = getUser profileUserName
                
                let! following =
                    match currentUserNameOption with
                    | Some userName ->
                        asyncResult {
                            let! (currentIdentityUser: ApplicationUser) = getUser userName
                            
                            let query = query {
                                for f in dbContext.UsersFollowing do
                                where (f.FollowedId = profileIdentityUser.Id && f.FollowerId = currentIdentityUser.Id)
                                select f
                                count
                            }
                            return query = 1
                        }

                    | None -> AsyncResult.retn false
                
                return {
                    Profile = {
                        Username = profileUserName
                        Bio = null
                        Image = null
                        Following = following
                    }
                }
            }
