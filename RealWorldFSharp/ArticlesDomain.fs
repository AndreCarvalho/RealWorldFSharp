namespace RealWorldFSharp.Articles

open System
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Common
open RealWorldFSharp.Domain

module Domain =
    let f = 2

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
                
    type Body = private Body of string
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

    type ArticleId = Guid
    
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
        Body: Body
        Tags: Tag list
        UserId: UserId
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
    }
            
    let simpleCreateSlug (title: Title) (dateTime:DateTimeOffset) =
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
            Slug = simpleCreateSlug title dateTime
            Body = body
            Tags = tags
            CreatedAt = dateTime
            UpdatedAt = dateTime
            UserId = userId
        }
