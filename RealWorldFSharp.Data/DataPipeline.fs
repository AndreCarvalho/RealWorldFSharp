namespace RealWorldFSharp.Data

open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Identity
open DataEntities
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

module DataPipeline =
    
    type RegisterNewUser = UserInfo * Password -> UserIdentityResult<unit>
    type AuthenticateUser = EmailAddress * Password -> UserIdentityResult<UserInfo>
    
    let registerNewUser (userManager: UserManager<ApplicationUser>) : RegisterNewUser =
        fun (userInfo, password) ->
            let applicationUser = userInfo |> DomainToEntityMapping.mapUserInfoToApplicationUser
            
            asyncResult {
                do! CommandRepository.registerNewUser userManager (applicationUser, password.Value)
            }
            
    let authenticateUser (userManager: UserManager<ApplicationUser>) : AuthenticateUser =
        fun (emailAddress, password) ->
            async {
                let! userInfo = CommandRepository.authenticateUser userManager (emailAddress.Value, password.Value)
                return userInfo |> Result.map EntityToDomainMapping.mapApplicationUserToUserInfo
            }
