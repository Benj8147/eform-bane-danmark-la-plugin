using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Database.Entities;

namespace BaneDanmarkLa.Pn.Infrastructure.Data.Seed.Data
{
    public class BaneDanmarkLaConfigurationSeedData : IPluginConfigurationSeedData
    {
        public PluginConfigurationValue[] Data => new[]
        {
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:LogLevel",
                Value = "4"
            },
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:LogLimit",
                Value = "25000"
            },
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:SdkConnectionString",
                Value = "..."
            },
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:MaxParallelism",
                Value = "1"
            },
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:NumberOfWorkers",
                Value = "1"
            },
            new PluginConfigurationValue()
            {
                Name = "BaneDanmarkLaBaseSettings:SiteIds",
                Value = ""
            }
        };
    }
}