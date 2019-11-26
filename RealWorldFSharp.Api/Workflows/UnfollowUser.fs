namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

module UnfollowUser =
    type UnfollowUserWorkflow(
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
        member __.Execute(currentUserName, userNameToUnfollow) =
            asyncResult {
                let! usernameToUnfollow = Username.create "username" userNameToUnfollow |> expectValidationError
                let currentUsername = Username.create "username" currentUserName |> valueOrException

                let! userInfoOption = DataPipeline.getUserInfo userManager usernameToUnfollow 
                let! (userInfoToUnfollow, _) = noneToUserNotFoundError userInfoOption usernameToUnfollow.Value |> expectUsersError
                
                let! currentUserInfoOption = DataPipeline.getUserInfo userManager currentUsername
                let! (currentUserInfo, _) = noneToUserNotFoundError currentUserInfoOption currentUsername.Value |> expectUsersError
                
                dbContext.ChangeTracker.QueryTrackingBehavior <- QueryTrackingBehavior.NoTracking
                let! userFollowing = DataPipeline.getUserFollowing dbContext currentUserInfo.Id |> expectDataRelatedErrorAsync
                
                let (userFollowing, result) = removeFromUserFollowing userInfoToUnfollow.Id userFollowing
                if result = Removed then
                    do! DataPipeline.removeUserFollowing dbContext (currentUserInfo.Id, userInfoToUnfollow.Id) |> expectDataRelatedErrorAsync
                
                return userInfoToUnfollow |> toProfileModelEnvelope userFollowing
            }

