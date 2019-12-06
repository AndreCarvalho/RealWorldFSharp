namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModels
open RealWorldFSharp.Domain.Articles

type CreateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               readDataContext: ReadDataContext
                           ) =
    member __.Execute(userId, command: CreateArticleCommandModel) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException
            
            let! cmd = validateCreateArticleCommand command |> expectValidationError
            
            let now = DateTimeOffset.UtcNow
            let article = createArticle userId cmd.Title cmd.Description cmd.Body cmd.Tags now

            do! DataPipeline.addArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            let! articleQuery = ReadModelQueries.getArticle readDataContext (article.Id.ToString())
            return articleQuery |> QueryModels.toSingleArticleEnvelopeReadModel
        }