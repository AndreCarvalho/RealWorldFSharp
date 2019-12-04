namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api.Workflows

[<ApiController>]
[<Route("api/tags")>]
type TagsController (getTagsWorkflow: GetTagsWorkflow) =
    inherit Controller()

    [<HttpGet>]
    [<Route("")>]
    member self.GetTags() =
        async {
            let! result = getTagsWorkflow.Execute()
            return result |> self.Ok
        } |> Async.StartAsTask