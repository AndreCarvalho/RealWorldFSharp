namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.Read.ReadModels
open RealWorldFSharp.Domain.Users

type RetrieveProfileWorkflow(
                                userManager: UserManager<ApplicationUser>,
                                readDataContext: ReadDataContext
                            ) =
    member __.Execute(currentUserIdOption, profileUserName) =
        
        asyncResult {
            let! profileUsername = Username.create "username" profileUserName |> expectValidationError
            let! profileUserInfoOption = DataPipeline.getUserInfoByUsername userManager profileUsername 
            let! profileUserInfo = noneToUserNotFoundError profileUserInfoOption profileUsername.Value |> expectUsersError
            
            let! profileReadModel = ReadModelQueries.getUserProfileReadModel readDataContext profileUserInfo.Id.Value currentUserIdOption
            return QueryModels.toProfileModelReadModelEnvelope profileReadModel
        }