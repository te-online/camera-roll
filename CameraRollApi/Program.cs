using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TinifyAPI;

var builder = WebApplication.CreateBuilder(args);
var tinifyApiKey = builder.Configuration["Tinify:Key"];

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure InMemory Database
builder.Services.AddDbContext<PhotoContext>(opt => opt.UseInMemoryDatabase("Photos"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Tinify.Key = tinifyApiKey;

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
