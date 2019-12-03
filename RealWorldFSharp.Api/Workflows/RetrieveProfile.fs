namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

type RetrieveProfileWorkflow(
                                dbContext: ApplicationDbContext,
                                userManager: UserManager<ApplicationUser>
                            ) =
    member __.Execute(currentUserIdOption, profileUserName) =           
        asyncResult {
            let! profileUsername = Username.create "username" profileUserName |> expectValidationError
            let! profileUserInfoOption = DataPipeline.getUserInfoByUsername userManager profileUsername 
            let! (profileUserInfo, _) = noneToUserNotFoundError profileUserInfoOption profileUsername.Value |> expectUsersError
            
            return!
                match currentUserIdOption with
                | Some currentUserId ->
                    asyncResult {
                        let userId = currentUserId |> ( UserId.create "userId") |> valueOrException
                        let! userFollowing = userId |> Helper.getUserFollowing dbContext userManager 
                        return profileUserInfo |> toProfileModelEnvelope userFollowing
                    }
                | None -> profileUserInfo |> toSimpleProfileModelEnvelope |> AsyncResult.retn
        }