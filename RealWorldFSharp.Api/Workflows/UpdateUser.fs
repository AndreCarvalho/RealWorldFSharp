namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp
open RealWorldFSharp.Domain
open RealWorldFSharp.QueryModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.CommandModels
open RealWorldFSharp.Data
open RealWorldFSharp.Data.DataEntities

module UpdateUser =
    
    type UpdateUserWorkflow (
                                    userManager: UserManager<ApplicationUser>,
                                    jwtOptions: IOptions<JwtConfiguration>                                
                            ) =
        let tryUpdateUsername command userInfo applicationUser =
            async {
                return!
                    match command.Username with
                    | Some newUsername -> DataPipeline.updateUserUsername userManager (applicationUser, newUsername)
                    | None -> AsyncResult.retn userInfo
            }
        
        let tryUpdateEmailAddress command userInfo applicationUser =
            async {
                return!
                    match command.EmailAddress with
                    | Some newEmailAddress -> DataPipeline.updateUserEmailAddress userManager (applicationUser, newEmailAddress)
                    | None -> AsyncResult.retn userInfo
            }        
        let tryUpdateUser command userInfo (applicationUser:ApplicationUser) =
            asyncResult {
                let userInfo:UserInfo =
                    match command.Bio with
                    | Some newBio ->
                        applicationUser.Bio <- newBio
                        { userInfo with Bio = Some newBio}
                    | None -> userInfo   
                let userInfo:UserInfo =
                    match command.Image with
                    | Some newImage ->
                        applicationUser.ImageUrl <- newImage
                        { userInfo with Image = Some newImage}
                    | None -> userInfo
                    
                if command.Bio.IsSome || command.Image.IsSome then
                    do! DataPipeline.updateUserInfo userManager applicationUser
                    
                return userInfo
            }
        
        member __.Execute(username, commandModel: UpdateUserCommandModel) =
            asyncResult {
                let! command = CommandModels.validateUpdateUserCommand commandModel |> expectValidationError
                let! username = Username.create "username" username |> expectValidationError

                let! userInfoOption = DataPipeline.getUserInfoByUsername userManager username
                let! (userInfo, applicationUser) = noneToUserNotFoundError userInfoOption username.Value |> expectUsersError
                
                let! userInfo = tryUpdateUsername command userInfo applicationUser |> expectUsersErrorAsync
                let! userInfo = tryUpdateEmailAddress command userInfo applicationUser |> expectUsersErrorAsync
                let! userInfo = tryUpdateUser command userInfo applicationUser |> expectUsersErrorAsync
                
                let token = Authentication.createToken jwtOptions.Value userInfo
                return userInfo |> toUserModelEnvelope token
            }    