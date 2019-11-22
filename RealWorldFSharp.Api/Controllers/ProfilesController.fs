namespace RealWorldFSharp.Api.Controllers

open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows.FollowUser
open RealWorldFSharp.Api.Workflows.RetrieveProfile
open RealWorldFSharp.Api.Workflows.UnfollowUser

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
        let currentUserName = base.HttpContext.User.Claims
                                |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                                |> Option.map (fun x -> x.Value)
                                
        async {
            let! result = retrieveProfileWorkflow.Execute(currentUserName, userName)
            return result |> resultToActionResult x
        } |> Async.StartAsTask
        
    [<HttpPost>]
    [<Route("{userNameToFollow}/follow")>]
    member x.Follow(userNameToFollow: string) =
        let currentUserName = base.HttpContext.User.Claims
                                |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                                |> Option.map (fun x -> x.Value)
                                |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = followUserWorkflow.Execute(currentUserName, userNameToFollow)
            return result |> resultToActionResult x
        } |> Async.StartAsTask
        
    [<HttpDelete>]
    [<Route("{userNameToUnfollow}/follow")>]
    member x.Unfollow(userNameToUnfollow: string) =
        let currentUserName = base.HttpContext.User.Claims
                                |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                                |> Option.map (fun x -> x.Value)
                                |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = unfollowUserWorkflow.Execute(currentUserName, userNameToUnfollow)
            return result |> resultToActionResult x
        } |> Async.StartAsTask