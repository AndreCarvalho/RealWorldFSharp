namespace RealWorldFSharp.Data

open FsToolkit.ErrorHandling
open DataEntities
open RealWorldFSharp.Domain
open RealWorldFSharp.Common.Errors

module EntityToDomainMapping =
        
    let private validateApplicationUser =
        fun (applicationUser: ApplicationUser) ->
            result {
                let! userName = Username.create "username" applicationUser.UserName
                let! emailAddress = EmailAddress.create "email" applicationUser.Email
                let! userId = UserId.create "id" applicationUser.Id
                
                return {
                    Username = userName 
                    EmailAddress = emailAddress
                    Id = userId
                    Bio = Option.ofObj applicationUser.Bio
                    Image = Option.ofObj applicationUser.ImageUrl
                }
            }
    
    let mapApplicationUserToUserInfo (applicationUser: ApplicationUser) : UserInfo =
        validateApplicationUser applicationUser |> valueOrException
        
    let mapUserFollowing (userFollowing:UserFollowingEntity) : UserFollowing =
        let userId = UserId.create "userid" userFollowing.Id |> valueOrException
        let following =
            userFollowing.Following
            |> List.map (UserId.create "userId")
            |> List.map valueOrException
            |> Set.ofList
        
        {
            Id = userId
            Following = following
        }
