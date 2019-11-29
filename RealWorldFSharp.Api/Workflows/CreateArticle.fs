namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open RealWorldFSharp
open RealWorldFSharp.Articles
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Domain

module CreateArticle =
    
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
                let article = Domain.createArticle userInfo.Id cmd.Title cmd.Description cmd.Body cmd.Tags now

                do! DataPipeline.addArticle dbContext article |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
                
                return article |> QueryModels.toSingleArticleEnvelope userInfo
            }

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
                let article = article |> Domain.updateArticle cmd.Title cmd.Body cmd.Description now
                
                do! DataPipeline.updateArticle dbContext article |> expectDataRelatedErrorAsync
                do! dbContext.SaveChangesAsync()
                
                let! userInfoOption = DataPipeline.getUserInfoById userManager article.UserId 
                let! (userInfo, _) = noneToError userInfoOption article.UserId.Value |> expectDataRelatedError
                
                return article |> QueryModels.toSingleArticleEnvelope userInfo
            }
