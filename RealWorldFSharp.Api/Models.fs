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
        
        
        