namespace RealWorldFSharp.Data.Read

open FsToolkit.ErrorHandling
open System.Linq
open FSharp.Data.Sql
open ReadModels

module ReadModelQueries =
   
    type SqlProvider  = SqlDataProvider<
                            Common.DatabaseProviderTypes.MSSQLSERVER,
                            "server=.\\SQLEXPRESS;database=RealWorldFSharp;Integrated security=SSPI",
                            UseOptionTypes=false>
    
    type ArticleQuery = {
        Article: Article
        Author: User
        Tags: string []
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
    
    type FeedArticlesQueryParams = {
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
    
    type FeedArticlesQueryResult = {
        ArticlesAndAuthors: seq<Article * User>
        ArticlesTagsMap: Map<string, string array>
        FavoriteCountMap: Map<string, int>
        UserFavoriteSet: Set<string>
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
        
    let private mapComment (record: SqlProvider.dataContext.``dbo.ArticleCommentsEntity``) : ReadModels.Comment =
        {
            Id = record.Id
            Body = record.Body
            CreatedAt = record.CreatedAt
            UpdatedAt = record.UpdatedAt
        }

    let private getIsFollowing (ctx: SqlProvider.dataContext) userIdOption =
        async {
            let! x =
                match userIdOption with
                | Some userId ->
                    query {
                        for fol in ctx.Dbo.UsersFollowing do
                        where (fol.FollowerId = userId)
                        select fol.FollowedId
                    } |> Array.executeQueryAsync
                | None -> async {return [||]}
            return x |> Set.ofArray
        }

    let getDataContext (connectionString: string) = SqlProvider.GetDataContext(connectionString)
    
    let getCommentsForArticle (connectionString: string) =
        fun userIdOption articleId ->
            async {
                let ctx = SqlProvider.GetDataContext(connectionString)
                
                let! commentsQuery = 
                    query {
                        for comment in ctx.Dbo.ArticleComments do
                        join author in ctx.Dbo.AspNetUsers on (comment.UserId = author.Id)
                        where (comment.ArticleId = articleId)
                        select (comment, author)
                    } |> Array.executeQueryAsync
                    
                let! isFollowingSet = getIsFollowing ctx userIdOption
                let mapped = commentsQuery |> Array.map (fun (comment, author) -> (mapComment comment, mapUser author))

                return (mapped, isFollowingSet)
            }
    
    let getTags (ctx: SqlProvider.dataContext) =
        query {
            for tag in ctx.Dbo.ArticleTags do
            select tag.Tag
            distinct
        } |> Array.executeQueryAsync
        
    let private getArticleTagsMap (ctx: SqlProvider.dataContext) =
        fun (articleIds: string seq) ->
            query {
                for tag in ctx.Dbo.ArticleTags do
                where (articleIds.Contains(tag.ArticleId))
                select (tag.ArticleId, tag.Tag)
            } |> Seq.toArray |> Array.groupBy (fun x -> fst x) |> Array.map (fun (k, v) -> (k, Array.map snd v)) |> Map.ofSeq
        
    let private getArticleQueryResult (ctx: SqlProvider.dataContext) =
        fun userIdOption (article: SqlProvider.dataContext.``dbo.ArticlesEntity``) ->
            let (article, author) =
                query {
                    for user in ctx.Dbo.AspNetUsers do
                    where (user.Id = article.UserId)
                    select (mapArticle article, mapUser user)
                    exactlyOne
                }
                
            let countFavorited = query {
                for fav in ctx.Dbo.ArticlesFavorited do
                where (fav.ArticleId = article.Id)
                count
            }
            
            let getTagsQuery = query {
                for tag in ctx.Dbo.ArticleTags do
                where (tag.ArticleId = article.Id)
                select tag.Tag
            }
            
            let tags = getTagsQuery |> Array.ofSeq
            
            match userIdOption with
            | Some userId ->
                let isFollowingQuery =
                    query {
                        for f in ctx.Dbo.UsersFollowing do
                        where (f.FollowedId = author.Id && f.FollowerId = userId)
                        count
                    }
                let isFavoriteQuery =
                    query {
                        for favorite in ctx.Dbo.ArticlesFavorited do
                        where (favorite.UserId = userId && favorite.ArticleId = article.Id)
                        count
                    }
                { 
                    Article = article
                    Author = author
                    Tags = tags
                    FavoriteCount = countFavorited
                    IsFavorited = (isFavoriteQuery = 1)
                    IsFollowingAuthor = (isFollowingQuery = 1)
                }
            | None -> { 
                Article = article
                Author = author
                Tags = tags
                FavoriteCount = countFavorited
                IsFavorited = false 
                IsFollowingAuthor = false
            }
            
    let getArticleBySlug (ctx: SqlProvider.dataContext) =
        fun userIdOption articleSlug ->
            async {
                let articleQuery = query {
                    for article in ctx.Dbo.Articles do
                    where (article.Slug = articleSlug)
                    select article
                    exactlyOneOrDefault
                } // HACK: workaround to be able to map to option from nullable type

                let articleOption = articleQuery |> Option.ofObj
                return articleOption |> Option.map (getArticleQueryResult ctx userIdOption)
            }
            
    let getArticleById (ctx: SqlProvider.dataContext) =
        fun userIdOption articleId ->
            async {
                let articleQuery = query {
                    for article in ctx.Dbo.Articles do
                    where (article.Id = articleId)
                    select article
                    exactlyOneOrDefault
                } // HACK: workaround to be able to map to option from nullable type

                let articleOption = articleQuery |> Option.ofObj
                return articleOption |> Option.map (getArticleQueryResult ctx userIdOption)
            }


    let getUserProfileReadModel (ctx: SqlProvider.dataContext) =
        fun userIdOption profileUserName ->
            async {
                return
                    query {
                        for user in ctx.Dbo.AspNetUsers do
                        where (user.UserName = profileUserName)
                        select user
                        exactlyOneOrDefault
                    }
                    |> Option.ofObj
                    |> Option.map (fun user ->
                        match userIdOption with
                        | Some userId ->
                            let isFollowingQuery =
                                query {
                                    for f in ctx.Dbo.UsersFollowing do
                                    where (f.FollowedId = user.Id && f.FollowerId = userId)
                                    count
                                }
                            (mapUser user, isFollowingQuery = 1)
                        | None -> (mapUser user, false))
            }

    let listArticles (ctx: SqlProvider.dataContext)  =
        fun userIdOption (queryParams: ListArticlesQueryParams) ->
            async {
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

                let articleTagsMap = getArticleTagsMap ctx articleIds
                    
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

                let! isFollowingSet = getIsFollowing ctx userIdOption
                let mapped = articlesResults |> Seq.map (fun (article, author) -> (mapArticle article, mapUser author))
                
                return { 
                    ArticlesAndAuthors = mapped
                    ArticlesTagsMap = articleTagsMap 
                    FavoriteCountMap = favoriteCountMap 
                    UserFavoriteSet = isFavoriteSet 
                    UserFollowingSet = isFollowingSet
                }
            }

    let feedArticles (ctx: SqlProvider.dataContext)  =
        fun userId (queryParams: FeedArticlesQueryParams) ->
            async {
                let articlesQuery = 
                    query {
                        for article in ctx.Dbo.Articles do
                        for author in article.``dbo.AspNetUsers by Id`` do
                        for following in author.``dbo.UsersFollowing by Id`` do
                        where (following.FollowerId = userId)
                        sortByDescending article.CreatedAt
                        select (mapArticle article, mapUser author)
                        skip queryParams.Offset
                        take queryParams.Limit
                    }
                    
                let! articlesResults = articlesQuery |> Array.executeQueryAsync
                let articleIds = articlesResults |> Array.map (fun (x,_) -> x.Id)
                
                let articleTagsMap = getArticleTagsMap ctx articleIds
                    
                let favoriteCountMap =
                    query {
                        for fav in ctx.Dbo.ArticlesFavorited do
                        where (articleIds.Contains(fav.ArticleId))
                        groupBy fav.ArticleId into g
                        select (g.Key, g.Count())
                    } |> Seq.toArray |> Map.ofSeq

                let isFavoriteSet =
                    query {
                        for fav in ctx.Dbo.ArticlesFavorited do
                        where (fav.UserId = userId)
                        select fav.ArticleId
                    } |> Set.ofSeq
                
                return { 
                    ArticlesAndAuthors = articlesResults
                    ArticlesTagsMap = articleTagsMap 
                    FavoriteCountMap = favoriteCountMap 
                    UserFavoriteSet = isFavoriteSet 
                }
            }