namespace RealWorldFSharp.Api.Controllers


open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows

[<Authorize>]
[<ApiController>]
[<Route("api/articles/{articleSlug}")>]
type FavoriteArticlesController (
                                    favoriteArticleWorkflow: FavoriteArticleWorkflow,
                                    unfavoriteArticleWorkflow: UnfavoriteArticleWorkflow
                                ) =
    inherit Controller()
    
    [<HttpPost>]
    [<Route("favorite")>]
    member self.FavoriteArticle(articleSlug: string) =
        let userId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = favoriteArticleWorkflow.Execute(userId, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
    
    [<HttpDelete>]
    [<Route("favorite")>]
    member self.UnfavoriteArticle(articleSlug: string) =
        let userId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = unfavoriteArticleWorkflow.Execute(userId, articleSlug)
            return result |> resultToActionResult self
        } |> Async.StartAsTask

