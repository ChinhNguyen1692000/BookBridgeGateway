using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// **QUAN TRỌNG: Thêm Controller Service**
builder.Services.AddControllers(); 

// Đăng ký Ocelot, Swagger, SwaggerForOcelot
builder.Services.AddOcelot();
// Các service khác giữ nguyên...
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// CORS đăng ký trước khi Build
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("https://e00d56ad.bookbridge-5ju.pages.dev",
          "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseRouting();
app.UseCors("AllowFrontEnd");

// 1. Health check endpoint (Render dùng để kiểm tra)
app.MapGet("/api/healthz", () => Results.Ok("Healthy"));

// 2. Controllers — phải đặt trước Ocelot
app.MapControllers();

// 3. SwaggerForOcelot UI (nên nằm sau controllers để tránh conflict)
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// 4. Cuối cùng — Ocelot gateway middleware
await app.UseOcelot();

app.Run();