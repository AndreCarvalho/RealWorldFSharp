namespace RealWorldFSharp.Api

open System
open System.Collections.Generic
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Common
open RealWorldFSharp.Data.ReadModels

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
    type ArticleModelEnvelope = {
        Article: ArticleModel
    }
    
    [<CLIMutable>]
    type CommentModel = {
        Id: string
        Body: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
        Author: ProfileModel
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
        
    let toProfileModelReadModel (userEntity: UserEntity) =
        {
            Username = userEntity.Username
            Bio = userEntity.Bio
            Image = userEntity.ImageUrl
            Following = false // todo
        }
        
    let toArticleModelReadModel favoriteCount isFavorite (article: ArticleEntity)  =
        {
            Slug = article.Slug
            Title = article.Title
            Description = article.Description
            Body = article.Body
            TagList = article.Tags |> Seq.map (fun x -> x.Tag) |> Array.ofSeq
            CreatedAt = article.CreatedAt
            UpdatedAt = article.UpdatedAt
            Favorited = isFavorite
            FavoritesCount = favoriteCount
            Author = toProfileModelReadModel article.User
        }
        
    let toSingleArticleEnvelopeReadModel favoriteCount isFavorite article  =
        {
            Article = toArticleModelReadModel favoriteCount isFavorite article 
        }
    
    let toCommentModel profileModel (comment: Comment) = 
        {
            Id = comment.Id.ToString()
            CreatedAt = comment.CreatedAt
            UpdatedAt = comment.UpdatedAt
            Body = comment.Body.Value
            Author = profileModel
        }
    let toCommentModelEnvelope profileModel (comment: Comment) =
        {
            Comment = toCommentModel profileModel comment
        }
        
    let toCommentsReadModelEnvelope (commentEntities: seq<ArticleCommentEntity *  bool>) =
        {
            Comments = commentEntities
                       |> Seq.map (fun (comment, isFollowing) -> {
                           Id = comment.Id
                           Body = comment.Body
                           CreatedAt = comment.CreatedAt
                           UpdatedAt = comment.UpdatedAt
                           Author = {
                               Username = comment.User.Username
                               Bio = comment.User.Bio
                               Image = comment.User.ImageUrl
                               Following = isFollowing
                           }
                       })
                       |> Array.ofSeq
        }
        
    let toTagsModelEnvelope tags =
        {
            Tags = tags |> Array.ofSeq
        }