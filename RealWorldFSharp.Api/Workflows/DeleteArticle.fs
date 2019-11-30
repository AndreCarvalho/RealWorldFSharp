namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

type DeleteArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                           ) =
    member __.Execute(username, articleSlug) =
        asyncResult {
            let currentUsername = Username.create "username" username |> valueOrException
            let! userInfoOption = DataPipeline.getUserInfoByUsername userManager currentUsername 
            let! (userInfo, _) = noneToUserNotFoundError userInfoOption currentUsername.Value |> expectUsersError

            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! article = validateDeleteArticle article userInfo |> expectOperationNotAllowedError
            
            do! DataPipeline.deleteArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
        }