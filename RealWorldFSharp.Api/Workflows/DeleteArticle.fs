namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data

type DeleteArticleWorkflow (
                               dbContext: ApplicationDbContext
                           ) =
    member __.Execute(userId, articleSlug) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException 
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! article = validateDeleteArticle article userId |> expectOperationNotAllowedError
            
            do! DataPipeline.deleteArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
        }