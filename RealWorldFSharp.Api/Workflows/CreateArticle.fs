namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Domain.Articles

type CreateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               databaseOptions: IOptions<Database>
                           ) =
    member __.Execute(userId, command: CreateArticleCommandModel) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException
            
            let! cmd = validateCreateArticleCommand command |> expectValidationError
            
            let now = DateTimeOffset.UtcNow
            let article = createArticle userId cmd.Title cmd.Description cmd.Body cmd.Tags now

            do! DataPipeline.addArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! articleQuery = ReadModelQueries.getArticleById context (Some userId.Value) (article.Id.ToString())
            return articleQuery.Value |> QueryModels.toSingleArticleEnvelopeReadModel
        }