var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowBackend", policy => policy
        .WithOrigins("http://localhost:5226") 
        .AllowAnyHeader()
        .AllowAnyMethod());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//

app.MapControllers();

app.UseCors("AllowIsoBackend");

app.Run();