/*
The MIT License (MIT)

Copyright (c) 2007 - 2019 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microting.eFormApi.BasePn.Infrastructure.Helpers.PluginDbOptions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Microting.eFormCaseTemplateBase.Infrastructure.Data;

namespace BaneDanmarkLa.Pn.Services
{
    using Infrastructure.Models.Settings;

    public class BaneDanmarkLaPnSettingsService : IBaneDanmarkLaPnSettingsService
    {
        private readonly ILogger<BaneDanmarkLaPnSettingsService> _logger;
        private readonly IBaneDanmarkLaLocalizationService _baneDanmarkLaLocalizationService;
        private readonly CaseTemplatePnDbContext _dbContext;
        private readonly IPluginDbOptions<BaneDanmarkLaBaseSettings> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public BaneDanmarkLaPnSettingsService(ILogger<BaneDanmarkLaPnSettingsService> logger,
            IBaneDanmarkLaLocalizationService baneDanmarkLaLocalizationService,
            CaseTemplatePnDbContext dbContext,
            IPluginDbOptions<BaneDanmarkLaBaseSettings> options,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _dbContext = dbContext;
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _baneDanmarkLaLocalizationService = baneDanmarkLaLocalizationService;
        }


        public async Task<OperationDataResult<BaneDanmarkLaBaseSettings>> GetSettings()
        {
            try
            {
                var option = _options.Value;

                if (option.SdkConnectionString == "...")
                {
                    string connectionString = _dbContext.Database.GetDbConnection().ConnectionString;

                    string dbNameSection = Regex.Match(connectionString, @"(Database=(...)_eform-angular-\w*-plugin;)").Groups[0].Value;
                    string dbPrefix = Regex.Match(connectionString, @"Database=(\d*)_").Groups[1].Value;
                    string sdk = $"Database={dbPrefix}_SDK;";
                    connectionString = connectionString.Replace(dbNameSection, sdk);
                    await _options.UpdateDb(settings => { settings.SdkConnectionString = connectionString;}, _dbContext, UserId);

                }

                return new OperationDataResult<BaneDanmarkLaBaseSettings>(true, option);
            }
            catch(Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationDataResult<BaneDanmarkLaBaseSettings>(false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorWhileObtainingBaneDanmarkLaSettings"));
            }
        }

        public async Task<OperationResult> UpdateSettings(BaneDanmarkLaBaseSettings baneDanmarkLaBaseSettings)
        {
            try
            {
                await _options.UpdateDb(settings =>
                {
                    settings.LogLevel = baneDanmarkLaBaseSettings.LogLevel;
                    settings.LogLimit = baneDanmarkLaBaseSettings.LogLimit;
                    settings.MaxParallelism = baneDanmarkLaBaseSettings.MaxParallelism;
                    settings.NumberOfWorkers = baneDanmarkLaBaseSettings.NumberOfWorkers;
                    settings.SdkConnectionString = baneDanmarkLaBaseSettings.SdkConnectionString;
                    settings.SiteIds = baneDanmarkLaBaseSettings.SiteIds;
                }, _dbContext, UserId);
                
                return new OperationResult(true,
                    _baneDanmarkLaLocalizationService.GetString("SettingsHaveBeenUpdatedSuccessfully"));
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                _logger.LogError(e.Message);
                return new OperationResult(false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorWhileUpdatingSettings"));
            }
        }
        
        public int UserId
        {
            get
            {
                var value = _httpContextAccessor?.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return value == null ? 0 : int.Parse(value);
            }
        }
    }
}