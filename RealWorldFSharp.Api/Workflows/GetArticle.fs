namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

module GetArticle =
    
    type GetArticleWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>
                            ) =
        member __.Execute(username, articleSlug) = // TODO: handle login case
            asyncResult {
                let slug = Slug.create articleSlug
                let! articleOption = DataPipeline.getArticle dbContext slug
                let! article = noneToError articleOption slug.Value |> expectDataRelatedError
                
                let! userInfoOption = DataPipeline.getUserInfoById userManager article.UserId 
                let! (userInfo, _) = noneToError userInfoOption article.UserId.Value |> expectDataRelatedError

                return article |> QueryModels.toSingleArticleEnvelope userInfo
            }
