using Microsoft.AspNetCore.Mvc;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using SqlSugar;

namespace SiteMonitor.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetController : ControllerBase
    {
        private readonly LogHelper _logger;
        private readonly ISqlSugarClient _db;
        public SetController(ISqlSugarClient db, LogHelper logHelper)
        {
            _logger = logHelper;
            _db = db;
        }
        // GET: api/<SetController>
        [HttpGet]
        public async Task<ReturnResult> Get()
        {
            var set = await _db.Queryable<RunConfig>().FirstAsync();
            if (set == null)
            {
                _logger.Information("监控配置未添加");
                return new ReturnResult { Code = 400, Msg = "监控配置未添加" };
            }
            return new ReturnResult { Code = 200, Msg = "OK", Data = set };
        }
        [HttpPost]
        public async Task<ReturnResult> Post([FromBody] RunConfig config)
        {
            try
            {
                var set = await _db.Queryable<RunConfig>().FirstAsync();
                if (set == null)
                {
                    await _db.Insertable(config).ExecuteCommandAsync();
                }
                else
                {
                    await _db.Updateable(config).ExecuteCommandAsync();
                }
                config.Update(config);
                return new ReturnResult { Code = 200, Msg = "OK" };
            }
            catch (Exception ex)
            {
                _logger.Error("更新监控配置发生异常", ex);
                return new ReturnResult { Code = 400, Msg = "更新监控配置发生异常" };
            }
        }   
    }
}
