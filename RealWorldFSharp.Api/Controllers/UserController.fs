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
[<Route("api/user")>]
type UserController(
                       retrieveUserWorkflow: RetrieveUserWorkflow,
                       updateUserWorkflow: UpdateUserWorkflow
                   ) =
    inherit Controller()

    [<HttpGet>]
    [<Route("")>]
    member self.Get() =
        let userName = base.HttpContext |> Http.getUserName
                        
        async {
            let! result = retrieveUserWorkflow.Execute userName
            return result |> resultToActionResult self
        } |> Async.StartAsTask
        
    [<HttpPut>]
    [<Route("")>]
    member self.Put(updateUser: UpdateUserCommandModelEnvelope) =
        let userId = base.HttpContext |> Http.getUserId
                        
        async {
            let! result = updateUserWorkflow.Execute(userId, updateUser.User)
            return result |> resultToActionResult self
        } |> Async.StartAsTask
