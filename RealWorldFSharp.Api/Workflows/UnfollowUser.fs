namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data.Write
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Domain.Users

type UnfollowUserWorkflow(
                           dbContext: ApplicationDbContext,
                           userManager: UserManager<ApplicationUser>
                       ) =
    member __.Execute(currentUserId, userNameToUnfollow) =
        asyncResult {
            let! usernameToUnfollow = Username.create "username" userNameToUnfollow |> expectValidationError

            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager usernameToUnfollow 
            let! userInfoToUnfollow = noneToUserNotFoundError userInfoOption usernameToUnfollow.Value |> expectUsersError
            
            let userId = currentUserId |> (UserId.create "userId") |> valueOrException
            
            let! userFollowing = DataPipeline.getUserFollowing dbContext userId |> expectDataRelatedErrorAsync
            
            let (userFollowing, result) = removeFromUserFollowing userInfoToUnfollow.Id userFollowing
            if result = Removed then
                do! DataPipeline.removeUserFollowing dbContext (userId, userInfoToUnfollow.Id) |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
                
            return userInfoToUnfollow |> toProfileModelEnvelope userFollowing
        }

