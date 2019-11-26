namespace RealWorldFSharp.Data

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
    type UserFollowing = {
        FollowerId: string
        FollowedId: string
    }
    
    type ApplicationDbContext(options:DbContextOptions<ApplicationDbContext>) =
        inherit IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
        
        [<DefaultValue>]
        val mutable usersFollowing: DbSet<UserFollowing>
                
        override x.OnModelCreating(modelBuilder: ModelBuilder) =
            base.OnModelCreating modelBuilder
            modelBuilder.Entity<UserFollowing>().HasKey("FollowerId", "FollowedId") |> ignore
        
        member x.UsersFollowing
            with get() = x.usersFollowing
            and set v = x.usersFollowing <- v


    type EmailAddress = string
    type Password = string
    type Username = string
    
    type UserId = string
    
    type UserFollowingEntity = {
        Id: string
        Following: string list
    }
    