namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Domain

type GetArticleWorkflow (
                           dbContext: ApplicationDbContext,
                           userManager: UserManager<ApplicationUser>
                        ) =
    member __.Execute(userIdOption, articleSlug) =           
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! authorUserInfoOption = DataPipeline.getUserInfoById userManager article.AuthorUserId 
            let! (authorUserInfo, _) = noneToError authorUserInfoOption article.AuthorUserId.Value |> expectDataRelatedError

            let! profileModel =
                match userIdOption with
                | Some userId ->
                    asyncResult {
                        let userId = userId |> ( UserId.create "userId") |> valueOrException
                        let! userFollowing = userId |> Helper.getUserFollowing dbContext userManager 
                        return authorUserInfo |> QueryModels.toProfileModel userFollowing
                    }
                | None ->
                    authorUserInfo |> QueryModels.toSimpleProfileModel |> AsyncResult.retn
            
            return article |> QueryModels.toSingleArticleEnvelope profileModel
        }