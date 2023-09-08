using Serilog;
using Serilog.Filters;
using ThreeL.Infra.Core.Enum;
using ThreeL.Infra.Core.Metadata;

namespace ThreeL.Infra.Core.Serilog
{
    public static class SerilogExtension
    {
        /// <summary>
        /// 根据模块生成对应的日志模块
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="template"></param>
        /// <param name="modules"></param>
        public static void BuildSerilogLogger(string rootPath, string template = null, params Module[] modules)
        {
            var configuration = new LoggerConfiguration().Enrich.FromLogContext();
            configuration.WriteTo.Console();
            foreach (var module in modules)
            {
                var desc = ((int)module, module.ToString(), module.GetDescription());
                configuration.WriteTo.Logger(lg =>
                {
                    lg.Filter.ByIncludingOnly(Matching.FromSource(desc.Item2))
                    .WriteTo.File(Path.Combine(rootPath, $"{desc.Item3}/log.log"), rollingInterval: RollingInterval.Hour,
                         outputTemplate: string.IsNullOrEmpty(template) ? "Date:{Timestamp:yyyy-MM-dd HH:mm:ss.fff} LogLevel:{Level} Class:{SourceContext} Message:{Message}{Exception}{NewLine}" : template); ;
                });
            }

            Log.Logger = configuration.CreateLogger();
        }
    }
}
