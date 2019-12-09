namespace RealWorldFSharp.Data.Read

open Microsoft.EntityFrameworkCore
open System.Linq
open FSharp.Data.Sql
open ReadModels

module ReadModelQueries =
   
    type SqlProvider  = SqlDataProvider<
                            Common.DatabaseProviderTypes.MSSQLSERVER,
                            "server=.\\SQLEXPRESS;database=RealWorldFSharp;Integrated security=SSPI",
                            UseOptionTypes=false,
                            ResolutionPath="C:\\Users\\andre\\.nuget\\packages\\system.data.sqlclient\\4.8.0\\lib\\netstandard2.0">
    
    type ArticleQuery = {
        Article: ArticleEntity
        FavoriteCount: int
        IsFavorited: bool
        IsFollowingAuthor: bool
    }
        
    type ListArticlesQueryParams = {
        Tag: string option
        Author: string option
        Favorited: string option
        Limit: int
        Offset: int
    }

    type ListArticlesQueryResult = {
        ArticlesAndAuthors: seq<Article * User>
        ArticlesTagsMap: Map<string, string array>
        FavoriteCountMap: Map<string, int>
        UserFavoriteSet: Set<string>
        UserFollowingSet: Set<string>
    }
    
    let getCommentsForArticle (dbContext: ReadDataContext) =
        fun userIdOption articleId ->
            match userIdOption with
            | Some userId ->
                async {
                    return query {
                        for cs in dbContext.ArticleComments.Include("User") do
                        where (cs.ArticleId = articleId)
                        
                        let followingQuery =
                            query {
                                for following in dbContext.UsersFollowing do
                                where (following.FollowerId = userId && cs.UserId = following.FollowedId)
                                count
                            }
                            
                        select (cs, followingQuery = 1)
                    }
                }
            | None ->
                async {
                    return query {
                        for cs in dbContext.ArticleComments.Include("User") do
                        where (cs.ArticleId = articleId)
                        select (cs, false)
                    }
                }

    let getTags (dbContext: ReadDataContext) =
        async {
            return query {
                for tag in dbContext.ArticleTags do
                select tag.Tag
                distinct
            }
        }
        
    let getArticle (dbContext: ReadDataContext) =
        fun articleId ->
            async {
                return query {
                    for article in dbContext.Articles.Include("User").Include("Tags") do
                    where (article.Id = articleId)
                    let favoriteCount = article.Favorited.Count
                    select {
                        Article = article
                        FavoriteCount = favoriteCount
                        IsFavorited = false
                        IsFollowingAuthor = false
                    }
                    exactlyOneOrDefault
                }
            }
            
    let getArticleForUser (dbContext: ReadDataContext) =
        fun userId articleId ->
            async {
                return query {
                    for article in dbContext.Articles.Include("User").Include("Tags") do
                    where (article.Id = articleId)
                    let favoriteCount = article.Favorited.Count
                    let isFavoriteQuery =
                        query {
                            for favorite in article.Favorited do
                            where (favorite.UserId = userId)
                            count
                        }
                    let isFollowingQuery =
                        query {
                            for f in dbContext.UsersFollowing do
                            where (f.FollowerId = userId)
                            count
                        }
                    select {
                        Article = article;
                        FavoriteCount = favoriteCount;
                        IsFavorited = isFavoriteQuery = 1;
                        IsFollowingAuthor = isFollowingQuery = 1
                    }
                    exactlyOneOrDefault
                }
            }
            
    let getUserProfileReadModel (dbContext: ReadDataContext) =
        fun profileUserId requestingUserIdOption ->
            match requestingUserIdOption with
            | Some userId ->
                async {
                    return query {
                        for u in dbContext.Users do
                        where (u.Id = profileUserId)
                        let isFollowingQuery =
                            query {
                                for f in dbContext.UsersFollowing do
                                where (f.FollowerId = userId)
                                count
                            }
                        select (u, isFollowingQuery = 1)
                        exactlyOneOrDefault
                    }
                }
            | None ->
                async {
                    return query {
                        for u in dbContext.Users do
                        where (u.Id = profileUserId)
                        select (u, false)
                        exactlyOneOrDefault
                    }
                }
                
    let private mapArticle (record: SqlProvider.dataContext.``dbo.ArticlesEntity``) : ReadModels.Article =
        {
            Id = record.Id
            Slug = record.Slug
            Title = record.Title
            Description = record.Description
            Body = record.Body
            CreatedAt = record.CreatedAt
            UpdatedAt = record.UpdatedAt
        }  
        
    let private mapUser (record: SqlProvider.dataContext.``dbo.AspNetUsersEntity``) : ReadModels.User =
        {
            Id = record.Id
            Bio = record.Bio
            Username = record.UserName
            ImageUrl = record.ImageUrl
        }

    let listArticles (connectionString: string)  =
        fun userIdOption (queryParams: ListArticlesQueryParams) ->
            async {
                let ctx = SqlProvider.GetDataContext(connectionString)
                
                let articlesQuery = 
                    query {
                        for article in ctx.Dbo.Articles do
                        join author in ctx.Dbo.AspNetUsers on (article.UserId = author.Id)
                        sortByDescending article.CreatedAt
                        select (article, author)
                        skip queryParams.Offset
                        take queryParams.Limit
                    }

                let filterByTagQuery =
                    match queryParams.Tag with
                    | Some t ->
                        query {
                            for (article, author) in articlesQuery do
                            join tag in ctx.Dbo.ArticleTags on (article.Id = tag.ArticleId)
                            where (tag.Tag = t)
                            select (article, author) 
                        }
                    | None -> articlesQuery

                let filterByAuthor =
                    match queryParams.Author with
                    | Some a ->
                        query {
                            for (article, author) in filterByTagQuery do
                            where (author.UserName = a)
                            select (article, author) 
                        }
                    | None -> filterByTagQuery

                let filterByFavoritedBy =
                    match queryParams.Favorited with
                    | Some username ->
                        let favorited = query {
                            for user in ctx.Dbo.AspNetUsers do
                            join fav in ctx.Dbo.ArticlesFavorited on (user.Id = fav.UserId)
                            where (user.UserName = username)
                            select fav
                        }
                        query {
                            for (article, author) in filterByAuthor do
                            join fav in favorited on (article.Id = fav.ArticleId)
                            select (article, author) 
                        }
                    | None -> filterByAuthor

                let! articlesResults = filterByFavoritedBy |> Array.executeQueryAsync

                let articleIds = articlesResults |> Array.map (fun (x,_) -> x.Id)

                let articleTagsMap =
                    query {
                        for tag in ctx.Dbo.ArticleTags do
                        where (articleIds.Contains(tag.ArticleId))
                        select (tag.ArticleId, tag.Tag)
                    } |> Seq.toArray |> Array.groupBy (fun x -> fst x) |> Array.map (fun (k, v) -> (k, Array.map snd v)) |> Map.ofSeq
                    
                let favoriteCountMap =
                    query {
                        for fav in ctx.Dbo.ArticlesFavorited do
                        where (articleIds.Contains(fav.ArticleId))
                        groupBy fav.ArticleId into g
                        select (g.Key, g.Count())
                    } |> Seq.toArray |> Map.ofSeq

                let isFavoriteSet =
                    match userIdOption with
                    | Some userId ->
                        query {
                            for fav in ctx.Dbo.ArticlesFavorited do
                            where (fav.UserId = userId)
                            select fav.ArticleId
                        } |> Set.ofSeq
                    | None -> Set.empty

                let isFollowingSet =
                    match userIdOption with
                    | Some userId ->
                        query {
                            for fol in ctx.Dbo.UsersFollowing do
                            where (fol.FollowerId = userId)
                            select fol.FollowedId
                        } |> Set.ofSeq
                    | None -> Set.empty

                let mapped = articlesResults |> Seq.map (fun (article, author) -> (mapArticle article, mapUser author))
                
                return { 
                    ArticlesAndAuthors = mapped
                    ArticlesTagsMap = articleTagsMap 
                    FavoriteCountMap = favoriteCountMap 
                    UserFavoriteSet = isFavoriteSet 
                    UserFollowingSet = isFollowingSet
                }
            }
