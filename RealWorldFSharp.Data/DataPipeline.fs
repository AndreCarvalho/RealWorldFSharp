namespace RealWorldFSharp.Data

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

module DataPipeline =
    
    type RegisterNewUser = UserInfo * Password -> UserIdentityResult<unit>
    type AuthenticateUser = EmailAddress * Password -> UserIdentityResult<UserInfo>
    type GetUserInfoByUsername = Username -> IoQueryResult<(UserInfo * ApplicationUser)>
    type GetUserInfoById = UserId -> IoQueryResult<(UserInfo * ApplicationUser)>
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
                    |> Option.map (fun user -> (EntityToDomainMapping.mapApplicationUserToUserInfo user, user))
            }
                
    let getUserInfoById (userManager: UserManager<ApplicationUser>) : GetUserInfoById =
        fun userId ->
            async {
                let! applicationUser = QueryRepository.getApplicationUserById userManager userId.Value
                
                return
                    applicationUser
                    |> Option.map (fun user -> (EntityToDomainMapping.mapApplicationUserToUserInfo user, user))
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
                let userFollowing = {FollowerId = followerId.Value; FollowedId = followedId.Value}
                do! CommandRepository.addUserFollowing dbContext userFollowing
            }
            
    let removeUserFollowing (dbContext: ApplicationDbContext) : RemoveUserFollowing =
        fun (followerId, followedId) ->
            asyncResult {
                let userFollowing = {FollowerId = followerId.Value; FollowedId = followedId.Value}
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