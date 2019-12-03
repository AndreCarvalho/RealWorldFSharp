namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Api
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

type UpdateArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
    member __.Execute(articleSlug, command: UpdateArticleCommandModel) =
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! cmd = validateUpdateArticleCommand command |> expectValidationError
            let now = DateTimeOffset.UtcNow
            let article = article |> updateArticle cmd.Title cmd.Body cmd.Description now
            
            do! DataPipeline.updateArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            let! userInfoOption = DataPipeline.getUserInfoById userManager article.AuthorUserId 
            let! (userInfo, _) = noneToError userInfoOption article.AuthorUserId.Value |> expectDataRelatedError
            
            return article |> QueryModels.toSingleArticleEnvelope (userInfo |> QueryModels.toSimpleProfileModel)
        }