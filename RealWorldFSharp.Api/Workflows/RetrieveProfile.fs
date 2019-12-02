namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

type RetrieveProfileWorkflow(
                                dbContext: ApplicationDbContext,
                                userManager: UserManager<ApplicationUser>
                            ) =
    member __.Execute(currentUsernameOption, profileUserName) =           
        asyncResult {
            let! profileUsername = Username.create "username" profileUserName |> expectValidationError
            let! profileUserInfoOption = DataPipeline.getUserInfoByUsername userManager profileUsername 
            let! (profileUserInfo, _) = noneToUserNotFoundError profileUserInfoOption profileUsername.Value |> expectUsersError
            
            return!
                match currentUsernameOption with
                | Some currentUsername ->
                    asyncResult {
                        let! userFollowing = currentUsername |> Helper.getUserFollowing dbContext userManager 
                        return profileUserInfo |> toProfileModelEnvelope userFollowing
                    }
                | None -> profileUserInfo |> toSimpleProfileModelEnvelope |> AsyncResult.retn
        }