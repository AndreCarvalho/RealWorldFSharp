namespace RealWorldFSharp.Api.Authentication

open System
open RealWorldFSharp.Api.Domain.Users
open RealWorldFSharp.Api.Settings
open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

module Authentication =
    
    let createToken (config: JwtConfiguration) (user: User) =
        let tokenHandler = new JwtSecurityTokenHandler()
        let key = Encoding.ASCII.GetBytes config.Secret
        
        let jwToken = new JwtSecurityToken(
                                              claims = [
                                                  new Claim(ClaimTypes.Email, user.EmailAddress.Value);
                                                  new Claim(ClaimTypes.Name, user.Username.Value);
                                                  new Claim(ClaimTypes.NameIdentifier, user.Id.Value);
                                              ],
                                              issuer = config.Issuer,
                                              audience = config.Audience,
                                              expires = System.Nullable(DateTime.UtcNow.AddDays(float 1)),
                                              signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                                          )
        tokenHandler.WriteToken(jwToken)