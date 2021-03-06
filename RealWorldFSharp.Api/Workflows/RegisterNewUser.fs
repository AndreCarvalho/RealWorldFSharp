namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write.DataEntities

type RegisterNewUserWorkflow(
                                userManager: UserManager<ApplicationUser>,
                                jwtOption: IOptions<JwtConfiguration>
                            ) =
    member __.Execute(command: RegisterNewUserCommandModel) =
        asyncResult {
            let userId = Guid.NewGuid().ToString()              
            let! (user, password) = validateRegisterNewUserCommand userId command |> expectValidationError
            do! DataPipeline.registerNewUser userManager (user, password) |> expectUsersErrorAsync
            let token = user |> Authentication.createToken jwtOption.Value
            return user |> toUserModelEnvelope token
        }