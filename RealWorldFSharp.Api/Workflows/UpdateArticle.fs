namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Domain.Users

type UpdateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               databaseOptions: IOptions<Database>
                           ) =
    member __.Execute(userId, articleSlug, command: UpdateArticleCommandModel) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! article = validateArticleOwner "update article" article userId |> expectOperationNotAllowedError
            
            let! cmd = validateUpdateArticleCommand command |> expectValidationError
            let now = DateTimeOffset.UtcNow
            let article = article |> updateArticle cmd.Title cmd.Body cmd.Description now
            
            do! DataPipeline.updateArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! articleQuery = ReadModelQueries.getArticleById context (Some userId.Value) (article.Id.ToString())
            return articleQuery.Value |> QueryModels.toSingleArticleEnvelopeReadModel
        }