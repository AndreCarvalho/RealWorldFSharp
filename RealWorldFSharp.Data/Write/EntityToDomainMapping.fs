namespace RealWorldFSharp.Data.Write

open System
open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Domain.Articles

module EntityToDomainMapping =
        
    let private validateApplicationUser =
        fun (applicationUser: ApplicationUser) ->
            result {
                let! userName = Username.create "username" applicationUser.UserName
                let! emailAddress = EmailAddress.create "email" applicationUser.Email
                let! userId = UserId.create "id" applicationUser.Id
                
                return {
                    Username = userName 
                    EmailAddress = emailAddress
                    Id = userId
                    Bio = Option.ofObj applicationUser.Bio
                    Image = Option.ofObj applicationUser.ImageUrl
                }
            }
    
    let mapApplicationUserToUserInfo applicationUser =
        validateApplicationUser applicationUser |> valueOrException
        
    let mapUserFollowing (userFollowing:UserFollowingsEntity) : UserFollowing =
        let userId = UserId.create "userid" userFollowing.Id |> valueOrException
        let following =
            userFollowing.Following
            |> List.map (UserId.create "userId")
            |> List.map valueOrException
            |> Set.ofList
        
        {
            Id = userId
            Following = following
        }
        
    let validateArticle (articleEntity:ArticleEntity) =
        result {
            let id = Guid.Parse articleEntity.Id
            let! title = Title.create "title" articleEntity.Title
            let slug = Slug.create articleEntity.Slug
            let! description = Description.create "description" articleEntity.Description
            let! body = ArticleBody.create "body" articleEntity.Body
            let! tags = articleEntity.Tags
                       |> Option.ofObj
                       |> Option.map List.ofSeq
                       |> Option.defaultValue []
                       |> List.map (fun y -> Tag.create "tag" y.Tag)
                       |> List.sequenceResultM
            
            let! userId = UserId.create "userid" articleEntity.UserId
            let createdAt = articleEntity.CreatedAt
            let updatedAt = articleEntity.UpdatedAt
            
            return {
                Id = id
                Title = title
                Slug = slug
                Description = description
                Body = body
                Tags = tags
                AuthorUserId = userId
                CreatedAt = createdAt
                UpdatedAt = updatedAt
            }
        }
        
    let mapArticle article =
        validateArticle article |> valueOrException
        
    let validateComment (commentEntity:ArticleCommentEntity) =
        result {
            let id = commentEntity.Id |> Guid.Parse
            let! body = CommentBody.create "body" commentEntity.Body
            let articleId = commentEntity.ArticleId |> Guid.Parse
            let! authorUserId = UserId.create "userid" commentEntity.UserId
            let createdAt = commentEntity.CreatedAt
            let updatedAt = commentEntity.UpdatedAt
            
            return {
                Id = id
                Body = body
                ArticleId = articleId
                AuthorUserId = authorUserId
                CreatedAt = createdAt
                UpdatedAt = updatedAt
            }
        }
        
    let mapCommentEntity entity = entity |> validateComment |> valueOrException
        
    let mapFavoriteArticles (entity:FavoriteArticlesEntity) : FavoriteArticles =
        let userId = UserId.create "userid" entity.UserId |> valueOrException
        let favorites =
            entity.Favorites
            |> List.map Guid.Parse
            |> Set.ofList
        {
            Id = userId
            Favorites = favorites
        }