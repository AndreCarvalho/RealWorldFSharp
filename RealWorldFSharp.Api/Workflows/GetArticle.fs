namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Read

type GetArticleWorkflow (databaseOptions: IOptions<Database>) =
    member __.Execute(userIdOption, articleSlug) =           
        asyncResult {
            let slug = Slug.create articleSlug
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! articleQueryOption = ReadModelQueries.getArticleBySlug context userIdOption slug.Value 
            let! articleQuery = noneToError articleQueryOption slug.Value |> expectDataRelatedError
            return articleQuery |> QueryModels.toSingleArticleEnvelopeReadModel
        }