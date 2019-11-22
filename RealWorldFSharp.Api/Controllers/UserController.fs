namespace RealWorldFSharp.Api.Controllers

open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Models.Request
open RealWorldFSharp.Api.Workflows.RetrieveUser
open RealWorldFSharp.Api.Workflows.UpdateUser

[<Authorize>]
[<ApiController>]
[<Route("api/user")>]
type UserController(
                       retrieveUserWorkflow: RetrieveUserWorkflow,
                       updateUserWorkflow: UpdateUserWorkflow
                   ) =
    inherit Controller()

    [<HttpGet>]
    [<Route("")>]
    member x.Get() =
        let userName = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = retrieveUserWorkflow.Execute userName
            
            return result |> resultToActionResult x
        } |> Async.StartAsTask
        
    [<HttpPut>]
    [<Route("")>]
    member x.Put(updateUser: UpdateUser) =
        let userName = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = updateUserWorkflow.Execute(userName, updateUser.User)
            return result |> resultToActionResult x
        } |> Async.StartAsTask
