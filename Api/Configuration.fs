namespace Api

module Settings =

    [<CLIMutable>]   
    type JwtConfiguration =
        {
            Secret: string
            Audience: string
            Issuer: string
        }
