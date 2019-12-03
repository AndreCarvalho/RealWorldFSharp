namespace RealWorldFSharp.Api

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open Settings
open RealWorldFSharp.Api.Workflows
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open RealWorldFSharp.Data.DataEntities

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration          

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddDbContext<ApplicationDbContext>(fun opt -> opt.UseInMemoryDatabase("InMemoryDb") |> ignore) |> ignore
        
        services.AddIdentity<ApplicationUser, IdentityRole>(fun opt ->
                opt.User.RequireUniqueEmail <- true 
                opt.Password.RequireNonAlphanumeric <- false 
                opt.Password.RequireDigit <- false 
                opt.Password.RequireUppercase <- false
                opt.Password.RequireLowercase <- false
            )
            .AddEntityFrameworkStores<ApplicationDbContext>() |> ignore
        
        services.AddControllers() |> ignore
        
        services.AddTransient<RegisterNewUserWorkflow>() |> ignore
        services.AddTransient<AuthenticateUserWorkflow>() |> ignore
        services.AddTransient<RetrieveUserWorkflow>() |> ignore
        services.AddTransient<UpdateUserWorkflow>() |> ignore
        services.AddTransient<RetrieveProfileWorkflow>() |> ignore
        services.AddTransient<FollowUserWorkflow>() |> ignore
        services.AddTransient<UnfollowUserWorkflow>() |> ignore
        services.AddTransient<CreateArticleWorkflow>() |> ignore
        services.AddTransient<GetArticleWorkflow>() |> ignore
        services.AddTransient<UpdateArticleWorkflow>() |> ignore
        services.AddTransient<DeleteArticleWorkflow>() |> ignore
        services.AddTransient<AddCommentWorkflow>() |> ignore
        services.AddTransient<GetCommentsWorkflow>() |> ignore
        
        let appSettingsSection = this.Configuration.GetSection "JwtConfiguration"
        services.Configure<JwtConfiguration> appSettingsSection |> ignore
        
        let jwtConfiguration = appSettingsSection.Get<JwtConfiguration>()
        let key = Encoding.ASCII.GetBytes jwtConfiguration.Secret
        let signinKey = new SymmetricSecurityKey(key)
        
        let tokenValidationParameters =
            new TokenValidationParameters(
                ValidateIssuer = true,
                ValidIssuer = jwtConfiguration.Issuer,
                
                ValidateAudience = true,
                ValidAudience = jwtConfiguration.Audience,
                
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signinKey,
                
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero)
        
        services.AddAuthentication(fun opt ->
            opt.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
            opt.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun opt ->
                opt.Events <- new JwtBearerEvents(OnMessageReceived =
                    fun (context) ->
                        let token = context.HttpContext.Request.Headers.["Authorization"]
                        if token.Count > 0 && token.[0].StartsWith("Token", StringComparison.OrdinalIgnoreCase) then
                            context.Token <- token.[0].Substring("Token ".Length).Trim()
                            
                        Task.CompletedTask
                    )
                opt.SaveToken <- true
                opt.TokenValidationParameters <- tokenValidationParameters
                ) |> ignore
            
        services.AddAuthorization() |> ignore
        

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseHttpsRedirection() |> ignore
        
        app.UseRouting() |> ignore

        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

        app.UseEndpoints(fun endpoints -> 
            endpoints.MapControllers() |> ignore
            ) |> ignore

    member val Configuration : IConfiguration = null with get, set
