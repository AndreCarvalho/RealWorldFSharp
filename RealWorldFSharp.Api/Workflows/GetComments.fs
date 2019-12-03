namespace RealWorldFSharp.Api.Workflows

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Data.DataEntities
open RealWorldFSharp.Common
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
        let getUserInfo userId =
            asyncResult {
                let! userOption = DataPipeline.getUserInfoById userManager userId
                return! noneToUserNotFoundError userOption userId.Value |> expectUsersError
            }
            
        asyncResult {
            let slug = Slug.create articleSlug
            let! articleOption = DataPipeline.getArticle dbContext slug
            let! article = noneToError articleOption slug.Value |> expectDataRelatedError
            
            let! comments = ReadQueries.getCommentsForArticle readDataContext (article.Id.ToString())
            
            return {
                Comments = comments
                           |> Seq.map (fun c -> {
                               Id = c.Id
                               Body = c.Body
                               CreatedAt = c.CreatedAt
                               UpdatedAt = c.UpdatedAt
                               Author = {
                                   Username = c.User.Username
                                   Bio = c.User.Bio
                                   Image = c.User.ImageUrl
                                   Following = Nullable.from(false) // TODO
                               }
                           })
                           |> Array.ofSeq
            }
            
            // query - command impedance mismatch... a better option would be to use CQRS with read and write models
//            let! comments = DataPipeline.getArticleComments dbContext article.Id |> expectDataRelatedErrorAsync
//            let userIds = comments |> Seq.map (fun c -> c.AuthorUserId) |> Seq.distinct
//            let! users = userIds |> Seq.map getUserInfo |> Async.Sequential
//            let! users = users |> List.ofArray |> Result.combine
//            
//            // complexity... and it's missing the auth version TODO
//            return comments
//                    |> List.ofSeq
//                    |> List.map (fun c -> (c, users
//                                              |> (List.find (fun (u, _) -> u.Id = c.AuthorUserId))
//                                              |> fst
//                                              |> QueryModels.toSimpleProfileModel))
//                    |> QueryModels.toCommentsModelEnvelope
        }

