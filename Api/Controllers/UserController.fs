namespace Api.Controllers

open Api.Errors
open Api.Models.Request
open System.Security.Claims
open Microsoft.AspNetCore.Mvc
open Api.Workflows.RetrieveUser
open Api.Workflows.UpdateUser
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Authorization
open Api.ModelsMapping

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
            
            return
                match result with
                | Ok user -> (x.Ok user) :> IActionResult
                | Error _ -> failwith "unexpected error at this level"
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
            
            return
                match result with
                | Ok user -> (x.Ok user) :> IActionResult
                | Error error ->
                    match error with
                    | UsersError er -> x.BadRequest(mapUsersError er) :> IActionResult
                    | ValidationError er -> x.BadRequest(mapValidationError er) :> IActionResult
                    | _ -> failwith "unexpected error case"
        } |> Async.StartAsTask
