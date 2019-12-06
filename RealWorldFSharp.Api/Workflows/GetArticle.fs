namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open RealWorldFSharp.Api
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Data
open RealWorldFSharp.Data.Read
open RealWorldFSharp.Data.ReadModels

type GetArticleWorkflow (
                           dbContext: ApplicationDbContext,
                           readDataContext: ReadDataContext
                        ) =
    member __.Execute(userIdOption, articleSlug) =           
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! articleQuery =
                match userIdOption with
                | Some userId -> ReadModelQueries.getArticleForUser readDataContext userId (article.Id.ToString())
                | None -> ReadModelQueries.getArticle readDataContext (article.Id.ToString())
                    
            return articleQuery |> QueryModels.toSingleArticleEnvelopeReadModel
        }