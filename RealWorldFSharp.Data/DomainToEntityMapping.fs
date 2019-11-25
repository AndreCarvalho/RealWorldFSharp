namespace RealWorldFSharp.Data

open DataEntities
open RealWorldFSharp.Domain

module DomainToEntityMapping =
    
    let mapUserInfoToApplicationUser (userInfo:UserInfo) =
        let x = new ApplicationUser ()
        x.Id <- userInfo.Id.Value
        x.UserName <- userInfo.Username.Value
        x.Email <- userInfo.EmailAddress.Value
        x.Bio <- Option.defaultValue null userInfo.Bio
        x.ImageUrl <- Option.defaultValue null userInfo.Image
        x

