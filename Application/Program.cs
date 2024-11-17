using Application.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/say/{message}", (string message) => $"Page says: {message}.");

app.Run();
