var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPolling", policy =>
    {
        policy
            .WithOrigins("http://localhost:5223")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowPolling");
app.MapControllers();
app.UseHttpsRedirection();

app.Run();

