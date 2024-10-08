using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mu_MovieApis.Models.Domain;
using Mu_MovieApis.Repositories.Abstract;
using Mu_MovieApis.Repositories.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddDbContext<DatabaseContext>(optionsAction => optionsAction.UseSqlite(builder.Configuration.GetConnectionString("conn")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options =>
{
  options.SaveToken = true;
  options.RequireHttpsMetadata = false;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
  };
});
// Add application services.
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder
  .WithOrigins("*")
  .AllowAnyOrigin()
  .AllowAnyMethod()
  .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();