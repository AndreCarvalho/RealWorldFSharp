namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

module RetrieveProfile =
    
    type RetrieveProfileWorkflow(
                                    dbContext: ApplicationDbContext,
                                    userManager: UserManager<ApplicationUser>
                                ) =
        member __.Execute(currentUsernameOption, profileUserName) =
            let getUserFollowing username =
                asyncResult {
                    let currentUsername = Username.create "username" username |> valueOrException
                    let! currentUserInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername
                    let! (currentUserInfo, _) = noneToUserNotFoundError currentUserInfoOption currentUsername.Value |> expectUsersError
                    return! DataPipeline.getUserFollowing dbContext currentUserInfo.Id |> expectDataRelatedErrorAsync
                }
                
            asyncResult {
                let! profileUsername = Username.create "username" profileUserName |> expectValidationError
                let! profileUserInfoOption = DataPipeline.getUserInfoByUsername userManager profileUsername 
                let! (profileUserInfo, _) = noneToUserNotFoundError profileUserInfoOption profileUsername.Value |> expectUsersError
                
                return!
                    match currentUsernameOption with
                    | Some currentUsername ->
                        asyncResult {
                            let! userFollowing = getUserFollowing currentUsername
                            return profileUserInfo |> toProfileModelEnvelope userFollowing
                        }
                    | None -> profileUserInfo |> toSimpleProfileModelEnvelope |> AsyncResult.retn
            }