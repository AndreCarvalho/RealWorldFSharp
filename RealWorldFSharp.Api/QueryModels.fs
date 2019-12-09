namespace RealWorldFSharp.Api

open System
open System.Collections.Generic
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Common
open RealWorldFSharp.Data.Read.ReadModelQueries
open RealWorldFSharp.Data.Read.ReadModels

module QueryModels =
    
    [<CLIMutable>]
    type Errors = {
        Errors: Dictionary<string, string array>
    }
    
    [<CLIMutable>]
    type UserModel = {
        Username: string
        Email: string
        Token: string
        Bio: string
        Image: string
    }
    
    [<CLIMutable>]
    type UserModelEnvelope = {
        User: UserModel
    }
    
    [<CLIMutable>]
    type ProfileModel = {
        Username: string
        Bio: string
        Image: string
        Following: bool
    }
    
    [<CLIMutable>]
    type ProfileModelEnvelope = {
        Profile: ProfileModel
    }
    
    [<CLIMutable>]
    type ArticleModel = {
        Slug: string
        Title: string
        Description: string
        Body: string
        TagList: string array
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
        Favorited: bool
        FavoritesCount: int
        Author : ProfileModel
    }
    
    [<CLIMutable>]
    type SingleArticleModelEnvelope = {
        Article: ArticleModel
    }
    
    [<CLIMutable>]
    type MultipleArticlesModelEnvelope = {
        Articles: ArticleModel array
        ArticlesCount: int
    }
    
    [<CLIMutable>]
    type CommentModel = {
        Id: string
        Body: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
        Author: ProfileModel
    }
    
    [<CLIMutable>]
    type ListArticlesQueryModel = {
        Tag: string
        Author: string
        Favorited: string
        Limit: Nullable<int>
        Offset: Nullable<int>
    }
    
    type TagsModelEnvelope = {
        Tags: string array
    }
    
    type SingleCommentModelEnvelope = {
        Comment: CommentModel
    }
    
    type MultipleCommentsModelEnvelope = {
        Comments: CommentModel array
    }
    
    let toUserModelEnvelope token (userInfo: UserInfo) =
        {
            User = {
                Username = userInfo.Username.Value
                Email = userInfo.EmailAddress.Value
                Bio = userInfo.Bio |> Option.defaultValue null
                Image = userInfo.Image |> Option.defaultValue null
                Token = token
            }
        }
        
    let toSimpleProfileModel (userInfo: UserInfo) = 
        {
            Username = userInfo.Username.Value
            Bio = userInfo.Bio |> Option.defaultValue null
            Image = userInfo.Image |> Option.defaultValue null
            Following = false
        }
        
    let toProfileModel (userFollowing: UserFollowing) (userInfo: UserInfo) =
        let model = toSimpleProfileModel userInfo
        if userFollowing.Id <> userInfo.Id then
            { model with Following = userFollowing.Following.Contains userInfo.Id }
        else
            model
        
    let toSimpleProfileModelEnvelope (userInfo: UserInfo) =
        {
            Profile = toSimpleProfileModel userInfo
        }
        
    let toProfileModelEnvelope (userFollowing: UserFollowing) (userInfo: UserInfo)  =
        let envelope = toSimpleProfileModelEnvelope userInfo
        { envelope with Profile = userInfo |> toProfileModel userFollowing }
        
    let toProfileModelReadModel (userEntity: UserEntity) isFollowing =
        {
            Username = userEntity.Username
            Bio = userEntity.Bio
            Image = userEntity.ImageUrl
            Following = isFollowing
        }
                
    let toProfileModelReadModel2 (user: User) isFollowing =
        {
            Username = user.Username
            Bio = user.Bio
            Image = user.ImageUrl
            Following = isFollowing
        }
        
    let toProfileModelReadModelEnvelope (userEntity:UserEntity, isFollowing) =
        {
            Profile = toProfileModelReadModel userEntity isFollowing
        }
        
    let toArticleModelReadModel (articleQuery: ArticleQuery)  =
        {
            Slug = articleQuery.Article.Slug
            Title = articleQuery.Article.Title
            Description = articleQuery.Article.Description
            Body = articleQuery.Article.Body
            TagList = articleQuery.Article.Tags |> Seq.map (fun x -> x.Tag) |> Array.ofSeq
            CreatedAt = articleQuery.Article.CreatedAt
            UpdatedAt = articleQuery.Article.UpdatedAt
            Favorited = articleQuery.IsFavorited
            FavoritesCount = articleQuery.FavoriteCount
            Author = toProfileModelReadModel articleQuery.Article.User articleQuery.IsFollowingAuthor
        }
        
    let toArticleModelReadModel2 (article: Article, user: User, tags: string array, isFavorite: bool, favoriteCount:int, isFollowingAuthor: bool)  =
        {
            Slug = article.Slug
            Title = article.Title
            Description = article.Description
            Body = article.Body
            TagList = tags
            CreatedAt = article.CreatedAt
            UpdatedAt = article.UpdatedAt
            Favorited = isFavorite
            FavoritesCount = favoriteCount
            Author = toProfileModelReadModel2 user isFollowingAuthor
        }
        
    let toSingleArticleEnvelopeReadModel articleQuery  =
        {
            Article = toArticleModelReadModel articleQuery 
        }
        
    let toMultipleArticlesEnvelopeReadModel articles  =
        let articles = articles |> Seq.map toArticleModelReadModel |> Array.ofSeq
        {
            Articles = articles
            ArticlesCount = articles.Length
        }
            
    let toMultipleArticlesEnvelopeReadModel2 (queryResult: ListArticlesQueryResult) =
        let articles = 
            queryResult.ArticlesAndAuthors
            |> Seq.map (fun (article, author) -> 
                let tags = queryResult.ArticlesTagsMap |> Map.find article.Id |> Array.ofSeq
                let isFavorite = queryResult.UserFavoriteSet |> Set.contains article.Id
                let isFollowing = queryResult.UserFollowingSet |> Set.contains author.Id
                let favoriteCount = queryResult.FavoriteCountMap |> Map.tryFind article.Id |> Option.defaultValue 0
                toArticleModelReadModel2 (article, author, tags, isFavorite, favoriteCount, isFollowing))
            |> Array.ofSeq

        {
            Articles = articles
            ArticlesCount = articles.Length
        }
    
    let toCommentModel profileModel (comment: RealWorldFSharp.Domain.Articles.Comment) = 
        {
            Id = comment.Id.ToString()
            CreatedAt = comment.CreatedAt
            UpdatedAt = comment.UpdatedAt
            Body = comment.Body.Value
            Author = profileModel
        }
    let toCommentModelEnvelope profileModel (comment: RealWorldFSharp.Domain.Articles.Comment) =
        {
            Comment = toCommentModel profileModel comment
        }
        
    let toCommentsReadModelEnvelope (commentsAndAuthors: (Comment * User) array, userFollowing) =
        {
            Comments = commentsAndAuthors
                       |> Array.map (fun (comment, user) -> {
                           Id = comment.Id
                           Body = comment.Body
                           CreatedAt = comment.CreatedAt
                           UpdatedAt = comment.UpdatedAt
                           Author = {
                                Username = user.Username
                                Bio = user.Bio
                                Image = user.ImageUrl
                                Following = userFollowing |> Set.contains user.Id
                           }
                       })
        }
        
    let toTagsModelEnvelope tags =
        {
            Tags = tags |> Array.ofSeq
        }