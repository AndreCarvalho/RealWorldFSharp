namespace RealWorldFSharp.Domain

open System
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Common
open RealWorldFSharp.Domain.Users

module Articles =

    type Title = private Title of string
        with
        member this.Value = match this with Title ti -> ti
        static member create fieldName title =
            if isNullOrEmpty title then
                validationError fieldName "title must not be null or empty"
            elif length title > 100 then
                validationError fieldName "title can not have more than 100 characters"
            else
                Ok <| Title title
                
    type Description = private Description of string
        with
        member this.Value = match this with Description de -> de
        static member create fieldName description =
            if isNullOrEmpty description then
                validationError fieldName "description must not be null or empty"
            elif length description > 50 then
                validationError fieldName "description can not have more than 50 characters"
            else
                Ok <| Description description
                
    type ArticleBody = private Body of string
        with
        member this.Value = match this with Body bo -> bo
        static member create fieldName body =
            if isNullOrEmpty body then
                validationError fieldName "body must not be null or empty"
            elif length body > 5000 then
                validationError fieldName "body can not have more than 5000 characters"
            else
                Ok <| Body body
                
    
    type Tag = private Tag of string
        with
        member this.Value = match this with Tag tag -> tag
        static member create fieldName tag =
            if isNullOrEmpty tag then
                validationError fieldName "tag must not be null or empty"
            elif length tag > 20 then
                validationError fieldName "tag can not have more than 20 characters"
            else
                Ok <| Tag tag

    type ArticleId = Guid  // TODO expand with private ctor and factory with validation
    
    type Slug = private Slug of string
        with
        member this.Value = match this with Slug slug -> slug
        static member create slug =
            Slug slug
    
    type Article = {
        Id: ArticleId
        Title: Title
        Slug: Slug
        Description: Description
        Body: ArticleBody
        Tags: Tag list
        AuthorUserId: UserId
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
    }
    
    type CommentBody = private CommentBody of string
        with
        member this.Value = match this with CommentBody comment -> comment
        static member create fieldName comment =
            if isNullOrEmpty comment then
                validationError fieldName "comment must not be null or empty"
            elif length comment > 200 then
                validationError fieldName "comment can not have more than 200 characters"
            else
                Ok <| CommentBody comment
               
    type CommentId = Guid // TODO expand with private ctor and factory with validation
 
    type Comment = {
        Id: CommentId
        Body: CommentBody
        ArticleId : ArticleId
        AuthorUserId: UserId
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
    }
    
    type FavoriteArticles = {
        Id: UserId
        Favorites: Set<ArticleId>
    }
    
    type AddFavoriteResult = Add | AlreadyPresent
    type RemoveFavoriteResult = Remove | NotPresent
    
    let addToFavorites (article: Article) favoriteArticles =
        if not <| favoriteArticles.Favorites.Contains article.Id then Add
        else AlreadyPresent           
    
    let removeFromFavorites (article: Article) favoriteArticles =
        if favoriteArticles.Favorites.Contains article.Id then Remove
        else NotPresent
            
    let simpleCreateSlug  (dateTime:DateTimeOffset) (title: Title) =
        let title = title.Value
        let words = title.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
        let fstPart = String.Join('-', words)
        let suffix = dateTime.ToUnixTimeMilliseconds().ToString()
        Slug (String.Concat(fstPart, "-", suffix))
        
    let createArticle userId title description body tags dateTime =
        {
            Id = Guid.NewGuid()
            Title = title
            Description = description
            Slug = simpleCreateSlug dateTime title
            Body = body
            Tags = tags
            CreatedAt = dateTime
            UpdatedAt = dateTime
            AuthorUserId = userId
        }
        
    let updateArticle titleOption bodyOption descriptionOption dateTime article =
        { article with
            Title = titleOption |> Option.defaultValue article.Title
            Body = bodyOption |> Option.defaultValue article.Body
            Description = descriptionOption |> Option.defaultValue article.Description
            Slug = titleOption |> Option.map (simpleCreateSlug dateTime) |> Option.defaultValue article.Slug
            UpdatedAt = dateTime
        }
        
        
    let validateArticleOwner operation (article:Article) userId =
        if article.AuthorUserId = userId then
            Ok article
        else
            operationNotAllowed operation "Not article author"
            
    let createComment commentBody (article:Article) userId dateTime =
        {
            Id = Guid.NewGuid()
            Body = commentBody
            CreatedAt = dateTime
            UpdatedAt = dateTime
            ArticleId = article.Id
            AuthorUserId = userId
        }
        
    let validateDeleteComment (comment:Comment) userId =
        if comment.AuthorUserId = userId then
            Ok comment
        else
            operationNotAllowed "Delete comment" "Not comment author"
        
