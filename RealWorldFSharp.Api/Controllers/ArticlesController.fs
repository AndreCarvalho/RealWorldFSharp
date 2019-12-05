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
[<Route("api/articles")>]
type ArticlesController (
                            createArticleWorkflow:CreateArticleWorkflow,
                            getArticleWorkflow: GetArticleWorkflow,
                            updateArticleWorkflow: UpdateArticleWorkflow,
                            deleteArticleWorkflow: DeleteArticleWorkflow
                        ) =
    inherit Controller()
    
    [<HttpPost>]
    [<Route("")>]
    member self.CreateArticle(createArticle: CreateArticleCommandModelEnvelope) =
        let userId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = createArticleWorkflow.Execute(userId, createArticle.Article)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpPut>]
    [<Route("{articleSlug}")>]
    member self.UpdateArticle(articleSlug: string, updateArticle: UpdateArticleCommandModelEnvelope) =
        let currentUserId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = updateArticleWorkflow.Execute(currentUserId, articleSlug, updateArticle.Article)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpDelete>]
    [<Route("{articleSlug}")>]
    member self.DeleteArticle(articleSlug: string) =
        let currentUserId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = deleteArticleWorkflow.Execute(currentUserId, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpGet>]
    [<AllowAnonymous>]
    [<Route("{articleSlug}")>]
    member self.GetArticle(articleSlug: string) =
        let userId = base.HttpContext |> Http.getUserIdOption
                        
        async {
            let! result = getArticleWorkflow.Execute(userId, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
    
    