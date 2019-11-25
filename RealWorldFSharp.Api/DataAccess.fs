namespace RealWorldFSharp.Api

open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore
open RealWorldFSharp.Data.DataEntities

module DataAccess =
    
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
    
