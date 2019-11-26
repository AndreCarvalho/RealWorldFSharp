namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Api.DataAccess
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Domain

module RetrieveProfile =
    
    type RetrieveProfileWorkflow(
                                    dbContext: ApplicationDbContext,
                                    userManager: UserManager<ApplicationUser>
                                ) =
        member __.Execute(currentUserNameOption, profileUserName) =
            asyncResult {
                let! username = Username.create "username" profileUserName |> expectValidationError
                let! userInfoOption = DataPipeline.getUserInfo userManager username 
                let! (userInfo, _) = noneToUserNotFoundError userInfoOption username.Value |> expectUsersError
                return userInfo |> toProfileModelEnvelope
            }
