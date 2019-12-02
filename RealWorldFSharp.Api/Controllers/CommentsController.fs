namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows
open RealWorldFSharp.CommandModels

[<Authorize>]
[<ApiController>]
[<Route("api/articles/{articleSlug}/comments")>]
type CommentsController (
                            addCommentWorkflow: AddCommentWorkflow
                        ) =
    inherit Controller()
    
    [<HttpPost>]
    [<Route("")>]
    member self.AddComment(articleSlug: string, commandModelEnvelope: AddCommentToArticleCommandModelEnvelope) =
        let username = base.HttpContext |> Http.getUserName
        
        async {
            let! result = addCommentWorkflow.Execute(username, articleSlug, commandModelEnvelope.Comment)
            return result |> resultToActionResult self
        } |> Async.StartAsTask