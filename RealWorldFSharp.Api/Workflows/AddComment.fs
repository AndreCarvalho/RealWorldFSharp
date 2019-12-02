namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Domain

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
            
            let! authorUserOption = DataPipeline.getUserInfoById userManager article.AuthorUserId
            let! (authorUserInfo, _) = noneToUserNotFoundError authorUserOption article.AuthorUserId.Value |> expectUsersError
            
            let! cmd = validateAddCommentToArticleCommand command |> expectValidationError
            let comment = createComment cmd.Body article userId DateTimeOffset.UtcNow
            
            let! userFollowing = userId |> Helper.getUserFollowing dbContext userManager
            
            do! comment |> DataPipeline.addComment dbContext |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            return comment |> QueryModels.toCommentModelEnvelope (authorUserInfo |> QueryModels.toProfileModel userFollowing)
        }