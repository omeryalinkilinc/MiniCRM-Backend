using MiniCRM.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MiniCRM.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Swagger servisleri
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS ayarı (frontend'den cookie gönderimi için şart)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // 🔹 frontend adresi
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 🔥 cookie gönderimi için gerekli
    });
});

// ✅ Controller servisi
builder.Services.AddControllers();

// ✅ DbContext bağlantısı
builder.Services.AddDbContext<MiniCRMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ JWT servis bağlantısı
builder.Services.AddScoped<JwtService>();

// ✅ JWT doğrulama ayarları
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // 🔥 Cookie'den gelen token'ı tanımak için
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["access_token"];
            Console.WriteLine("Gelen token: " + token); // 🔍 debug log
            context.Token = token;
            return Task.CompletedTask;
        }
    };

// 🔐 Token doğrulama parametreleri
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = "MiniCRM",
    ValidAudience = "MiniCRMClient",
    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT key not found"))),

    NameClaimType = "UserId" // ✅ Bu satır UserId claim'ini tanımak için şart
};
});

var app = builder.Build();

// ✅ Swagger middleware (sadece development'ta)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Middleware sırası (önemli!)
app.UseRouting();
app.UseCors("AllowFrontend"); // 🔥 CORS policy aktif edilmeli
app.UseAuthentication();      // 🔐 JWT doğrulama
app.UseAuthorization();       // 🔐 Role bazlı erişim
/*app.UseHttpsRedirection(); */   // 🌐 HTTPS yönlendirme

// ✅ Örnek endpoint (test amaçlı)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ✅ Controller endpoint'leri
app.MapControllers();

// ✅ Uygulamayı başlat
app.Run();

// ✅ DTO tanımı (örnek)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
