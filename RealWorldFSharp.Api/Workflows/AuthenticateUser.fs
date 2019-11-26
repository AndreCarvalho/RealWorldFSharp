namespace RealWorldFSharp.Api.Workflows

open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.QueryModels
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities

module AuthenticateUser =
    type AuthenticateUserWorkflow(
                                    userManager: UserManager<ApplicationUser>,
                                    jwtOptions: IOptions<JwtConfiguration>
                                ) =
        member __.Execute(command: AuthenticateUserCommandModel) =
            asyncResult {
                let! authCommand = validateAuthenticateUserCommand command |> expectValidationError
                let! userInfo = DataPipeline.authenticateUser userManager (authCommand.EmailAddress, authCommand.Password)
                                |> expectUsersErrorAsync
                let token = Authentication.createToken jwtOptions.Value userInfo
                return userInfo |> toUserModelEnvelope token
            }