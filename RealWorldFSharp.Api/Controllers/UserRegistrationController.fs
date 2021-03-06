﻿namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api.Workflows

[<ApiController>]
[<Route("api/users")>]
type UserRegistrationController (registerNewUserWorkflow: RegisterNewUserWorkflow) =
    inherit Controller()

    [<HttpPost>]
    [<Route("")>]
    member x.Post(payload: RegisterNewUserCommandModelEnvelope) =
        async {
            let! result = registerNewUserWorkflow.Execute payload.User
            return result |> Http.resultToActionResult x
        } |> Async.StartAsTask
        

