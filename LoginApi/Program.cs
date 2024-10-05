using LoginApi.Context;
using LoginApi.Repositories;
using LoginApi.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();

// Load the certificate
var cert = new X509Certificate2("/app/cert.pem");

// Configure Data Protection to use the specified directory
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/app/.aspnet/DataProtection-Keys"))
    .ProtectKeysWithCertificate(cert);

// Add JWT authentication
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration.GetSection("AppSettings:Token").Value!)),  // Using token from configuration
        ValidateIssuer = false,  // Consider setting to true and specifying Issuer in production
        ValidateAudience = false,  // Consider setting to true and specifying Audience in production
    };
});
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Login API",
        Version = "v1"
    });
});

builder.Services.AddTransient<UserService>();
builder.Services.AddTransient(typeof(IAsyncRepository<>), typeof(AsyncRepository<>));

var connection = builder.Configuration.GetConnectionString("DockerLoginDB");

builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql(connection);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
    //app.ApplyMigration();
//}

app.UseHttpsRedirection();

if (builder.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
//app.MapProductEndpoints();

//ApplyMigration();

app.Run();


//void ApplyMigration()
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var _db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

//        if (_db.Database.GetPendingMigrations().Count() > 0)
//            _db.Database.Migrate();
//    }
//}

