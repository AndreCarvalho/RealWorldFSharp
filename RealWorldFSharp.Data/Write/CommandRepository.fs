namespace RealWorldFSharp.Data.Write

open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Common.Errors

module CommandRepository =
    
    type RegisterNewUser = (ApplicationUser * Password) -> UserIdentityResult<unit>
    type AuthenticateUser = (EmailAddress * Password) -> UserIdentityResult<ApplicationUser>
    type UpdateUserEmail = (ApplicationUser * EmailAddress) -> UserIdentityResult<ApplicationUser>
    type UpdateUserUsername = (ApplicationUser * Username) -> UserIdentityResult<ApplicationUser>
    
    // TODO: this needs old and new password to work
    type UpdateUserPassword = (ApplicationUser * Password) -> UserIdentityResult<ApplicationUser>
    type UpdateUserInfo = ApplicationUser -> UserIdentityResult<ApplicationUser>
    type AddUserFollowing = UserFollowingEntity -> IoResult<unit>
    type RemoveUserFollowing = UserFollowingEntity -> IoResult<unit>
    type AddArticle = ArticleEntity -> IoResult<unit>
    type UpdateArticle = ArticleEntity -> IoResult<unit>
    type DeleteArticle = ArticleEntity -> IoResult<unit>
    type AddComment = ArticleCommentEntity -> IoResult<unit>
    type DeleteComment = ArticleCommentEntity -> IoResult<unit>
    type AddFavoriteArticle = FavoriteArticleEntity -> IoResult<unit>
    type RemoveFavoriteArticle = FavoriteArticleEntity -> IoResult<unit>

    let registerNewUser (userManager: UserManager<ApplicationUser>) : RegisterNewUser =
        fun (applicationUser, password) ->
            async {
                let! res = userManager.CreateAsync(applicationUser, password) |> Async.AwaitTask
                
                if res.Succeeded then
                    return Ok ()
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }
            
    let authenticateUser (userManager: UserManager<ApplicationUser>) : AuthenticateUser =
        fun (email, password) ->
            async {
                let! user = userManager.FindByEmailAsync email |> Async.AwaitTask
                
                if user = null then
                    return userNotFoundError email
                else
                    let! res = userManager.CheckPasswordAsync(user, password) |> Async.AwaitTask
                    
                    if res then
                        return Ok user
                    else
                        return wrongPasswordError email
            }
            
    let updateUserEmail (userManager: UserManager<ApplicationUser>) : UpdateUserEmail =
        fun (user, email) ->
            async {
                let! res = userManager.SetEmailAsync(user, email) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }
            
    let updateUserUsername (userManager: UserManager<ApplicationUser>) : UpdateUserUsername =
        fun (user, username) ->
            async {
                let! res = userManager.SetUserNameAsync(user, username) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }            
    let updateUserInfo (userManager: UserManager<ApplicationUser>) : UpdateUserInfo =
        fun user ->
            async {
                let! res = userManager.UpdateAsync(user) |> Async.AwaitTask
                if res.Succeeded then
                    return Ok user
                else
                    let firstError = res.Errors |> Seq.head
                    return identityError firstError.Code firstError.Description 
            }
            
    let addUserFollowing (dbContext: ApplicationDbContext) : AddUserFollowing =
        fun userFollowing ->
            async {
                let! _ = (dbContext.UsersFollowing.AddAsync userFollowing).AsTask() |> Async.AwaitTask
                return Ok ()
                // TODO: handle ex?
            }
            
    let removeUserFollowing (dbContext: ApplicationDbContext) : RemoveUserFollowing =
        fun userFollowing ->
            async {
                do (dbContext.UsersFollowing.Remove userFollowing) |> ignore
                return Ok ()
                // TODO: handle ex?
            }
            
    let addArticle (dbContext: ApplicationDbContext) : AddArticle =
        fun articleEntity ->
            async {
                let! _ = (dbContext.Articles.AddAsync articleEntity).AsTask() |> Async.AwaitTask
                return Ok ()
                // TODO: handle ex?
            }
            
    let updateArticle (dbContext: ApplicationDbContext) : UpdateArticle =
        fun articleEntity ->
            async {
                articleEntity.Tags <- null // hack to handle duplicate tags on update. it works because we dont update tags...
                do (dbContext.Articles.Update articleEntity) |> ignore 
                return Ok ()
                // TODO: handle ex?
            }
            
    let deleteArticle (dbContext: ApplicationDbContext) : DeleteArticle =
        fun articleEntity ->
            async {
                do (dbContext.Articles.Remove articleEntity) |> ignore
                return Ok ()
                // TODO: handle ex?
            }
            
    let addComment (dbContext: ApplicationDbContext) : AddComment =
        fun commentEntity ->
            async {
                do (dbContext.ArticleComments.AddAsync commentEntity) |> ignore
                return Ok ()
                // TODO: handle ex?
            }
                        
    let deleteComment (dbContext: ApplicationDbContext) : DeleteComment =
        fun commentEntity ->
            async {
                do (dbContext.ArticleComments.Remove commentEntity) |> ignore
                return Ok ()
                // TODO: handle ex?
            }
            
    let addFavoriteArticle (dbContext: ApplicationDbContext) : AddFavoriteArticle =
        fun favoriteArticle ->
            async {
                let! _ = (dbContext.FavoriteArticles.AddAsync favoriteArticle).AsTask() |> Async.AwaitTask
                return Ok ()
                // TODO: handle ex?
            }
            
    let removeFavoriteArticle (dbContext: ApplicationDbContext) : AddFavoriteArticle =
        fun favoriteArticle ->
            async {
                dbContext.FavoriteArticles.Remove favoriteArticle |> ignore
                return Ok ()
                // TODO: handle ex?
            }