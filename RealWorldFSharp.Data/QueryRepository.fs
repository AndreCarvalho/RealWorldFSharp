namespace RealWorldFSharp.Data

open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Common.Errors

module QueryRepository =
    
    type IoQueryResult<'a> = Async<'a option>

    type GetApplicationUserByUsername = UserManager<ApplicationUser> -> Username -> IoQueryResult<ApplicationUser>
    type GetApplicationUserById = UserManager<ApplicationUser> -> UserId -> IoQueryResult<ApplicationUser>
    type GetUserFollowing = ApplicationDbContext -> UserId -> IoResult<UserFollowingEntity>
    type GetArticle = ApplicationDbContext -> Slug -> IoQueryResult<(ArticleEntity * ArticleTagEntity list)>

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
                        for f in dbContext.UsersFollowing do
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
                    for f in dbContext.Articles do
                    where (f.Slug = slug)
                    select f
                    exactlyOneOrDefault
                }
                
                let articleEntityOption = Option.ofObj articleQuery
                
                let articleTagsQuery =
                    match articleEntityOption with
                    | Some article ->
                            query {
                                for t in dbContext.ArticleTags do
                                where (t.ArticleId = article.Id)
                                select t
                            }
                            |> List.ofSeq
                    | None -> []
                    
                return articleEntityOption |> Option.map (fun a -> (a, articleTagsQuery))
            }