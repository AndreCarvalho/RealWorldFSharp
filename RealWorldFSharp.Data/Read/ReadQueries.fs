namespace RealWorldFSharp.Data.Read

open RealWorldFSharp.Data.ReadModels
open Microsoft.EntityFrameworkCore

module ReadQueries =
    let getCommentsForArticle (dbContext: ReadDataContext) (articleId:string) =
        async {
            let query =
                query {
                    for cs in dbContext.ArticleComments.Include("User") do
                    where (cs.ArticleId = articleId)
                    select cs
                }
            return query
        }

