using Microsoft.AspNetCore.Mvc;
using SiteMonitor.Models;
using SqlSugar;
using System.Reflection;

namespace SiteMonitor.Extension
{
    public static class SqlsugarSetup
    {
        public static void AddSqlsugarSetup(this IServiceCollection services, string dbName = "SystemData")
        {

            SqlSugarScope sqlSugar = new SqlSugarScope(new ConnectionConfig()
            {
                DbType = SqlSugar.DbType.Sqlite,
                ConnectionString = $"DataSource={dbName}.db",
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    //注意:  这儿AOP设置不能少
                    EntityService = (c, p) =>
                    {
                        /***低版本C#写法***/
                        // int?  decimal?这种 isnullable=true 不支持string(下面.NET 7支持)

                        //if (p.IsPrimarykey == false && c.PropertyType.IsGenericType &&
                        //c.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        //{
                        //    p.IsNullable = true;
                        //}

                        /***高版C#写法***/
                        //支持string?和string  
                        if (p.IsPrimarykey == false && new NullabilityInfoContext()
                         .Create(c).WriteState is NullabilityState.Nullable)
                        {
                            p.IsNullable = true;
                        }
                    }
                }
            },
            db =>
            {
                //单例参数配置，所有上下文生效
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql);//输出sql
                };
                //技巧：拿到非ORM注入对象
                //services.GetService<注入对象>();
            });


            var dbpath = $"{AppDomain.CurrentDomain.BaseDirectory}{dbName}.db";
            if (!File.Exists(dbpath))
            {
                Console.WriteLine(dbpath + " 文件不存在");
                sqlSugar.DbMaintenance.CreateDatabase();
                sqlSugar.CodeFirst.InitTables(typeof(RunConfig), typeof(SiteConfig));
            }

            services.AddSingleton<ISqlSugarClient>(sqlSugar);//这边是SqlSugarScope用AddSingleton
        }


    }
}
