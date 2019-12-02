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
    member __.Execute(username, articleSlug, command: AddCommentToArticleCommandModel) =
        asyncResult {
            let currentUsername = Username.create "username" username |> valueOrException
            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername 
            let! (userInfo, _) = noneToUserNotFoundError userInfoOption currentUsername.Value |> expectUsersError
            
            // TODO: store userId in token??
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! authorUserOption = DataPipeline.getUserInfoById userManager article.AuthorUserId
            let! (authorUserInfo, _) = noneToUserNotFoundError authorUserOption article.AuthorUserId.Value |> expectUsersError
            
            let! cmd = validateAddCommentToArticleCommand command |> expectValidationError
            let comment = createComment cmd.Body article userInfo DateTimeOffset.UtcNow
            
            let! userFollowing = username |> Helper.getUserFollowing dbContext userManager
            
            do! comment |> DataPipeline.addComment dbContext |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            return comment |> QueryModels.toCommentModelEnvelope (authorUserInfo |> QueryModels.toProfileModel userFollowing)
        }