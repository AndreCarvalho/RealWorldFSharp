namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows

[<Authorize>]
[<ApiController>]
[<Route("api/profiles")>]
type ProfilesController(
                           retrieveProfileWorkflow: RetrieveProfileWorkflow,
                           followUserWorkflow: FollowUserWorkflow,
                           unfollowUserWorkflow: UnfollowUserWorkflow
                       ) =
    inherit Controller()
    
    [<HttpGet>]
    [<AllowAnonymous>]
    [<Route("{userName}")>]
    member x.Get(userName: string) =
        let currentUserName = base.HttpContext |> Http.getUserNameOption
                                
        async {
            let! result = retrieveProfileWorkflow.Execute(currentUserName, userName)
            return result |> resultToActionResult x
        } |> Async.StartAsTask
        
    [<HttpPost>]
    [<Route("{userNameToFollow}/follow")>]
    member x.Follow(userNameToFollow: string) =
        let currentUserName = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = followUserWorkflow.Execute(currentUserName, userNameToFollow)
            return result |> resultToActionResult x
        } |> Async.StartAsTask
        
    [<HttpDelete>]
    [<Route("{userNameToUnfollow}/follow")>]
    member x.Unfollow(userNameToUnfollow: string) =
        let currentUserName = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = unfollowUserWorkflow.Execute(currentUserName, userNameToUnfollow)
            return result |> resultToActionResult x
        } |> Async.StartAsTask