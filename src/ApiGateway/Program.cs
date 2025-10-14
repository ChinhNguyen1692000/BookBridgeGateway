using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load file cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Đăng ký các service
builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();

// ✅ Tránh lỗi trùng API description khi merge Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// ✅ Đăng ký SwaggerForOcelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseRouting();

// ✅ Dùng SwaggerForOcelot UI
app.UseSwaggerForOcelotUI(opt =>
{
    // ⚠️ Không dùng RoutePrefix nữa
    opt.PathToSwaggerGenerator = "/swagger/docs";
    // opt.Use// Ở phiên bản 8.x, bạn nên sử dụng thuộc tính này:
    // opt.EndPointCollapserBehaviour = "BookBridge API Gateway";
})
.UseOcelot()
.Wait();

app.Run();
