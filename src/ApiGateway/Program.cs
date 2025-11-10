using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Bật Endpoint API Explorer
builder.Services.AddEndpointsApiExplorer();

// Đăng ký Health Checks
builder.Services.AddHealthChecks(); 

// Đăng ký Ocelot, Swagger, SwaggerForOcelot
builder.Services.AddOcelot();
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
        policy.WithOrigins("https://bookbridge-5ju.pages.dev",
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

// 1. Cấu hình các Middleware cơ bản
app.UseRouting();
app.UseCors("AllowFrontEnd");

// 2. Map Health Check và Root URL endpoint (sẽ bắt các yêu cầu /healthz và /)
// Phải được đặt SAU app.UseRouting() và TRƯỚC Ocelot
app.UseEndpoints(endpoints =>
{
    // Cấu hình Health Check cho Render (hoạt động độc lập với Ocelot)
    endpoints.MapHealthChecks("/healthz"); 
    // Xử lý Root URL để loại bỏ lỗi HEAD / và GET /
    endpoints.MapGet("/", () => Results.Ok("API Gateway is running. Check /swagger/docs for API details."));
});


// 3. SwaggerForOcelot UI
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// 4. Cuối cùng — Ocelot gateway middleware
// Ocelot sẽ chỉ xử lý các yêu cầu không được MapGet/MapHealthChecks xử lý ở trên
await app.UseOcelot(); // Dòng này là dòng 71

app.Run();