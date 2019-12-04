namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels

type GetTagsWorkflow (readDataContext: ReadDataContext) =
    member __.Execute() =
        async {
            let! tags = ReadQueries.getTags readDataContext
            return tags |> QueryModels.toTagsModelEnvelope
        }