namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

module FollowUser =
    
    type FollowUserWorkflow(
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
        member __.Execute(currentUserName, userNameToFollow) =
            asyncResult {
                let! usernameToFollow = Username.create "username" userNameToFollow |> expectValidationError
                let currentUsername = Username.create "username" currentUserName |> valueOrException

                let! userInfoOption = DataPipeline.getUserInfoByUsername userManager usernameToFollow 
                let! (userInfoToFollow, _) = noneToUserNotFoundError userInfoOption usernameToFollow.Value |> expectUsersError
                
                let! currentUserInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername
                let! (currentUserInfo, _) = noneToUserNotFoundError currentUserInfoOption currentUsername.Value |> expectUsersError
                
                let! userFollowing = DataPipeline.getUserFollowing dbContext currentUserInfo.Id |> expectDataRelatedErrorAsync
                
                let (userFollowing, result) = addToUserFollowing userInfoToFollow.Id userFollowing
                if result = Added then
                    do! DataPipeline.addUserFollowing dbContext (currentUserInfo.Id, userInfoToFollow.Id) |> expectDataRelatedErrorAsync
                    do! dbContext.SaveChangesAsync()
                
                return userInfoToFollow |> toProfileModelEnvelope userFollowing
            }