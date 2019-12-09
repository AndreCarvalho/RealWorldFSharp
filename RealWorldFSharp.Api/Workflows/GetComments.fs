namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModels
open RealWorldFSharp.Api.Settings

type GetCommentsWorkflow (
                               dbContext: ApplicationDbContext,
                               readDataContext: ReadDataContext,
                               databaseOptions: IOptions<Database>
                           ) =
    member __.Execute(userIdOption, articleSlug) =
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! comments = ReadModelQueries.getCommentsForArticle databaseOptions.Value.ConnectionString userIdOption (article.Id.ToString())
            return comments |> QueryModels.toCommentsReadModelEnvelope
        }

