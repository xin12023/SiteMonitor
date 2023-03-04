using SiteMonitor.Extension;
using SiteMonitor.Helpers;
using SiteMonitor.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();


//// ӳ��������Ϣ�� AppConfig ����
//builder.Services.Configure<MonitorConfig>(builder.Configuration.GetSection("MonitorConfig"));

////���������ļ�
//var config = new ConfigurationBuilder()
//    .SetBasePath(AppContext.BaseDirectory)
//    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
//    .Build();

////ע�������ļ�
//builder.Services.Configure<TokenConfig>(config);




//ע����־����

builder.Services.AddSingleton<LogHelper>();

//ע���̨����

builder.Services.AddHostedService<SiteBackgroundService>();

//ע��TG����
builder.Services.AddSingleton<TgHelper>();

//ע�����ݿ����ӵ���
builder.Services.AddSqlsugarSetup("SystemData");


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
