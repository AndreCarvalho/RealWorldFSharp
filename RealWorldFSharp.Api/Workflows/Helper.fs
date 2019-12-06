namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

module Helper =
    let getUserFollowing dbContext userManager userId =
        asyncResult {
            let! currentUserInfoOption = DataPipeline.getUserInfoById userManager userId
            let! currentUserInfo = noneToUserNotFoundError currentUserInfoOption userId.Value |> expectUsersError
            return! DataPipeline.getUserFollowing dbContext currentUserInfo.Id |> expectDataRelatedErrorAsync
        }

