namespace RealWorldFSharp.Api

open System
open System.Collections.Generic
open RealWorldFSharp.Domain
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Common

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
        Following: Nullable<bool>
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
            Following = Nullable.empty<bool> 
        }
        
    let toProfileModel (userFollowing: UserFollowing) (userInfo: UserInfo) =
        let model = toSimpleProfileModel userInfo
        if userFollowing.Id <> userInfo.Id then
            { model with Following = userFollowing.Following.Contains userInfo.Id |> Nullable.from }
        else
            model
        
    let toSimpleProfileModelEnvelope (userInfo: UserInfo) =
        {
            Profile = toSimpleProfileModel userInfo
        }
        
    let toProfileModelEnvelope (userFollowing: UserFollowing) (userInfo: UserInfo)  =
        let envelope = toSimpleProfileModelEnvelope userInfo
        { envelope with Profile = userInfo |> toProfileModel userFollowing }

    let toArticleModel (profileModel: ProfileModel) (article: Article) =
        {
            Slug = article.Slug.Value
            Title = article.Title.Value
            Description = article.Description.Value
            Body = article.Body.Value
            TagList = article.Tags |> Array.ofList |> Array.map (fun x -> x.Value)
            CreatedAt = article.CreatedAt
            UpdatedAt = article.UpdatedAt
            Favorited = false //TODO
            FavoritesCount = 0 //TODO
            Author = profileModel
        }
    let toSingleArticleEnvelope profileModel article =
        {
            Article = toArticleModel profileModel article
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
        
    let toCommentsModelEnvelope commentAndProfilePairs =
        let commentsModel = commentAndProfilePairs |> List.map (fun (c, p) -> toCommentModel p c) |> Array.ofList 
        {
            Comments = commentsModel
        }
        