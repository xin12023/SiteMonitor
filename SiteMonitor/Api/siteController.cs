using Microsoft.AspNetCore.Mvc;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using SqlSugar;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SiteMonitor.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class siteController : ControllerBase
    {
        private readonly LogHelper _logger;
        private readonly ISqlSugarClient _db;

        public siteController(ISqlSugarClient db, LogHelper logHelper)
        {
            _logger = logHelper;
            _db = db;
        }

        // GET: api/site
        [HttpGet]
        public async Task<ReturnResult> Get()
        {
            try
            {
                var sites = await _db.Queryable<SiteConfig>().ToListAsync();
                return new ReturnResult { Code = 200, Msg = "查询成功", Data = sites };
            }
            catch (Exception ex)
            {
                _logger.Error("查询监控站点配置发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "查询监控站点配置发生异常" };
            }
        }

        // GET: api/site/{id}
        [HttpGet("{id}")]
        public async Task<ReturnResult> Get(int id)
        {
            try
            {
                var site = await _db.Queryable<SiteConfig>().InSingleAsync(id);
                if (site == null)
                {
                    _logger.Information("监控站点未找到");
                    return new ReturnResult { Code = 400, Msg = "监控站点未找到" };
                }
                return new ReturnResult { Code = 200, Msg = "查询成功", Data = site };
            }
            catch (Exception ex)
            {
                _logger.Error("查询监控站点配置发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "查询监控站点配置发生异常" };
            }
        }

        // POST: api/site
        [HttpPost]
        public async Task<ReturnResult> Post([FromBody] SiteConfig config)
        {
            try
            {
                await _db.Insertable(config).ExecuteCommandAsync();
                return new ReturnResult { Code = 200, Msg = "添加成功" };
            }
            catch (Exception ex)
            {
                _logger.Error("添加监控站点配置发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "添加监控站点配置发生异常" };
            }
        }

        // PUT api/site/5
        [HttpPut("{id}")]
        public async Task<ReturnResult> Put(int id, [FromBody] SiteConfig config)
        {
            try
            {
                var site = await _db.Queryable<SiteConfig>().Where(s => s.Id == id).FirstAsync();
                if (site == null)
                {
                    _logger.Information($"编号为{id}的监控站点不存在");
                    return new ReturnResult { Code = 400, Msg = $"编号为{id}的监控站点不存在" };
                }
                config.Id = id;
                await _db.Updateable(config).ExecuteCommandAsync();
                return new ReturnResult { Code = 200, Msg = "OK" };
            }
            catch (Exception ex)
            {
                _logger.Error("更新监控站点发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "更新监控站点发生异常" };
            }
        }

        // DELETE api/site/5
        [HttpDelete("{id}")]
        public async Task<ReturnResult> Delete(int id)
        {
            try
            {
                var site = await _db.Queryable<SiteConfig>().Where(s => s.Id == id).FirstAsync();
                if (site == null)
                {
                    _logger.Information($"编号为{id}的监控站点不存在");
                    return new ReturnResult { Code = 400, Msg = $"编号为{id}的监控站点不存在" };
                }
                await _db.Deleteable<SiteConfig>().In(id).ExecuteCommandAsync();
                return new ReturnResult { Code = 200, Msg = "OK" };
            }
            catch (Exception ex)
            {
                _logger.Error("删除监控站点发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "删除监控站点发生异常" };
            }
        }


    }
}
