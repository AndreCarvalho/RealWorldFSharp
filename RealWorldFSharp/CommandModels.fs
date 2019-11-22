namespace RealWorldFSharp

module CommandModels =
    
    [<CLIMutable>]
    type NewUserInfo = {
        Username: string
        Email: string
        Password: string
    }
    
    [<CLIMutable>]
    type RegisterNewUser = {
        User: NewUserInfo
    }
