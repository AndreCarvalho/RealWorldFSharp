namespace RealWorldFSharp.Data

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open System.Threading
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore

module ReadModels =
    
    [<Table("AspNetUsers")>]
    [<AllowNullLiteral>]
    type UserEntity() =
        [<Key>]
        member val Id: string = null with get, set
        member val Username: string = null with get, set
        member val Bio: string = null with get, set
        member val ImageUrl: string = null with get, set
    
    
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
    type ArticleTagEntity() =
        [<Key>]
        member val Id: int = -1 with get, set
        member val Tag: string = null with get, set
        

    // A DataContext optimized for queries/reading 
    type ReadDataContext(options:DbContextOptions<ReadDataContext>) =
        inherit DbContext(options)
        
        [<DefaultValue>] val mutable articleComments: DbSet<ArticleCommentEntity>
        [<DefaultValue>] val mutable articleTags: DbSet<ArticleTagEntity>
        
        override x.OnModelCreating(modelBuilder: ModelBuilder) =
            base.OnModelCreating modelBuilder
        
        override x.SaveChangesAsync(_: bool, _: CancellationToken) =
            //Prevent writing with this context
            Task.FromResult -1
            
        override x.SaveChanges(_: bool) =
            //Prevent writing with this context            
            1
            
        member x.ArticleComments
            with get() = x.articleComments
            and set v = x.articleComments <- v
            
        member x.ArticleTags
            with get() = x.articleTags
            and set v = x.articleTags <- v

