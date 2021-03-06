﻿namespace RealWorldFSharp.Data.Write

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Domain.Articles
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Common.Errors

module DataPipeline =
    
    type RegisterNewUser = UserInfo * Password -> UserIdentityResult<unit>
    type AuthenticateUser = EmailAddress * Password -> UserIdentityResult<UserInfo>
    type GetUserInfoByUserIdForUpdate = UserId -> IoQueryResult<(UserInfo * ApplicationUser)>
    type GetUserInfoByUsername = Username -> IoQueryResult<UserInfo>
    type GetUserInfoById = UserId -> IoQueryResult<UserInfo>
    type UpdateUserEmailAddress = (ApplicationUser * EmailAddress) -> UserIdentityResult<UserInfo>
    type UpdateUserUsername = (ApplicationUser * Username) -> UserIdentityResult<UserInfo>
    type UpdateUserInfo = ApplicationUser -> UserIdentityResult<unit>
    type GetUserFollowing = UserId -> IoResult<UserFollowing>
    type AddUserFollowing = (UserId * UserId) -> IoResult<unit>
    type RemoveUserFollowing = (UserId * UserId) -> IoResult<unit>
    type AddArticle = Article -> IoResult<unit>
    type UpdateArticle = Article -> IoResult<unit>
    type GetArticle = Slug -> IoQueryResult<Article>
    type DeleteArticle = Article -> IoResult<unit>
    type AddComment = Comment -> IoResult<unit>
    type GetComment = CommentId -> IoQueryResult<Comment>
    type DeleteComment = Comment -> IoResult<unit>
    type GetFavoriteArticles = UserId -> IoResult<FavoriteArticles>
    type AddFavoriteArticle = (UserId * Article) -> IoResult<unit>
    type RemoveFavoriteArticle = (UserId * Article) -> IoResult<unit>
    
    let registerNewUser (userManager: UserManager<ApplicationUser>) : RegisterNewUser =
        fun (userInfo, password) ->
            let applicationUser = userInfo |> DomainToEntityMapping.mapUserInfoToApplicationUser
            
            asyncResult {
                do! CommandRepository.registerNewUser userManager (applicationUser, password.Value)
            }
            
    let authenticateUser (userManager: UserManager<ApplicationUser>) : AuthenticateUser =
        fun (emailAddress, password) ->
            async {
                let! applicationUser = CommandRepository.authenticateUser userManager (emailAddress.Value, password.Value)
                return applicationUser |> Result.map EntityToDomainMapping.mapApplicationUserToUserInfo
            }
            
    let getUserInfoByUsername (userManager: UserManager<ApplicationUser>) : GetUserInfoByUsername =
        fun (username) ->
            async {
                let! applicationUser = QueryRepository.getApplicationUserByUsername userManager username.Value
                
                return
                    applicationUser
                    |> Option.map (fun user -> (EntityToDomainMapping.mapApplicationUserToUserInfo user))
            }
            
    let getUserInfoByUserIdForUpdate (userManager: UserManager<ApplicationUser>) : GetUserInfoByUserIdForUpdate =
        fun userId ->
            async {
                let! applicationUser = QueryRepository.getApplicationUserById userManager userId.Value
                
                return
                    applicationUser
                    |> Option.map (fun user -> (EntityToDomainMapping.mapApplicationUserToUserInfo user, user))
            }
                
    let getUserInfoById (userManager: UserManager<ApplicationUser>) : GetUserInfoById =
        fun userId ->
            async {
                let! applicationUser = QueryRepository.getApplicationUserById userManager userId.Value
                
                return
                    applicationUser
                    |> Option.map (fun user -> (EntityToDomainMapping.mapApplicationUserToUserInfo user))
            }
    
    let updateUserEmailAddress (userManager: UserManager<ApplicationUser>) : UpdateUserEmailAddress =
        fun (applicationUser, emailAddress) ->
            async {
                let! applicationUserUpdated = CommandRepository.updateUserEmail userManager (applicationUser, emailAddress.Value)
                return applicationUserUpdated |> Result.map EntityToDomainMapping.mapApplicationUserToUserInfo
            }
            
    let updateUserUsername (userManager: UserManager<ApplicationUser>) : UpdateUserUsername =
        fun (applicationUser, username) ->
            async {
                let! applicationUserUpdated = CommandRepository.updateUserUsername userManager (applicationUser, username.Value)
                return applicationUserUpdated |> Result.map EntityToDomainMapping.mapApplicationUserToUserInfo
            }
            
    let updateUserInfo (userManager: UserManager<ApplicationUser>) : UpdateUserInfo =
        fun applicationUser ->
            async {
                let! _ = CommandRepository.updateUserInfo userManager applicationUser
                return Ok ()
            }
            
    let getUserFollowing (dbContext: ApplicationDbContext) : GetUserFollowing =
        fun userId ->
            async {
                let! entity = QueryRepository.getUserFollowing dbContext userId.Value
                return entity |> Result.map EntityToDomainMapping.mapUserFollowing
            }
            
    let addUserFollowing (dbContext: ApplicationDbContext) : AddUserFollowing =
        fun (followerId, followedId) ->
            asyncResult {
                let userFollowing = UserFollowingEntity(FollowerId = followerId.Value, FollowedId = followedId.Value)
                do! CommandRepository.addUserFollowing dbContext userFollowing
            }
            
    let removeUserFollowing (dbContext: ApplicationDbContext) : RemoveUserFollowing =
        fun (followerId, followedId) ->
            asyncResult {
                let userFollowing = UserFollowingEntity(FollowerId = followerId.Value, FollowedId = followedId.Value)
                do! CommandRepository.removeUserFollowing dbContext userFollowing
            }
            
    let addArticle (dbContext: ApplicationDbContext) : AddArticle =
        fun article ->
            asyncResult {
                let entity = article |> DomainToEntityMapping.mapArticleToEntity
                do! CommandRepository.addArticle dbContext entity
            }
            
    let updateArticle (dbContext: ApplicationDbContext) : UpdateArticle =
        fun article ->
            asyncResult {
                let entity = article |> DomainToEntityMapping.mapArticleToEntity
                do! CommandRepository.updateArticle dbContext entity
            }
            
    let getArticle (dbContext: ApplicationDbContext) : GetArticle =
        fun slug ->
            async {
                let! result = QueryRepository.getArticle dbContext slug.Value
                return result |> Option.map (EntityToDomainMapping.mapArticle) 
            }            
           
    let deleteArticle (dbContext: ApplicationDbContext) : DeleteArticle =
        fun article ->
            asyncResult {
                let entity = article |> DomainToEntityMapping.mapArticleToEntity
                do! CommandRepository.deleteArticle dbContext entity
            }
            
    let addComment (dbContext: ApplicationDbContext) : AddComment =
        fun comment ->
            asyncResult {
                let entity = comment |> DomainToEntityMapping.mapCommentToEntity
                do! CommandRepository.addComment dbContext entity
            }
            
    let getComment (dbContext: ApplicationDbContext) : GetComment =
        fun commentId ->
            async {
                let! entity = QueryRepository.getComment dbContext (commentId.ToString())
                return entity |> Option.map (EntityToDomainMapping.mapCommentEntity)
            }
            
    let deleteComment (dbContext: ApplicationDbContext) : DeleteComment =
        fun comment ->
            asyncResult {
                let entity = comment |> DomainToEntityMapping.mapCommentToEntity
                do! CommandRepository.deleteComment dbContext entity
            }

    let getFavoriteArticles (dbContext: ApplicationDbContext) : GetFavoriteArticles =
        fun userId ->
            async {
                let! entity = QueryRepository.getFavoriteArticles dbContext userId.Value
                return entity |> Result.map EntityToDomainMapping.mapFavoriteArticles
            }
            
    let addFavoriteArticle (dbContext: ApplicationDbContext) : AddFavoriteArticle =
        fun (userId, article) ->
            asyncResult {
                let favoriteArticle = FavoriteArticleEntity(UserId = userId.Value, ArticleId = article.Id.ToString())
                do! CommandRepository.addFavoriteArticle dbContext favoriteArticle
            }
            
    let removeFavoriteArticle (dbContext: ApplicationDbContext) : RemoveFavoriteArticle =
        fun (userId, article) ->
            asyncResult {
                let favoriteArticle = FavoriteArticleEntity(UserId = userId.Value, ArticleId = article.Id.ToString())
                do! CommandRepository.removeFavoriteArticle dbContext favoriteArticle
            }