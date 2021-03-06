namespace RealWorldFSharp.Api.Authentication

open System
open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open RealWorldFSharp.Domain.Users
open RealWorldFSharp.Api.Settings

module Authentication =
    
    let createToken (config: JwtConfiguration) (userInfo: UserInfo) =
        let tokenHandler = new JwtSecurityTokenHandler()
        let key = Encoding.ASCII.GetBytes config.Secret
        
        let jwToken = new JwtSecurityToken(
                                              claims = [
                                                  new Claim(ClaimTypes.Email, userInfo.EmailAddress.Value);
                                                  new Claim(ClaimTypes.Name, userInfo.Username.Value);
                                                  new Claim(ClaimTypes.NameIdentifier, userInfo.Id.Value);
                                              ],
                                              issuer = config.Issuer,
                                              audience = config.Audience,
                                              expires = System.Nullable(DateTime.UtcNow.AddDays(float 1)),
                                              signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                          )
        tokenHandler.WriteToken(jwToken)