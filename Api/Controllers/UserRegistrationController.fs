namespace Api.Controllers

open Microsoft.AspNetCore.Mvc
open Api.Models.Request
open Api.Workflows.RegisterNewUser
open FsToolkit.ErrorHandling
open Api.Errors
open Api.ModelsMapping

[<ApiController>]
[<Route("api/users")>]
type UserRegistrationController (registerNewUserWorkflow: RegisterNewUserWorkflow) =
    inherit Controller()

    [<HttpPost>]
    [<Route("")>]
    member x.Post(payload: RegisterNewUser) =
        async {
            let! result = registerNewUserWorkflow.Execute payload
            
            return
                match result with
                | Ok user -> x.Ok(user) :> IActionResult
                | Error error ->
                    match error with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | ValidationError er -> x.BadRequest(mapValidationError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
        } |> Async.StartAsTask
        

