namespace RealWorldFSharp.Api.Workflows

open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api.Models.Response
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.DataAccess
open RealWorldFSharp.Api.Domain.Users
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Api.Models.Request
open RealWorldFSharp.Api.Models.Response

module RetrieveProfile =
    
    type RetrieveProfileWorkflow(
                                    dbContext: ApplicationDbContext,
                                    userManager: UserManager<IdentityUser>
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
                let! (profileIdentityUser: IdentityUser) = getUser profileUserName
                
                let! following =
                    match currentUserNameOption with
                    | Some userName ->
                        asyncResult {
                            let! (currentIdentityUser: IdentityUser) = getUser userName
                            
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
