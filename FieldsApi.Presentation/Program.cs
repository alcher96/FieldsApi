using FieldsApi;
using FieldsApi.Application.Services;
using FieldsApi.Domain.Repositories;
using FieldsApi.Infrastructure.Repositories;
using FieldsApi.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IFieldRepository>(_ =>
{
    var fieldsPath = Path.Combine(AppContext.BaseDirectory, "Data", "fields.kml");
    var centroidsPath = Path.Combine(AppContext.BaseDirectory, "Data", "centroids.kml");

    // Проверка существования файлов для отладки
    if (!File.Exists(fieldsPath))
        throw new FileNotFoundException($"Fields file not found at: {fieldsPath}");
    if (!File.Exists(centroidsPath))
        throw new FileNotFoundException($"Centroids file not found at: {centroidsPath}");

    return new FieldRepository(fieldsPath, centroidsPath);
});
builder.Services.AddSingleton<IFieldService, FieldService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));


// Настройка Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fields API", Version = "v1" });
});

// Настройка graceful shutdown
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(30); // Увеличен до 30 секунд
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fields API v1"));
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();

