namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain.Users

type FollowUserWorkflow(
                           dbContext: ApplicationDbContext,
                           userManager: UserManager<ApplicationUser>
                       ) =
    member __.Execute(currentUserId, userNameToFollow) =
        asyncResult {
            let! usernameToFollow = Username.create "username" userNameToFollow |> expectValidationError
            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager usernameToFollow 
            let! (userInfoToFollow, _) = noneToUserNotFoundError userInfoOption usernameToFollow.Value |> expectUsersError
            
            let userId = currentUserId |> (UserId.create "userId") |> valueOrException

            let! userFollowing = DataPipeline.getUserFollowing dbContext userId |> expectDataRelatedErrorAsync
            
            let (userFollowing, result) = addToUserFollowing userInfoToFollow.Id userFollowing
            if result = Added then
                do! DataPipeline.addUserFollowing dbContext (userId, userInfoToFollow.Id) |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
            
            return userInfoToFollow |> toProfileModelEnvelope userFollowing
        }