namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.Extensions.Options
open RealWorldFSharp.Api
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Domain.Users

type RetrieveProfileWorkflow(databaseOptions: IOptions<Database>) =
    member __.Execute(currentUserIdOption, profileUserName) =
        
        asyncResult {
            let! profileUsername = Username.create "username" profileUserName |> expectValidationError
            let context = ReadModelQueries.getDataContext databaseOptions.Value.ConnectionString
            
            let! profileReadModelOption = ReadModelQueries.getUserProfileReadModel context currentUserIdOption profileUsername.Value
            let! profileReadModel = noneToUserNotFoundError profileReadModelOption profileUsername.Value |> expectUsersError
            return QueryModels.toProfileModelReadModelEnvelope profileReadModel
        }