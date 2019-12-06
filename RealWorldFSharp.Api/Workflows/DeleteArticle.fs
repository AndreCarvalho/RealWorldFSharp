namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.Write.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data.Write

type DeleteArticleWorkflow (
                               dbContext: ApplicationDbContext
                           ) =
    member __.Execute(userId, articleSlug) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException 
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! article = validateArticleOwner "delete article" article userId |> expectOperationNotAllowedError
            
            do! DataPipeline.deleteArticle dbContext article |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
        }