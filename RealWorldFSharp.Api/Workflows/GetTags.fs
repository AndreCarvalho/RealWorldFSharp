namespace RealWorldFSharp.Api.Workflows

open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Data.Read

type GetTagsWorkflow (databaseOptions: IOptions<Database>) =
    member __.Execute() =
        async {
            let! tags = ReadModelQueries.getTags databaseOptions.Value.ConnectionString
            return tags |> QueryModels.toTagsModelEnvelope
        }