using SiteMonitor.Extension;
using SiteMonitor.Helpers;
using SiteMonitor.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


//// 映射配置信息到 AppConfig 类中
//builder.Services.Configure<MonitorConfig>(builder.Configuration.GetSection("MonitorConfig"));

////加载配置文件
//var config = new ConfigurationBuilder()
//    .SetBasePath(AppContext.BaseDirectory)
//    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
//    .Build();

////注册配置文件
//builder.Services.Configure<TokenConfig>(config);




//注册日志服务

builder.Services.AddSingleton<LogHelper>();

//注册后台服务

builder.Services.AddHostedService<SiteBackgroundService>();

//注册TG服务
builder.Services.AddSingleton<TgHelper>();

//注册数据库连接单例
builder.Services.AddSqlsugarSetup("SystemData");


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
