using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using BookStore_API.DataAccess;
using BookStore_API.DataAccess.DAOs;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using BookStore_API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json;
using BookStore_API.Authentions;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Giữ nguyên tên property (Email thay vì email)
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Không phân biệt hoa thường
    });
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddDbContext<BookStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});


// Register JWTAuthService with dependencies
builder.Services.AddScoped<JWTAuthService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["JwtSettings:Key"];
        var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
        var jwtAudience = builder.Configuration["JwtSettings:Audience"];
        var jwtExpireMinutes = int.Parse(builder.Configuration["JwtSettings:ExpireMinutes"] ?? "60");

        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new ArgumentNullException("JwtSettings:Key", "JWT Key cannot be null or empty. Please check appsettings.json.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authen", Version = "v1" });

    // Cấu hình bảo mật JWT (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Yêu cầu bảo mật cho tất cả các endpoint
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
            new List<string>()
        }
    });
});

// Initialize Firebase Admin SDK không có firebase-key thì đừng có bật.
//FirebaseApp.Create(new AppOptions()
//{
//    Credential = GoogleCredential.FromFile("path/to/your/firebase-key.json")
//});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IDao<>), typeof(Dao<>));
builder.Services.AddScoped<IMapperService, MapperService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWishService, WishService>();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<BookStoreContext>();

//    dbContext.Database.EnsureDeleted();

//    // Kiểm tra nếu cơ sở dữ liệu đã tồn tại và nếu có migration cần thiết thì cập nhật
//    if (dbContext.Database.CanConnect())
//    {
//        // Nếu cơ sở dữ liệu tồn tại, kiểm tra xem có migration cần phải thực hiện không
//        dbContext.Database.Migrate();
//    }
//    else
//    {
//        // Nếu cơ sở dữ liệu chưa tồn tại, tạo mới cơ sở dữ liệu
//        dbContext.Database.EnsureCreated();
//    }

//    // Khởi tạo dữ liệu mẫu (nếu cần)
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
