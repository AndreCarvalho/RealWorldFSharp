namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Domain
open RealWorldFSharp.Data

module Helper =
    let getUserFollowing dbContext userManager username =
        asyncResult {
            let currentUsername = Username.create "username" username |> valueOrException
            let! currentUserInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername
            let! (currentUserInfo, _) = noneToUserNotFoundError currentUserInfoOption currentUsername.Value |> expectUsersError
            return! DataPipeline.getUserFollowing dbContext currentUserInfo.Id |> expectDataRelatedErrorAsync
        }

