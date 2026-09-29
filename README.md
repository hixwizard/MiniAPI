### MiniAPI — шпаргалка по .NET Minimal API

#### Что это

Минимальный Web API на .NET 8. Аналог FastAPI (Python), но на C#.
Весь сервер — один файл `Program.cs`, без контроллеров и лишней структуры.

---

#### Запуск

```bash
# Просто HTTP (по умолчанию первый профиль из launchSettings.json)
dotnet run

# HTTP + HTTPS (явно указываем профиль)
dotnet run --launch-profile https
```
---
#### launchSettings.json — конфиг запуска (только для разработки)

Лежит: `Properties/launchSettings.json`
- **Не попадает в прод** (не копируется при `dotnet publish`).
- Описывает профили запуска: порты, переменные окружения, URL браузера.
- `dotnet run` без флагов берёт **первый** профиль из списка.
- Блок `iisSettings` / `IIS Express` — только для Windows, на Linux не нужен.
---

##### appsettings.json — главный конфиг приложения

Аналог `settings.py` в Django. Хранит настройки в JSON.

#### Приоритет конфигурации (от низкого к высокому)

1. `appsettings.json` — базовые настройки
2. `appsettings.{Environment}.json` — перезаписывает по окружению
3. Переменные окружения — перезаписывают всё

#### Окружение

Задаётся переменной `ASPNETCORE_ENVIRONMENT`:
- `Development` — локальная разработка (читает `appsettings.Development.json`)
- `Production` — прод (читает `appsettings.Production.json`)

#### Чтение конфига в коде

```csharp
var builder = WebApplication.CreateBuilder(args);

// Простое чтение
var debugMode = builder.Configuration.GetValue<bool>("BotSettings:DebugMode");
var maxRules = builder.Configuration.GetValue<int>("BotSettings:MaxRules");

// Строка подключения к БД (секция ConnectionStrings)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
```
### Секреты (пароли, ключи API)
**Не хранить в appsettings.json!** Передавать через:
- переменные окружения,
- секрет-менеджер (в проде: Vault, AWS Secrets, и т.п.).
---
### Пакеты
```bash
# Сам EF Core — движок ORM (аналог Django ORM)
dotnet add package Microsoft.EntityFrameworkCore

# Провайдер для PostgreSQL (аналог psycopg2/asyncpg)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```
```bash
# Ещё один пакет — для миграций через CLI
dotnet add package Microsoft.EntityFrameworkCore.Design
```
### Миграции
```bash
# Создать миграцию (аналог makemigrations)
dotnet ef migrations add InitialCreate

# Применить к базе (аналог migrate)
dotnet ef database update
```
---
### Program.cs — минимальный API

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// GET — простой текст
app.MapGet("/", () => "Hello World!");

// GET — параметр из пути
app.MapGet("/hello/{name}", (string name) => $"Привет, {name}!");

// GET — JSON (анонимный объект сериализуется автоматически)
app.MapGet("/items", () => new[] {
    new { id = 1, name = "Item A", price = 19.99 },
    new { id = 2, name = "Item B", price = 29.99 }
});

// POST — приём JSON в типизированный объект
record ItemCreate(string Name, decimal Price);

app.MapPost("/items", (ItemCreate item) =>
{
    return Results.Created($"/items/{item.Name}",
        new { id = Guid.NewGuid(), item.Name, item.Price });
});

app.Run();
```

---

#### Сравнение с FastAPI / DRF

| FastAPI / DRF | .NET Minimal API |
|---------------|------------------|
| `app.get("/")` | `app.MapGet("/", ...)` |
| `app.post("/")` | `app.MapPost("/", ...)` |
| Pydantic-модель | `record` как DTO |
| Валидация | атрибуты `[Required]`, `[Range]` + FluentValidation |
| `settings.py` | `appsettings.json` |
| `DEBUG = True` | `ASPNETCORE_ENVIRONMENT=Development` |
| `ALLOWED_HOSTS` | `AllowedHosts` |
| uvicorn / gunicorn | Kestrel (встроен) |
| nginx перед приложением | то же самое |

---

#### Команды

```bash
# Создать проект
dotnet new web -n MiniAPI

# Запустить
dotnet run
dotnet run --launch-profile https

# Доверять локальному сертификату (один раз)
dotnet dev-certs https --trust

# Собрать для деплоя
dotnet publish -c Release
```