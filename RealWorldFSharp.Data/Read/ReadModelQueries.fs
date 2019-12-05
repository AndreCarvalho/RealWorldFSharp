namespace RealWorldFSharp.Data.Read

open RealWorldFSharp.Data.ReadModels
open Microsoft.EntityFrameworkCore

module ReadModelQueries =
    
    let getCommentsForArticle (dbContext: ReadDataContext) (articleId:string) =
        async {
            return query {
                for cs in dbContext.ArticleComments.Include("User") do
                where (cs.ArticleId = articleId)
                select cs
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
                    select (article, favoriteCount)
                    exactlyOneOrDefault
                }
            }
            
    let getArticleWithFavorite (dbContext: ReadDataContext) =
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
                    select (article, favoriteCount, isFavoriteQuery = 1)
                    exactlyOneOrDefault
                }
            }