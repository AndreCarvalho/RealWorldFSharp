namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels

type GetCommentsWorkflow (
                               dbContext: ApplicationDbContext,
                               userManager: UserManager<ApplicationUser>,
                               readDataContext: ReadDataContext
                           ) =
    member __.Execute(userIdOption, articleSlug) =
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! comments = ReadModelQueries.getCommentsForArticle readDataContext (article.Id.ToString())
            
            return!
                match userIdOption with
                | Some userId ->
                    asyncResult {
                        let userId = userId |> (UserId.create "userId") |> valueOrException
                        let! userFollowing = userId |> Helper.getUserFollowing dbContext userManager
                        return comments |> QueryModels.toCommentsModelEnvelope (Some userFollowing)
                    }
                | None ->
                    comments |> QueryModels.toCommentsModelEnvelope None |> AsyncResult.retn
        }

