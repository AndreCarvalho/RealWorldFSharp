namespace RealWorldFSharp.Api.Workflows

open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Data.Read

type GetTagsWorkflow (databaseOptions: IOptions<Database>) =
    member __.Execute() =
        async {
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            let! tags = ReadModelQueries.getTags context
            return tags |> QueryModels.toTagsModelEnvelope
        }