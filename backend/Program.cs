using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using backend.Data.Abstractions;
using backend.Data.Data;
using backend.Data.Entities;
using backend.Data.Enums;
using backend.SeedData;
using backend.SeedData.Seeders;
using backend.Services.Model.Auth;
using backend.Services.Model.Files;
using backend.Services.Model.SeedData;
using backend.Services.Services.Auth;
using backend.Services.Services.Files;
using backend.Services.Services.SeedData;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string FrontendCorsPolicy = "FrontendCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- configuration
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"The '{JwtOptions.SectionName}' configuration section is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it with 'dotnet user-secrets set \"Jwt:SigningKey\" \"<key>\"' " +
        "in development, or an environment variable in production.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SeedDataOptions>(builder.Configuration.GetSection(SeedDataOptions.SectionName));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.SectionName));

// ---------------------------------------------------------------- persistence
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------- identity
// AddIdentityCore rather than AddIdentity: this is a token API, and AddIdentity
// would register cookie schemes that compete with JWT as the default.
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ---------------------------------------------------------------- authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------- application services
// Every service in the solution is registered here. The class libraries stay free
// of DI wiring so there is exactly one place to look when a dependency is missing.
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// ---------------------------------------------------------------- file storage
// The one switch: FileStorage:Provider decides which IFileStorage backs uploads.
// Adding Azure means writing AzureFileStorage in backend.Services/Services/Files/
// and adding one arm here — nothing that consumes IFileUploadService changes.
var fileStorageProvider = builder.Configuration
    .GetSection(FileStorageOptions.SectionName)
    .Get<FileStorageOptions>()?.Provider ?? FileStorageProvider.Local;

switch (fileStorageProvider)
{
    case FileStorageProvider.Local:
        builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
        break;

    default:
        // Fail at startup rather than at the first upload, and say what is missing.
        throw new InvalidOperationException(
            $"{FileStorageOptions.SectionName}:Provider is set to '{fileStorageProvider}', which has no " +
            "IFileStorage implementation registered. Implement it in " +
            "backend.Services/Services/Files/ and add it to the switch in Program.cs.");
}

// ---------------------------------------------------------------- seeders
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<DatabaseSeeder>();

// ---------------------------------------------------------------- web
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums travel as strings on the wire; the C# enum stays the source of truth.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---------------------------------------------------------------- swagger
// No AddEndpointsApiExplorer here: that one discovers minimal-API endpoints, and
// this app is entirely controller-based, where AddControllers already supplies the
// API explorer Swashbuckle reads.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DeadOrAlive API",
        Version = "v1",
        Description = "Uptime and heartbeat monitoring for websites, APIs and background jobs.",
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the accessToken returned by /api/auth/login. The 'Bearer ' prefix is added for you.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    // The requirement is built per-document because the scheme reference has to be
    // resolved against the document that declares it.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document), new List<string>() },
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ---------------------------------------------------------------- mapping
TypeAdapterConfig.GlobalSettings.Scan(
    typeof(Program).Assembly,
    typeof(AuthService).Assembly);

var app = builder.Build();

// ---------------------------------------------------------------- pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DeadOrAlive API v1");
        options.DocumentTitle = "DeadOrAlive API";
    });

    // Seeding must not stop the host: on a database that has not been migrated yet
    // the API should still start so Swagger is reachable and the error is visible.
    try
    {
        // The seeders depend on scoped services, which the root provider cannot supply.
        using var scope = app.Services.CreateScope();
        var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await databaseSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(
            ex,
            "Database seeding failed. The API has still started — create the database and apply migrations, then restart.");
    }
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
