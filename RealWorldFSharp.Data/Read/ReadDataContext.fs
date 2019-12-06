namespace RealWorldFSharp.Data.Read

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Threading
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open System.Collections.Generic

module ReadModels =
    
    [<Table("AspNetUsers")>]
    [<AllowNullLiteral>]
    type UserEntity() =
        [<Key>]
        member val Id: string = null with get, set
        member val Username: string = null with get, set
        member val Bio: string = null with get, set
        member val ImageUrl: string = null with get, set
    
    [<CLIMutable>]
    type UserFollowingEntity = {
        FollowerId: string
        FollowedId: string
    }
    
    [<Table("ArticleComments")>]
    type ArticleCommentEntity() =
        [<Key>]
        member val Id: string = null with get, set
        member val Body: string = null with get, set
        member val ArticleId: string = null with get, set
        member val UserId: string = null with get, set
        member val CreatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val UpdatedAt: DateTimeOffset = DateTimeOffset.MinValue with get, set
        member val User: UserEntity = null with get, set
        
    [<Table("ArticleTags")>]
    [<AllowNullLiteral>]
    type ArticleTagEntity() =
        [<Key>]
        member val Id: int = -1 with get, set
        member val Tag: string = null with get, set
        
    [<CLIMutable>]
    [<Table("ArticlesFavorited")>]
    type FavoriteArticleEntity = {
        UserId: string
        ArticleId: string
    }
        
    [<AllowNullLiteral>]
    [<Table("Articles")>]
    type ArticleEntity() =
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
        member val Favorited: List<FavoriteArticleEntity> = null with get, set
        member val User: UserEntity = null with get, set
    
    // A DataContext optimized for queries/reading 
    type ReadDataContext(options:DbContextOptions<ReadDataContext>) =
        inherit DbContext(options)
        
        [<DefaultValue>] val mutable articleComments: DbSet<ArticleCommentEntity>
        [<DefaultValue>] val mutable articleTags: DbSet<ArticleTagEntity>
        [<DefaultValue>] val mutable articles: DbSet<ArticleEntity>
        [<DefaultValue>] val mutable users: DbSet<UserEntity>
        [<DefaultValue>] val mutable usersFollowing: DbSet<UserFollowingEntity>
        
        override x.OnModelCreating(modelBuilder: ModelBuilder) =
            base.OnModelCreating modelBuilder
            modelBuilder.Entity<ArticleEntity>().HasMany("Tags").WithOne().HasForeignKey("ArticleId") |> ignore
            modelBuilder.Entity<ArticleEntity>().HasMany("Favorited").WithOne().HasForeignKey("ArticleId") |> ignore
            modelBuilder.Entity<FavoriteArticleEntity>().HasKey("UserId", "ArticleId") |> ignore
            modelBuilder.Entity<UserFollowingEntity>().HasKey("FollowerId", "FollowedId") |> ignore

        override x.SaveChangesAsync(_: bool, _: CancellationToken) =
            //Prevent writing with this context
            Task.FromResult -1
            
        override x.SaveChanges(_: bool) =
            //Prevent writing with this context            
            -1
            
        member x.ArticleComments
            with get() = x.articleComments
            and set v = x.articleComments <- v
            
        member x.ArticleTags
            with get() = x.articleTags
            and set v = x.articleTags <- v
            
        member x.Articles
            with get() = x.articles
            and set v = x.articles <- v
            
        member x.Users
            with get() = x.users
            and set v = x.users <- v            
        
        member x.UsersFollowing
            with get() = x.usersFollowing
            and set v = x.usersFollowing <- v

