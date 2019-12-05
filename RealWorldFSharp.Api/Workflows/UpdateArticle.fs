namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels
open RealWorldFSharp.Domain.Users

type UpdateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               readDataContext: ReadDataContext
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
            
            let! articleReadModel = ReadModelQueries.getArticle readDataContext (article.Id.ToString())
            return articleReadModel |> QueryModels.toSingleArticleEnvelopeReadModel
        }