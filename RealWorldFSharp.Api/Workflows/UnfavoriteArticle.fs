namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Data.Write
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Domain.Articles

type UnfavoriteArticleWorkflow(
                               dbContext: ApplicationDbContext,
                               databaseOptions: IOptions<Database>
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
            
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! articleQuery = ReadModelQueries.getArticleById context (Some userId.Value) (article.Id.ToString())
            return articleQuery.Value |> QueryModels.toSingleArticleEnvelopeReadModel
        }