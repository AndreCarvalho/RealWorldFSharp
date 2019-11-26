namespace RealWorldFSharp.Api.Controllers

open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open RealWorldFSharp.Api.Http
open RealWorldFSharp.Api.Workflows.RetrieveUser
open RealWorldFSharp.Api.Workflows.UpdateUser
open RealWorldFSharp.CommandModels

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
    member self.Get() =
        let userName = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = retrieveUserWorkflow.Execute userName
            
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpPut>]
    [<Route("")>]
    member self.Put(updateUser: UpdateUserCommandModelEnvelope) =
        let userName = base.HttpContext.User.Claims
                        |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Name)
                        |> Option.map (fun x -> x.Value)
                        |> Option.defaultValue "_NOT_A_USER_"
                        
        async {
            let! result = updateUserWorkflow.Execute(userName, updateUser.User)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
