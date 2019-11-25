﻿namespace RealWorldFSharp.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Api.Workflows.RegisterNewUser

[<ApiController>]
[<Route("api/users")>]
type UserRegistrationController (registerNewUserWorkflow: RegisterNewUserWorkflow) =
    inherit Controller()

    [<HttpPost>]
    [<Route("")>]
    member x.Post(payload: RegisterNewUserCommandModel) =
        async {
            let! result = registerNewUserWorkflow.Execute payload
            return result |> Http.resultToActionResult x
        } |> Async.StartAsTask
        

