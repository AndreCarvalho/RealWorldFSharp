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
        let username = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = createArticleWorkflow.Execute(username, createArticle.Article)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpPut>]
    [<Route("{articleSlug}")>]
    member self.UpdateArticle(articleSlug: string, updateArticle: UpdateArticleCommandModelEnvelope) =
        let username = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = updateArticleWorkflow.Execute(articleSlug, updateArticle.Article)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpDelete>]
    [<Route("{articleSlug}")>]
    member self.DeleteArticle(articleSlug: string) =
        let username = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = deleteArticleWorkflow.Execute(username, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpGet>]
    [<AllowAnonymous>]
    [<Route("{articleSlug}")>]
    member self.GetArticle(articleSlug: string) =
        let username = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = getArticleWorkflow.Execute(username, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
    
    