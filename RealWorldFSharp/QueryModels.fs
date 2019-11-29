namespace RealWorldFSharp

open Articles.Domain
open System
open System
open System
open System
open System
open System
open System.Collections.Generic
open RealWorldFSharp.Domain
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
        
    let toSimpleProfileModelEnvelope (userInfo: UserInfo) =
        {
            Profile = toSimpleProfileModel userInfo
        }
        
    let toProfileModelEnvelope (userFollowing: UserFollowing) (userInfo: UserInfo)  =
        let envelope = toSimpleProfileModelEnvelope userInfo
        { envelope with Profile = { envelope.Profile with Following = userFollowing.Following.Contains userInfo.Id |> Nullable.from }}

    let toArticleModel (userInfo: UserInfo) (article: Article) =
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
            Author = toSimpleProfileModel userInfo
        }
    let toSingleArticleEnvelope userInfo article =
        {
            Article = toArticleModel userInfo article
        }