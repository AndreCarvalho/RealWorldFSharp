namespace RealWorldFSharp.Api.Workflows

open System
open FsToolkit.ErrorHandling
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Domain

type DeleteCommentWorkflow (
                               dbContext: ApplicationDbContext
                           ) =
    member __.Execute(userId, articleSlug, commentId:string) =
        asyncResult {
            let userId = UserId.create "userId" userId |> valueOrException
            
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! _ = noneToError articleOption slug.Value |> expectDataRelatedError

            let commentId = commentId |> Guid.Parse
            let! commentOption = DataPipeline.getComment dbContext commentId
            let! comment = noneToError commentOption (commentId.ToString()) |> expectDataRelatedError

            let! comment = validateDeleteComment comment userId |> expectOperationNotAllowedError
            
            do! comment |> DataPipeline.deleteComment dbContext |> expectDataRelatedErrorAsync
            do! dbContext.SaveChangesAsync()
            
            return ()
        }