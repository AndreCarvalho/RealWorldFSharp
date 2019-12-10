namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModelQueries

type FeedArticlesWorkflow(databaseOptions: IOptions<Database>) =
    member __.Execute(userId, queryModel: FeedArticlesQueryModel) =
        asyncResult {
            let queryParams = {
                Limit = queryModel.Limit |> Nullable.defaultWith 20
                Offset = queryModel.Offset |> Nullable.defaultWith 0
            }            

            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! result = ReadModelQueries.feedArticles context userId queryParams
            return result |> QueryModels.toFeedArticlesEnvelopeReadModel
        }