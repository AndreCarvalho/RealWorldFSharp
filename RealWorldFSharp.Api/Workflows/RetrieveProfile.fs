namespace RealWorldFSharp.Api.Workflows

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels
open RealWorldFSharp.Domain.Users

type RetrieveProfileWorkflow(
                                dbContext: ApplicationDbContext,
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