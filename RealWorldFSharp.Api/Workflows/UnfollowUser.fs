namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

type UnfollowUserWorkflow(
                           dbContext: ApplicationDbContext,
                           userManager: UserManager<ApplicationUser>
                       ) =
    member __.Execute(currentUserId, userNameToUnfollow) =
        asyncResult {
            let! usernameToUnfollow = Username.create "username" userNameToUnfollow |> expectValidationError

            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager usernameToUnfollow 
            let! (userInfoToUnfollow, _) = noneToUserNotFoundError userInfoOption usernameToUnfollow.Value |> expectUsersError
            
            let userId = currentUserId |> (UserId.create "userId") |> valueOrException
            
            dbContext.ChangeTracker.QueryTrackingBehavior <- QueryTrackingBehavior.NoTracking
            let! userFollowing = DataPipeline.getUserFollowing dbContext userId |> expectDataRelatedErrorAsync
            
            let (userFollowing, result) = removeFromUserFollowing userInfoToUnfollow.Id userFollowing
            if result = Removed then
                do! DataPipeline.removeUserFollowing dbContext (userId, userInfoToUnfollow.Id) |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
                
            return userInfoToUnfollow |> toProfileModelEnvelope userFollowing
        }

