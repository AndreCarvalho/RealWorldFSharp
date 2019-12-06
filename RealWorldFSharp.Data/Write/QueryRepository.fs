namespace RealWorldFSharp.Data.Write

open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open DataEntities
open RealWorldFSharp.Common.Errors

module QueryRepository =
    
    type IoQueryResult<'a> = Async<'a option>

    type GetApplicationUserByUsername = UserManager<ApplicationUser> -> Username -> IoQueryResult<ApplicationUser>
    type GetApplicationUserById = UserManager<ApplicationUser> -> UserId -> IoQueryResult<ApplicationUser>
    type GetUserFollowing = ApplicationDbContext -> UserId -> IoResult<UserFollowingsEntity>
    type GetArticle = ApplicationDbContext -> Slug -> IoQueryResult<ArticleEntity>
    type GetComment = ApplicationDbContext -> CommentId -> IoQueryResult<ArticleCommentEntity>
    type GetFavoriteArticles = ApplicationDbContext -> UserId -> IoResult<FavoriteArticlesEntity>

    let getApplicationUserByUsername : GetApplicationUserByUsername =
        fun userManager username ->
            async {
                let! user = userManager.FindByNameAsync username |> Async.AwaitTask
                return Option.ofObj user
            }
            
    let getApplicationUserById : GetApplicationUserById =
        fun userManager id ->
            async {
                let! user = userManager.FindByIdAsync id |> Async.AwaitTask
                return Option.ofObj user
            }
            
    let getUserFollowing : GetUserFollowing =
        fun dbContext userId ->
            async {
                try
                    let query = query {
                        for f in dbContext.UsersFollowing.AsNoTracking() do
                        where (f.FollowerId = userId)
                        select f
                    }
                    
                    let userFollowing = {
                        Id = userId
                        Following = query |> Seq.map (fun x -> x.FollowedId) |> List.ofSeq
                    }
                    
                    return Ok userFollowing
                with
                | ex -> return GenericError ex |> Error //TODO: let it crash?
            }
            
    let getArticle : GetArticle =
        fun dbContext slug ->
            async {
                let articleQuery = query {
                    for f in dbContext.Articles.Include("Tags").AsNoTracking() do
                    where (f.Slug = slug)
                    select f
                    exactlyOneOrDefault
                }
                
                return Option.ofObj articleQuery
            }
            
    let getComment : GetComment =
        fun dbContext commentId ->
            async {
                let articleQuery = query {
                    for c in dbContext.ArticleComments.AsNoTracking() do
                    where (c.Id = commentId)
                    select c
                    exactlyOneOrDefault
                }
                
                return Option.ofObj articleQuery
            }
            
            
    let getFavoriteArticles : GetFavoriteArticles =
        fun dbContext userId ->
            async {
                try
                    let query = query {
                        for f in dbContext.FavoriteArticles.AsNoTracking() do
                        where (f.UserId = userId)
                        select f
                    }
                    
                    let favoriteArticles = {
                        UserId = userId
                        Favorites = query |> Seq.map (fun x -> x.ArticleId) |> List.ofSeq
                    }
                    
                    return Ok favoriteArticles
                with
                | ex -> return GenericError ex |> Error //TODO: let it crash?
            }