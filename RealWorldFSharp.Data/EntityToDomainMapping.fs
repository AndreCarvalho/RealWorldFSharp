namespace RealWorldFSharp.Data

open System
open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

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
        
    let mapUserFollowing (userFollowing:UserFollowingEntity) : UserFollowing =
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
            let! body = Body.create "body" articleEntity.Body
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
                UserId = userId
                CreatedAt = createdAt
                UpdatedAt = updatedAt
            }
        }
        
    let mapArticle article =
        validateArticle article |> valueOrException