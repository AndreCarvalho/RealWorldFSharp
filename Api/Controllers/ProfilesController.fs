namespace Api.Controllers

open System.Security.Claims
open Api.Errors
open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open Api.ModelsMapping
open Api.Workflows.FollowUser
open Api.Workflows.RetrieveProfile
open Api.Workflows.UnfollowUser

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
            
            return
                match result with
                | Ok profile -> (x.Ok profile) :> IActionResult
                | Error err ->
                    match err with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
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
            
            return
                match result with
                | Ok profile -> (x.Ok profile) :> IActionResult
                | Error err ->
                    match err with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
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
            
            return
                match result with
                | Ok profile -> (x.Ok profile) :> IActionResult
                | Error err ->
                    match err with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
        } |> Async.StartAsTask