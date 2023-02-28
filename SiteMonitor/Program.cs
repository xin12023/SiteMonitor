using Microsoft.Extensions.Hosting;
using SiteMonitor.Models;
using SiteMonitor.Serve;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
//注册后台服务
builder.Services.AddHostedService<SiteBackgroundService>();

// 映射配置信息到 AppConfig 类中
builder.Services.Configure<MonitorConfig>(builder.Configuration.GetSection("MonitorConfig"));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
