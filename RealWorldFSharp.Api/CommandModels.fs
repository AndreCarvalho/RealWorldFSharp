namespace RealWorldFSharp.Api

open FsToolkit.ErrorHandling
open RealWorldFSharp.Domain
open RealWorldFSharp.Articles.Domain
open RealWorldFSharp.Common
open RealWorldFSharp.Common.Errors

module CommandModels =
   
    [<CLIMutable>]
    type RegisterNewUserCommandModel = {
        Username: string
        Email: string
        Password: string
    }
    
    [<CLIMutable>]
    type RegisterNewUserCommandModelEnvelope = {
        User: RegisterNewUserCommandModel
    }
    
    [<CLIMutable>]
    type AuthenticateUserCommandModel = {
        Email: string
        Password: string
    }        
    
    [<CLIMutable>]
    type AuthenticateUserCommandModelEnvelope = {
        User: AuthenticateUserCommandModel
    }
    
    [<CLIMutable>]
    type UpdateUserCommandModel = {
        Bio: string
        Image: string
        Email: string
        Username: string
    }
    
    [<CLIMutable>]
    type UpdateUserCommandModelEnvelope = {
        User: UpdateUserCommandModel
    }
    
    [<CLIMutable>]
    type CreateArticleCommandModel = {
         Title: string
         Description: string
         Body: string
         TagList: string array
    }
    
    [<CLIMutable>]
    type CreateArticleCommandModelEnvelope = {
         Article: CreateArticleCommandModel
    }
    
    [<CLIMutable>]
    type UpdateArticleCommandModel = {
         Title: string
         Description: string
         Body: string
    }
    
    [<CLIMutable>]
    type UpdateArticleCommandModelEnvelope = {
         Article: UpdateArticleCommandModel
    }
    
    [<CLIMutable>]
    type AddCommentToArticleCommandModel = {
        Body: string
    }
        
    [<CLIMutable>]
    type AddCommentToArticleCommandModelEnvelope = {
        Comment: AddCommentToArticleCommandModel
    }
            
    type AuthenticateUserCommand = {
        EmailAddress: EmailAddress
        Password: Password
    }
    
    type UpdateUserCommand = {
        Bio: Bio option
        Image: Image option
        EmailAddress: EmailAddress option
        Username: Username option
    }
    
    type CreateArticleCommand = {
        Title: Title
        Description: Description
        Body: ArticleBody
        Tags: Tag list
    }
    
    type UpdateArticleCommand = {
        Title: Title option
        Description: Description option
        Body: ArticleBody option
    }
    
    type AddCommentToArticleCommand = {
        Body: CommentBody
    }
    
    type ValidateRegisterNewUserCommand = RegisterNewUserCommandModel -> ValidationResult<UserInfo * Password>
    type ValidateAuthenticateUserCommand = AuthenticateUserCommandModel -> ValidationResult<AuthenticateUserCommand>
    type ValidateUpdateUserCommand = UpdateUserCommandModel -> ValidationResult<UpdateUserCommand>
    type ValidateCreateArticleCommand = CreateArticleCommandModel -> ValidationResult<CreateArticleCommand>
    type ValidateUpdateArticleCommand = UpdateArticleCommandModel -> ValidationResult<UpdateArticleCommand>
    type ValidateAddCommentToArticleCommand = AddCommentToArticleCommandModel -> ValidationResult<AddCommentToArticleCommand>

                
    let private validateOptional validation value =
        value |> Option.ofObj |> Option.map validation |> Option.sequenceResult     

    let validateRegisterNewUserCommand userId : ValidateRegisterNewUserCommand =
        fun command ->
            result {
                let! userId = UserId.create "userId" userId
                let! userName = Username.create "username" command.Username
                let! emailAddress = EmailAddress.create "email" command.Email
                let! password = Password.create "password" command.Password
                
                return ({
                    Username = userName
                    EmailAddress = emailAddress
                    Id = userId
                    Bio = None
                    Image = None
                }, password)
            }
            
    let validateAuthenticateUserCommand : ValidateAuthenticateUserCommand =
        fun command ->
            result {
                let! emailAddress = EmailAddress.create "email" command.Email
                let! password = Password.create "password" command.Password
                
                return {
                    EmailAddress = emailAddress
                    Password = password
                }
            }            
            
    let validateUpdateUserCommand: ValidateUpdateUserCommand =
        fun command ->
            result {
                let! emailAddress = command.Email |> validateOptional (EmailAddress.create "email")
                let! username = command.Username |> validateOptional (Username.create "username")
                let bio = command.Bio |> Option.ofObj
                let image = command.Image |> Option.ofObj
                
                return {
                    Bio = bio
                    Image = image
                    EmailAddress = emailAddress
                    Username = username
                } 
            }
            
    let validateCreateArticleCommand : ValidateCreateArticleCommand =
        fun command ->
            result {
                let! title = Title.create "title" command.Title 
                let! description = Description.create "description" command.Description 
                let! body = ArticleBody.create "body" command.Body
                
                let tags = if (isNull command.TagList) then [||] else command.TagList
                let! validatedTags = tags |> Array.map (Tag.create "tag") |> List.ofArray |> Result.combine
                
                return {
                    Title = title
                    Description = description
                    Body = body
                    Tags = validatedTags
                }
            }
            
    let validateUpdateArticleCommand : ValidateUpdateArticleCommand =
        fun command ->
            result {
                let! title = command.Title |> validateOptional (Title.create "title")
                let! description = command.Description |> validateOptional (Description.create "description") 
                let! body = command.Body |> validateOptional (ArticleBody.create "body")

                return {
                    Title = title
                    Description = description
                    Body = body
                }
            }
            
    let validateAddCommentToArticleCommand : ValidateAddCommentToArticleCommand =
        fun command ->
            result {
                let! body = command.Body |> CommentBody.create "body"
                
                return {
                    Body = body
                }
            }