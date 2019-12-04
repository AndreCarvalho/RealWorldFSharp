namespace RealWorldFSharp.Data

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore

module DataEntities =
    [<AllowNullLiteral>]
    type ApplicationUser() =
        inherit IdentityUser()
        member val Bio:string = null with get, set
        member val ImageUrl:string = null with get, set
        
    [<CLIMutable>]
    type UserFollowingEntity = {
        FollowerId: string
        FollowedId: string
    }

    [<AllowNullLiteral>]
    type ArticleTagEntity() =
        [<Key>]
        member val Id: int = 0 with get, set
        member val ArticleId: string = null with get, set
        member val Tag: string = null with get, set
    and
        [<AllowNullLiteral>]
        ArticleEntity() =
            [<Key>]
            member val Id: string = null with get, set
            member val Title: string = null with get, set
            member val Slug: string = null with get, set
            member val Description: string = null with get, set
            member val Body: string = null with get, set
            member val UserId: string = null with get, set
            member val CreatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
            member val UpdatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
            member val Tags: List<ArticleTagEntity> = null with get, set
            
    [<AllowNullLiteral>]
    type ArticleCommentEntity() =
        [<Key>]
        member val Id: string = null with get, set
        member val Body: string = null with get, set
        member val ArticleId: string = null with get, set
        member val UserId: string = null with get, set
        member val CreatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val UpdatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
        
    [<CLIMutable>]
    [<Table("ArticlesFavorited")>]
    type FavoriteArticleEntity = {
        UserId: string
        ArticleId: string
    }
    
    type ApplicationDbContext(options:DbContextOptions<ApplicationDbContext>) =
        inherit IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
        
        [<DefaultValue>] val mutable usersFollowing: DbSet<UserFollowingEntity>
        [<DefaultValue>] val mutable articles: DbSet<ArticleEntity>        
        [<DefaultValue>] val mutable articleTags: DbSet<ArticleTagEntity>
        [<DefaultValue>] val mutable articleComments: DbSet<ArticleCommentEntity>
        [<DefaultValue>] val mutable favoriteArticles: DbSet<FavoriteArticleEntity>
                
        override x.OnModelCreating(modelBuilder: ModelBuilder) =
            base.OnModelCreating modelBuilder
            modelBuilder.Entity<UserFollowingEntity>().HasKey("FollowerId", "FollowedId") |> ignore
            modelBuilder.Entity<FavoriteArticleEntity>().HasKey("UserId", "ArticleId") |> ignore
            
            modelBuilder.Entity<ArticleTagEntity>().HasOne<ArticleEntity>().WithMany().HasForeignKey("ArticleId") |> ignore

            modelBuilder.Entity<ArticleEntity>().HasIndex("Slug").IsUnique() |> ignore
            modelBuilder.Entity<ArticleEntity>().HasMany("Tags").WithOne().HasForeignKey("ArticleId") |> ignore
        
        member x.UsersFollowing
            with get() = x.usersFollowing
            and set v = x.usersFollowing <- v        
        member x.Articles
            with get() = x.articles
            and set v = x.articles <- v
        member x.ArticleTags
            with get() = x.articleTags
            and set v = x.articleTags <- v
        member x.ArticleComments
            with get() = x.articleComments
            and set v = x.articleComments <- v
        
        member x.FavoriteArticles
            with get() = x.favoriteArticles
            and set v = x.favoriteArticles <- v

    type EmailAddress = string
    type Password = string
    type Username = string
    
    type UserId = string
    
    type ArticleId = string
    type Slug = string
    type CommentId = string
    
    type UserFollowingsEntity = {
        Id: string
        Following: string list
    }    
    type FavoriteArticlesEntity = {
        UserId: string
        Favorites: string list
    }
    