namespace RealWorldFSharp

open System.Collections.Generic
open RealWorldFSharp.Domain

module QueryModels =
    
    [<CLIMutable>]
    type Errors = {
        Errors: Dictionary<string, string array>
    }
    
    [<CLIMutable>]
    type User = {
        Username: string
        Email: string
        Token: string
        Bio: string
        Image: string
    }
    
    [<CLIMutable>]
    type UserResponse = {
        User: User
    }

    let toUserResponse token (userInfo: UserInfo) =
        {
            Username = userInfo.Username.Value
            Email = userInfo.EmailAddress.Value
            Bio = userInfo.Bio |> Option.defaultValue null
            Image = userInfo.Image |> Option.defaultValue null
            Token = token
        }
