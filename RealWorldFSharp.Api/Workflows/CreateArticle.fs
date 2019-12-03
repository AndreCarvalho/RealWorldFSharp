namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Domain.Articles

type CreateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
    member __.Execute(username, command: CreateArticleCommandModel) =
        asyncResult {
            let currentUsername = Username.create "username" username |> valueOrException
            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername 
            let! (userInfo, _) = noneToUserNotFoundError userInfoOption currentUsername.Value |> expectUsersError
            
            let! cmd = validateCreateArticleCommand command |> expectValidationError
            
            let now = DateTimeOffset.UtcNow
            let article = createArticle userInfo.Id cmd.Title cmd.Description cmd.Body cmd.Tags now

            do! DataPipeline.addArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            return article |> QueryModels.toSingleArticleEnvelope (userInfo |> QueryModels.toSimpleProfileModel)
        }