namespace Api.Controllers

open Microsoft.AspNetCore.Mvc
open Api.Models.Request
open Api.Workflows.AuthenticateUser
open Api.Errors
open Api.ModelsMapping
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
            
            return
                match result with
                | Ok user -> x.Ok(user) :> IActionResult
                | Error error ->
                    match error with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | ValidationError er -> x.BadRequest(mapValidationError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
        } |> Async.StartAsTask