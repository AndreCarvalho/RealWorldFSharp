namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open RealWorldFSharp.Api.Workflows.AuthenticateUser
open RealWorldFSharp.Api.Http
open FsToolkit.ErrorHandling
open RealWorldFSharp.CommandModels


[<ApiController>]
[<Route("api/users")>]
type UserAuthenticationController (authenticateUserWorkflow: AuthenticateUserWorkflow) =
    inherit Controller()

    [<HttpPost>]
    [<Route("login")>]
    member x.Post(payload: AuthenticateUserCommandModelEnvelope) =
        async {
            let! result = authenticateUserWorkflow.Execute payload.User
            return result |> resultToActionResult x
        } |> Async.StartAsTask