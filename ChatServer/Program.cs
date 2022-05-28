using ChatServer.Hubs;
using ChatServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddUserService();

#endregion

WebApplication app = builder.Build();

#region Middlewares

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(options =>
{
    options.WithOrigins("http://127.0.0.1:5500")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(x => true)
    .AllowCredentials();
});

app.UseEndpoints(endpoints => {
    endpoints.MapHub<ChatHub>("/chat");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Jwt}/{action=Registration}/{id?}"
    );
});

#endregion

app.Run();
