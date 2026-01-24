using Microsoft.AspNetCore.Identity;
using MongoDB.AspNet.Identity;
using MongoDB.Driver;
using MongoIdentityUser = MongoDB.AspNet.Identity.IdentityUser;
using MongoIdentityRole = MongoDB.AspNet.Identity.IdentityRole;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB
var connectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? "mongodb://localhost:27017/SampleIdentityDb";
var mongoUrl = new MongoUrl(connectionString);
var mongoClient = new MongoClient(mongoUrl);
var mongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);

// Register MongoDB database as a service
builder.Services.AddSingleton(mongoDatabase);

// Configure ASP.NET Core Identity with MongoDB
builder.Services.AddIdentity<MongoIdentityUser, MongoIdentityRole>(options =>
{
    // Password settings (relaxed for demo purposes)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddUserStore<UserStore<MongoIdentityUser>>()
.AddRoleStore<RoleStore>()
.AddRoleManager<RoleManager<MongoIdentityRole>>()
.AddSignInManager()
.AddDefaultTokenProviders();

// Add authorization
builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
