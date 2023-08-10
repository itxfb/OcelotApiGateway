using ApiGateway.Models;
using ApiGateway.Services;
using APIGateway.Middleware;
using AuthorizationService.JwtStatelessToken;
using Database;
using Microsoft.EntityFrameworkCore;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SharedLib.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("configuration.json", false, true);

builder.Services.AddCors();

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
builder.Services.AddTransient<IAdoNet>(x => new AdoNet(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IDBHelper>(x => new DBHelper(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddSingleton<UserSession>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddStatelessTokenAuthentication();

builder.Services
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthentication();
//app.UseAuthorization();

app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<HttpLoggingMiddleware>();

//app.Use(async (context, next) =>
//{
//    context.Response.OnStarting(async o => {
//        if (o is HttpContext ctx)
//        {
//            ctx.Response.Headers["OnStarting-One"] = "1";
//        }
//    }, context);
//    await next();
//});
//app.MapControllers();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

await app.UseOcelot();


//app.UseStatusCodePages(async statusCodeContext =>
//{
//    // using static System.Net.Mime.MediaTypeNames;
//    statusCodeContext.HttpContext.Response.ContentType = "text/plain";// Text.Plain;

//    await statusCodeContext.HttpContext.Response.WriteAsync(
//        $"Status Code Page: {statusCodeContext.HttpContext.Response.StatusCode}");
//});

app.Run();
