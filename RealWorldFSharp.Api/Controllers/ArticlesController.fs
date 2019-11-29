namespace RealWorldFSharp.Api.Controllers

open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows.CreateArticle
open RealWorldFSharp.Api.Workflows.GetArticle
open RealWorldFSharp.CommandModels


[<Authorize>]
[<ApiController>]
[<Route("api/articles")>]
type ArticlesController (
                            createArticleWorkflow:CreateArticleWorkflow,
                            getArticleWorkflow: GetArticleWorkflow
                        ) =
    inherit Controller()
    
    [<HttpPost>]
    [<Route("")>]
    member self.CreateArticle(createArticle: CreateArticleCommandModelEnvelope) =
        let username = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = createArticleWorkflow.Execute(username, createArticle.Article)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpGet>]
    [<AllowAnonymous>]
    [<Route("{articleSlug}")>]
    member self.GetArticle(articleSlug: string) =
        let username = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = getArticleWorkflow.Execute(username, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
    
    