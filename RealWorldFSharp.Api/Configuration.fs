namespace RealWorldFSharp.Api

module Settings =

    [<CLIMutable>]   
    type JwtConfiguration =
        {
            Secret: string
            Audience: string
            Issuer: string
        }
        
    [<CLIMutable>]   
    type Database = {
        ConnectionString: string
    }
        
    
