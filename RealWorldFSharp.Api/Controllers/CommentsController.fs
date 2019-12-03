namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows
open RealWorldFSharp.Api.CommandModels

[<Authorize>]
[<ApiController>]
[<Route("api/articles/{articleSlug}/comments")>]
type CommentsController (
                            addCommentWorkflow: AddCommentWorkflow,
                            getCommentsWorkflow: GetCommentsWorkflow,
                            deleteCommentWorkflow: DeleteCommentWorkflow
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
        
        
    [<HttpDelete>]
    [<Route("{commentId}")>]
    member self.DeleteComment(articleSlug: string, commentId: string) =
        let userId = base.HttpContext |> Http.getUserId
        
        async {
            let! result = deleteCommentWorkflow.Execute(userId, articleSlug, commentId)
            return result |> resultToActionResult self
        } |> Async.StartAsTask