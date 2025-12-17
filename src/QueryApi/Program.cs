var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger'ı kök adrese taşıyoruz
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoEnergyMonitor API v1");
    c.RoutePrefix = string.Empty;  // Bu satır kritik – Swagger kök adreste açılır
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();