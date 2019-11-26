namespace RealWorldFSharp

open System
open System.Collections.Generic
open RealWorldFSharp.Domain
open RealWorldFSharp.Common

module QueryModels =
    
    [<CLIMutable>]
    type Errors = {
        Errors: Dictionary<string, string array>
    }
    
    [<CLIMutable>]
    type UserModel = {
        Username: string
        Email: string
        Token: string
        Bio: string
        Image: string
    }
    
    [<CLIMutable>]
    type UserModelEnvelope = {
        User: UserModel
    }
    
    [<CLIMutableAttribute>]
    type ProfileModel = {
        Username: string
        Bio: string
        Image: string
        Following: Nullable<bool>
    }
    
    [<CLIMutable>]
    type ProfileModelEnvelope = {
        Profile: ProfileModel
    }

    let toUserModelEnvelope token (userInfo: UserInfo) =
        {
            User = {
                Username = userInfo.Username.Value
                Email = userInfo.EmailAddress.Value
                Bio = userInfo.Bio |> Option.defaultValue null
                Image = userInfo.Image |> Option.defaultValue null
                Token = token
            }
        }
        
    let toSimpleProfileModelEnvelope (userInfo: UserInfo) =
        {
            Profile = {
                Username = userInfo.Username.Value
                Bio = userInfo.Bio |> Option.defaultValue null
                Image = userInfo.Image |> Option.defaultValue null
                Following = Nullable.empty<bool> 
            }
        }
        
    let toProfileModelEnvelope (userFollowing: UserFollowing) (userInfo: UserInfo)  =
        let envelope = toSimpleProfileModelEnvelope userInfo
        { envelope with Profile = { envelope.Profile with Following = userFollowing.Following.Contains userInfo.Id |> Nullable.from }}
