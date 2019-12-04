namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Domain.Articles

type UnfavoriteArticleWorkflow(
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                            ) =
    member __.Execute(currentUserId, articleSlug) =
        asyncResult {
            let userId = currentUserId |> (UserId.create "userId") |> valueOrException

            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! favoriteArticles = DataPipeline.getFavoriteArticles dbContext userId |> expectDataRelatedErrorAsync
            let result = favoriteArticles |> removeFromFavorites article.Id
            
            if result = Remove then
                do! DataPipeline.removeFavoriteArticle dbContext (userId, article.Id) |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
            
            let! userInfoOption = DataPipeline.getUserInfoById userManager article.AuthorUserId 
            let! (userInfo, _) = noneToError userInfoOption article.AuthorUserId.Value |> expectDataRelatedError
            
            // TODO: return read model version
            
            return article |> QueryModels.toSingleArticleEnvelope (userInfo |> QueryModels.toSimpleProfileModel)
        }