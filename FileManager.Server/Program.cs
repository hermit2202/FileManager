using FileManager.Server.Data;
using FileManager.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку контроллеров и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключаем SQLite базу данных (файл filemanager.db создастся автоматически)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=filemanager.db"));

// Регистрируем наш сервис хранения файлов
builder.Services.AddSingleton<FileStorageService>();

var app = builder.Build();

// Автоматически создаем БД при старте сервера
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();