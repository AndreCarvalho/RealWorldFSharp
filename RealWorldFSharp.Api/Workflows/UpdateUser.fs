namespace RealWorldFSharp.Api.Workflows

open RealWorldFSharp.Api
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Identity
open FsToolkit.ErrorHandling
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Api.QueryModels
open RealWorldFSharp.Api.Authentication
open RealWorldFSharp.Api.Settings
open RealWorldFSharp.Common.Errors
open RealWorldFSharp.Api.CommandModels
open RealWorldFSharp.Data.Write
open RealWorldFSharp.Data.Write.DataEntities

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
    
    member __.Execute(userId, commandModel: UpdateUserCommandModel) =
        asyncResult {
            let! command = CommandModels.validateUpdateUserCommand commandModel |> expectValidationError
            let userId = UserId.create "userId" userId |> valueOrException

            let! userInfoOption = DataPipeline.getUserInfoByUserIdForUpdate userManager userId
            let! (userInfo, applicationUser) = noneToUserNotFoundError userInfoOption userId.Value |> expectUsersError
            
            let! userInfo = tryUpdateUsername command userInfo applicationUser |> expectUsersErrorAsync
            let! userInfo = tryUpdateEmailAddress command userInfo applicationUser |> expectUsersErrorAsync
            let! userInfo = tryUpdateUser command userInfo applicationUser |> expectUsersErrorAsync
            
            let token = Authentication.createToken jwtOptions.Value userInfo
            return userInfo |> toUserModelEnvelope token
        }    