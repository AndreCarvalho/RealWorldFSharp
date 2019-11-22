namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open RealWorldFSharp.Api.Models.Request
open RealWorldFSharp.Api.Workflows.AuthenticateUser
open RealWorldFSharp.Api.Http
open FsToolkit.ErrorHandling


[<ApiController>]
[<Route("api/users")>]
type UserAuthenticationController (authenticateUserWorkflow: AuthenticateUserWorkflow) =
    inherit Controller()

    [<HttpPost>]
    [<Route("login")>]
    member x.Post(payload: AuthenticateUser) =
        async {
            let! result = authenticateUserWorkflow.Execute payload
            return result |> resultToActionResult x
        } |> Async.StartAsTask