
using BusinessCard.Core.Repository;
using BusinessCard.Core.Servises;
using BusinessCard_Rahaf.Modals;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var connectionString = builder.Configuration.GetConnectionString("DbConnection");
builder.Services.AddDbContext<DbCardContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IBusinessCardServise, BusinessCardRepository>();

builder.Services.AddSwaggerGen();

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
