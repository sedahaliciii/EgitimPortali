using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EgitimPortaliAPI.Models;
using EgitimPortaliAPI.Repositories;
using Microsoft.OpenApi.Models; // Swagger için gerekli
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger yapılandırmasını güncelle - Authorize butonu ekle
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Eğitim Portalı API", Version = "v1" });

    // JWT Bearer yetkilendirme tanımı ekle
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DbContext konfigürasyonu
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity konfigürasyonu
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(Program));

// JWT konfigürasyonu - Önceki .AddAuthentication() çağrısını kaldırın
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };

    // Token doğrulama hatalarını konsola logla
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for: {context.Principal.Identity.Name}");
            return Task.CompletedTask;
        }
    };
});

// CORS konfigürasyonu
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Repository'leri DI sistemine kaydet
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EnrollmentRepository>();
builder.Services.AddScoped<AssignmentRepository>();
builder.Services.AddScoped<InstructorRepository>();
builder.Services.AddScoped<LessonRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<ReviewRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Eğitim Portalı API V1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

    // Development ortamında JWT ayarlarını konsola yazdır
    Console.WriteLine($"JWT Issuer: {app.Configuration["JWT:Issuer"]}");
    Console.WriteLine($"JWT Audience: {app.Configuration["JWT:Audience"]}");
    Console.WriteLine($"JWT Key Length: {app.Configuration["JWT:Key"]?.Length ?? 0} chars");
}

app.UseHttpsRedirection();

// CORS'u kimlik doğrulamadan önce kullan
app.UseCors("AllowAll");

// Kimlik doğrulama ve yetkilendirme pipeline'da doğru sırada olmalı
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Admin kullanıcı oluşturma
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Admin rolü oluştur
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Normal kullanıcı rolü oluştur
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Mevcut admin kullanıcıyı sil
        var existingAdmin = await userManager.FindByEmailAsync("admin@mail.com");
        if (existingAdmin != null)
        {
            await userManager.DeleteAsync(existingAdmin);
            Console.WriteLine("✅ Mevcut admin kullanıcı silindi.");
        }

        // Yeni admin kullanıcı oluştur
        var adminEmail = "admin@mail.com";
        var adminPassword = "Admin123!";

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            RegistrationDate = DateTime.Now
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Admin kullanıcı oluşturuldu: {adminEmail} / {adminPassword}");
        }
        else
        {
            Console.WriteLine($"❌ Admin oluşturma hatası: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Admin kullanıcı oluşturma hatası: {ex.Message}");
    }
}

app.Run();