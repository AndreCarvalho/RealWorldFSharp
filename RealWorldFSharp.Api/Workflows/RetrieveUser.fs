namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open FsToolkit.ErrorHandling
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

module RetrieveUser =
    
    type RetrieveUserWorkflow (
                                    userManager: UserManager<ApplicationUser>,
                                    jwtOptions: IOptions<JwtConfiguration>
                              ) =
        member __.Execute(username) =           
            asyncResult {
                let! username = Username.create "username" username |> expectValidationError
                let! userInfoOption = DataPipeline.getUserInfo userManager username 
                let! (userInfo, _) = noneToUserNotFoundError userInfoOption username.Value |> expectUsersError
                let token = userInfo |> Authentication.createToken jwtOptions.Value
                return userInfo |> toUserModelEnvelope token
            }