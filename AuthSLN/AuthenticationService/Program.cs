using AuthenticationService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var certPath = builder.Configuration["Certificate:Path"];
var certPassword = builder.Configuration["Certificate:Password"];

if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
{
    throw new Exception("Certificate path or password is not configured.");
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("keys"))
    .ProtectKeysWithCertificate(new X509Certificate2(File.ReadAllBytes(certPath), certPassword));

var rsa = RSA.Create();
var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"];
if (string.IsNullOrEmpty(publicKeyPath))
{
    throw new Exception("Public key path is not configured.");
}
rsa.ImportFromPem(File.ReadAllText(publicKeyPath));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new RsaSecurityKey(rsa)
        };
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
    serverOptions.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps(certPath, certPassword);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
