namespace RealWorldFSharp.Api.Workflows 

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModelQueries

type ListArticlesWorkflow(databaseOptions: IOptions<Database>) =
    member __.Execute(userIdOption, queryModel: ListArticlesQueryModel) =
        asyncResult {
            let queryParams = {
                Tag = Option.ofObj queryModel.Tag
                Author = Option.ofObj queryModel.Author
                Favorited = Option.ofObj queryModel.Favorited
                Limit = queryModel.Limit |> Nullable.defaultWith 20
                Offset = queryModel.Offset |> Nullable.defaultWith 0
            }            

            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! result = ReadModelQueries.listArticles context userIdOption queryParams
            return result |> QueryModels.toMultipleArticlesEnvelopeReadModel
        }
