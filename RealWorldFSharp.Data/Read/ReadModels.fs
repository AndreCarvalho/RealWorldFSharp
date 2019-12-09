namespace RealWorldFSharp.Data.Read

open System

module ReadModels =

    type Article = {
        Id: string
        Slug: string
        Title: string
        Description: string
        Body: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
    }    
    
    type User = {
        Id: string
        Username: string
        Bio: string
        ImageUrl: string
    }
    
    type Comment = {
        Id: string
        CreatedAt: DateTimeOffset
        UpdatedAt: DateTimeOffset
        Body: string
    }