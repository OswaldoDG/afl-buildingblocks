namespace CloudReportsBuildingBlocksPOC;

using CloudReportsBuildingBlocksPOC.BackgroundServices;
using CloudReportsBuildingBlocksPOC.Configuration;
using CloudReportsBuildingBlocksPOC.GraphQL.Mutations;
using CloudReportsBuildingBlocksPOC.GraphQL.Queries;
using CloudReportsBuildingBlocksPOC.GraphQL.Types;
using CloudReportsBuildingBlocksPOC.Hubs;
using CloudReportsBuildingBlocksPOC.Middleware;
using CloudReportsBuildingBlocksPOC.Repositories;
using CloudReportsBuildingBlocksPOC.Services;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using FluentMigrator.Runner;
using FluentStorage;
using FluentStorage.Blobs;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.Reflection;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables()
                   .AddUserSecrets<Program>();

        // Dapper config for PostgreSQL
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        builder.Services.AddSingleton<DapperContext>();
        builder.Services.AddSingleton<Database>();

        // Add Dapper configuration string.
        builder.Services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString(Database.SorsConnectionName);
            return new NpgsqlConnection(connectionString!);
        });

        builder.Services.AddScoped<IUnitOfWork>(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString(Database.SorsConnectionName);
            return new UnitOfWork(connectionString!);
        });

        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Configure CORS for SignalR
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("SignalRCorsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddScoped<IFileSystemProvider, GcpFileSystemProvider>();
        builder.Services.AddScoped<IFileBatchService, FileBatchService>();
        builder.Services.AddScoped<IOtdrService, OtdrService>();
        UrlSigner urlSigner = UrlSigner.FromCredential(await GoogleCredential.GetApplicationDefaultAsync());
        builder.Services.AddSingleton(urlSigner);
        builder.Services.AddSingleton<IBlobStorage>(sp =>
        {
            return StorageFactory.Blobs.GoogleCloudStorageFromEnvironmentVariable(builder.Configuration.GetValue<string>("GcpStorage:BucketName"));
        });

        builder.Services.AddDistributedMemoryCache();

        string projectId = builder.Configuration.GetValue<string>("Descope:ProjectId")!;
        string authorityUrl = builder.Configuration.GetValue<string>("Descope:AuthorityUrl")!;
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.Authority = $"{authorityUrl.TrimEnd('/')}/{projectId}";
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = authorityUrl,
                   ValidateAudience = true,
                   ValidAudience = projectId,
                   ValidateLifetime = true,
                   RoleClaimType = "roles", // Optional mapping for Descope roles
               };

               options.Events = new JwtBearerEvents
               {
                   OnAuthenticationFailed = context =>
                   {
                       // Cutos login if validation fails (ej. logging)
                       return Task.CompletedTask;
                   },
               };
           });

        // Configure PubSub options
        builder.Services.Configure<PubSubOptions>(
            builder.Configuration.GetSection(PubSubOptions.SectionName));

        // Register GCP Pub/Sub service & listener
        builder.Services.AddSingleton<IPubSubWriterService, GcpPubSubWriterService>();
        builder.Services.AddSingleton<IPubSubListenerService, GcpPubSubListenerService>();
        builder.Services.AddHostedService<FileEventsListenerBackgroundService>();

        // Register SignalR services
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<ISessionConnectionMappingService, SessionConnectionMappingService>();

        builder.Services.AddScoped<IUserContextService, UserContextService>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi(options =>
        {
            options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        });

        // Postgres Migrations with FluentMigrator.
        builder.Services.AddLogging(c => c.AddFluentMigratorConsole())
        .AddFluentMigratorCore()
        .ConfigureRunner(c => c.AddPostgres15_0()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString(Database.SorsConnectionName))
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

        // Graphql configuration.
        builder.Services
                .AddGraphQLServer()
                .AddAuthorization()
                .AddMutationConventions()
                .AddQueryType(d => d.Name("Query"))
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<StorageQueries>()
                .AddTypeExtension<SorsQueries>()
                .AddTypeExtension<SorsMutations>()
                .AddType<BlobEntryType>()
                .AddType<BatchResponseType>()
                .AddType<BatchItemResponseType>()
                .AddType<BatchStatusType>()
                .AddFiltering()
                .AddSorting()
                .AddProjections();

        var app = builder.Build();

        app.MapGraphQL("/graphql");

        // Add the user context middleware.
        app.UseSecurityContext();

        // Execute DB migrations.
        // app.MigrateDatabase();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<BatchNotificationHub>("/hubs/batchnotifications")
            .RequireCors("SignalRPolicy");

        app.MapHealthChecks("/health");

        await app.RunAsync();
    }
}