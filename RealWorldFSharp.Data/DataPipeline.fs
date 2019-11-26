namespace RealWorldFSharp.Data

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

module DataPipeline =
    
    type RegisterNewUser = UserInfo * Password -> UserIdentityResult<unit>
    type AuthenticateUser = EmailAddress * Password -> UserIdentityResult<UserInfo>
    type GetUserInfo = Username -> IoQueryResult<(UserInfo * ApplicationUser)>
    type UpdateUserEmailAddress = (ApplicationUser * EmailAddress) -> UserIdentityResult<UserInfo>
    type UpdateUserUsername = (ApplicationUser * Username) -> UserIdentityResult<UserInfo>
    type UpdateUserInfo = ApplicationUser -> UserIdentityResult<unit>
    
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
            
    let getUserInfo (userManager: UserManager<ApplicationUser>) : GetUserInfo =
        fun (username) ->
            async {
                let! applicationUser = QueryRepository.getApplicationUser userManager username.Value
                
                return
                    match applicationUser with
                    | Some user -> Some (EntityToDomainMapping.mapApplicationUserToUserInfo user, user)
                    | None -> None
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