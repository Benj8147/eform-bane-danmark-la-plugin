using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Database.Entities;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Controllers
{
    using Infrastructure.Models.Settings;

    public class BaneDanmarkLaSettingsController : Controller
    {
        private readonly IBaneDanmarkLaPnSettingsService _baneDanmarkLaPnSettingsService;

        public BaneDanmarkLaSettingsController(IBaneDanmarkLaPnSettingsService baneDanmarkLaPnSettingsService)
        {
            _baneDanmarkLaPnSettingsService = baneDanmarkLaPnSettingsService;
        }

        [HttpGet]
        [Authorize(Roles = EformRole.Admin)]
        [Route("api/bane-danmark-la-pn/settings")]
        public async Task<OperationDataResult<BaneDanmarkLaBaseSettings>> GetSettings()
        {
            return await _baneDanmarkLaPnSettingsService.GetSettings();
        }
        
        [HttpPost]
        [Authorize(Roles = EformRole.Admin)]
        [Route("api/bane-danmark-la-pn/settings")]
        public async Task<OperationResult> UpdateSettings([FromBody] BaneDanmarkLaBaseSettings baneDanmarkLaBaseSettings)
        {
            return await _baneDanmarkLaPnSettingsService.UpdateSettings(baneDanmarkLaBaseSettings);
        }

    }
}