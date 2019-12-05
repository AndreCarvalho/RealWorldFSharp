namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Domain.Articles

type UnfavoriteArticleWorkflow(
                               dbContext: ApplicationDbContext,
                               readDataContext: ReadDataContext
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
            
            let! (articleReadModel, favoriteCount) = ReadModelQueries.getArticle readDataContext (article.Id.ToString())
            return articleReadModel |> QueryModels.toSingleArticleEnvelopeReadModel favoriteCount false
        }