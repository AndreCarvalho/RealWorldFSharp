namespace RealWorldFSharp.Api
open System.Collections.Generic
open System.Collections.ObjectModel

module Models =
    module Request = 
        
        [<CLIMutable>]
        type AuthenticationData = {
            Email: string
            Password: string
        }        
        
        [<CLIMutable>]
        type AuthenticateUser = {
            User: AuthenticationData
        }
        
        [<CLIMutable>]
        type UpdateUserData = {
            Username: string
            Email: string
            Password: string
            Image: string
            Bio: string
        }
        
        [<CLIMutable>]
        type UpdateUser = {
            User: UpdateUserData
        }
        
    module Response =
        
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

        [<CLIMutableAttribute>]
        type Profile = {
            Username: string
            Bio: string
            Image: string
            Following: bool
        }
        
        [<CLIMutable>]
        type ProfileResponse = {
            Profile: Profile
        }
        
        
        