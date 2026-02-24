using Microsoft.EntityFrameworkCore;
using WEB_USER_API.DATA;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace WEB_USER_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            
            // Voeg CORS toe voor Android emulator en ontwikkeling
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAndroidEmulator",
                    builder => builder
                        .WithOrigins("http://10.0.2.2:5053", "http://localhost:5053")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<API_DBContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection 'Default connection' is not found"));
            });

            //  Authentication moet hier staan, v��r builder.Build()
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = false,
                       ValidateAudience = false,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(
                           Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                   };
               });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("AllowAndroidEmulator");
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            //  Middleware pipeline
            app.UseAuthentication();            
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}