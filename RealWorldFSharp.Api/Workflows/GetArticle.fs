namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

type GetArticleWorkflow (
                           dbContext: ApplicationDbContext,
                           userManager: UserManager<ApplicationUser>
                        ) =
    member __.Execute(currentUsernameOption, articleSlug) =           
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! userInfoOption = DataPipeline.getUserInfoById userManager article.UserId 
            let! (userInfo, _) = noneToError userInfoOption article.UserId.Value |> expectDataRelatedError

            let! profileModel =
                match currentUsernameOption with
                | Some currentUsername ->
                    asyncResult {
                        let! userFollowing = currentUsername |> Helper.getUserFollowing dbContext userManager 
                        return userInfo |> QueryModels.toProfileModel userFollowing
                    }
                | None ->
                    userInfo |> QueryModels.toSimpleProfileModel |> AsyncResult.retn
            
            return article |> QueryModels.toSingleArticleEnvelope profileModel
        }