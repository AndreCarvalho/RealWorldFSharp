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
                            addCommentWorkflow: AddCommentWorkflow,
                            getCommentsWorkflow: GetCommentsWorkflow
                        ) =
    inherit Controller()
    
    [<HttpPost>]
    [<Route("")>]
    member self.AddComment(articleSlug: string, commandModelEnvelope: AddCommentToArticleCommandModelEnvelope) =
        let userId = base.HttpContext |> Http.getUserId
        
        async {
            let! result = addCommentWorkflow.Execute(userId, articleSlug, commandModelEnvelope.Comment)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
        
    [<HttpGet>]
    [<AllowAnonymous>]
    [<Route("")>]
    member self.GetComments(articleSlug: string) =
        let userIdOption = base.HttpContext |> Http.getUserIdOption
        
        async {
            let! result = getCommentsWorkflow.Execute(userIdOption, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask