namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModels

type GetTagsWorkflow (readDataContext: ReadDataContext) =
    member __.Execute() =
        async {
            let! tags = ReadModelQueries.getTags readDataContext
            return tags |> QueryModels.toTagsModelEnvelope
        }