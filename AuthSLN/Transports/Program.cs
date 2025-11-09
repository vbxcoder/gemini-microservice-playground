var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var certPath = builder.Configuration["Certificate:Path"];
var certPassword = builder.Configuration["Certificate:Password"];

if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
{
    throw new Exception("Certificate path or password is not configured.");
}

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

app.UseAuthorization();

app.MapControllers();

app.Run();