using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDoListAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen
    (c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDoListAPI", Version = "v1" });
        }
    );

builder.Services.AddDbContext<ToDoListContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI
    (
        c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
            c.RoutePrefix = string.Empty; // Esto hace que Swagger UI esté en la raíz
        }
    );
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

