namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write

type AddCommentWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
    member __.Execute(userId, articleSlug, command: AddCommentToArticleCommandModel) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! userOption = DataPipeline.getUserInfoById userManager userId
            let! userInfo = noneToUserNotFoundError userOption article.AuthorUserId.Value |> expectUsersError
            
            let! cmd = validateAddCommentToArticleCommand command |> expectValidationError
            let comment = createComment cmd.Body article userId DateTimeOffset.UtcNow
            
            do! comment |> DataPipeline.addComment dbContext |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            return comment |> QueryModels.toCommentModelEnvelope (userInfo |> QueryModels.toSimpleProfileModel)
        }