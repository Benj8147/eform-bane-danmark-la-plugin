using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Abstractions
{
    using Infrastructure.Models.Settings;

    public interface IBaneDanmarkLaPnSettingsService
    {
        Task<OperationDataResult<BaneDanmarkLaBaseSettings>> GetSettings();
        Task<OperationResult> UpdateSettings(BaneDanmarkLaBaseSettings baneDanmarkLaBaseSettings);
        
    }
}