namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open FsToolkit.ErrorHandling
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Domain.Users



type RetrieveUserWorkflow (
                                userManager: UserManager<ApplicationUser>,
                                jwtOptions: IOptions<JwtConfiguration>
                          ) =
    member __.Execute(username) =           
        asyncResult {
            let username = Username.create "username" username |> valueOrException
            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager username 
            let! userInfo = noneToUserNotFoundError userInfoOption username.Value |> expectUsersError
            let token = userInfo |> Authentication.createToken jwtOptions.Value
            return userInfo |> toUserModelEnvelope token
        }