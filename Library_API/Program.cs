using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Library_API.DataAccess;
using Library_API.DataAccess.DAOs;
using Library_API.Models;
using Library_API.Repository;
using Library_API.Services.Interface;
using Library_API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BookstoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddScoped<IAuthorServices, AuthorServices>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreContext>();

    // Kiểm tra nếu cơ sở dữ liệu đã tồn tại và nếu có migration cần thiết thì cập nhật
    if (dbContext.Database.CanConnect())
    {
        // Nếu cơ sở dữ liệu tồn tại, kiểm tra xem có migration cần phải thực hiện không
        dbContext.Database.Migrate();
    }
    else
    {
        // Nếu cơ sở dữ liệu chưa tồn tại, tạo mới cơ sở dữ liệu
        dbContext.Database.EnsureCreated();
    }

    // Khởi tạo dữ liệu mẫu (nếu cần)
    DbInitializer.Initialize(dbContext);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
