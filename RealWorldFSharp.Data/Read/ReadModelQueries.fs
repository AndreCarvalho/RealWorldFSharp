namespace RealWorldFSharp.Data.Read

open RealWorldFSharp.Data.ReadModels
open Microsoft.EntityFrameworkCore

module ReadModelQueries =
   
    type ArticleQuery =
        {
            Article: ArticleEntity
            FavoriteCount: int
            IsFavorited: bool
            IsFollowingAuthor: bool
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
                        Article = article;
                        FavoriteCount = favoriteCount;
                        IsFavorited = false;
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