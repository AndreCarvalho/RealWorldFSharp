namespace RealWorldFSharp.Data

open DataEntities
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Domain

module DomainToEntityMapping =
    
    let mapUserInfoToApplicationUser (userInfo:UserInfo) =
        let x = new ApplicationUser ()
        x.Id <- userInfo.Id.Value
        x.UserName <- userInfo.Username.Value
        x.Email <- userInfo.EmailAddress.Value
        x.Bio <- Option.defaultValue null userInfo.Bio
        x.ImageUrl <- Option.defaultValue null userInfo.Image
        x
        
    let mapArticleToEntity (article:Article): ArticleEntity =
        let tags = article.Tags
                   |> List.map (fun tag -> new ArticleTagEntity(
                                                                   Tag = tag.Value,
                                                                   ArticleId = article.Id.ToString()
                                                                   //Articles = null
                                                               ))
                   |> Array.ofList
                   
        new ArticleEntity(
            Id = article.Id.ToString(),
            Title = article.Title.Value,
            Slug = article.Slug.Value,
            Description = article.Description.Value,
            Body = article.Body.Value,
            UserId = article.UserId.Value,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Tags = tags
            )
        
    let mapTagToEntity articleId (tag:Tag): ArticleTagEntity =
        ArticleTagEntity(ArticleId = articleId, Tag = tag.Value)
