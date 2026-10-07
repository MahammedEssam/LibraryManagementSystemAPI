using LibraryManagement.BLL;
using LibraryManagement.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register Database Connection
string connString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddSingleton(new DatabaseConnection(connString));

// Register Layers (Dependency Injection)
builder.Services.AddScoped<HealthDAL>();
builder.Services.AddScoped<HealthBLL>();

builder.Services.AddScoped<AuthorDAL>();
builder.Services.AddScoped<AuthorBLL>();
builder.Services.AddScoped<BookDAL>();
builder.Services.AddScoped<BookBLL>();
builder.Services.AddScoped<MemberDAL>();
builder.Services.AddScoped<MemberBLL>();
builder.Services.AddScoped<LoanDAL>();
builder.Services.AddScoped<LoanBLL>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
