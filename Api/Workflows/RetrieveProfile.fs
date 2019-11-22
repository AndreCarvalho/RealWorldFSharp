namespace Api.Workflows

open Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open Api.Models.Response
open Api.Authentication
open Api.DataAccess
open Api.Domain.Users
open Api.Settings
open Api.Errors
open Api.Models.Request
open Api.Models.Response
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling

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
