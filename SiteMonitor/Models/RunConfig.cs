using SqlSugar;
using System.Runtime.CompilerServices;

namespace SiteMonitor.Models
{
    
    [SugarTable("runConfig")]
    public partial class RunConfig
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        [SugarColumn(Length = 128)]
        public string? TgToken { get; set; }
        [SugarColumn(Length = 128)]
        public string? GroupIds { get; set; }
        [SugarColumn(Length = 64)]
        public string? RunCron { get; set; }
        [SugarColumn(Length = 32)]
        public string? LogPath { get; set; }
        [SugarColumn(Length = 32)]
        public string? LogNameFormat { get; set; }
        public short? LogCopie { get; set; }



        //注册事件
        public delegate void ConfigChangedEventHandler(RunConfig sender);
        public static event ConfigChangedEventHandler? ConfigChanged;

        private void NotifyConfigChanged()
        {
            ConfigChanged?.Invoke(this);
        }

        public void Update(RunConfig updatedConfig)
        {
            // 通知更新
            NotifyConfigChanged();
        }
    }
}
