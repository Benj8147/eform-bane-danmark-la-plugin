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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Data.Seed;
using BaneDanmarkLa.Pn.Infrastructure.Data.Seed.Data;
using BaneDanmarkLa.Pn.Infrastructure.Models.Settings;
using BaneDanmarkLa.Pn.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microting.eFormApi.BasePn;
using Microting.eFormApi.BasePn.Infrastructure.Database.Extensions;
using Microting.eFormApi.BasePn.Infrastructure.Models.Application;
using Microting.eFormApi.BasePn.Infrastructure.Settings;
using Microting.eFormCaseTemplateBase.Infrastructure.Data;
using Microting.eFormCaseTemplateBase.Infrastructure.Data.Factories;

namespace BaneDanmarkLa.Pn
{

    public class EformBaneDanmarkLaPlugin : IEformPlugin
    {
        public string Name => "Microting BaneDanmark La Plugin";
        public string PluginId => "eform-angular-banedanmarkla-plugin";
        public string PluginPath => PluginAssembly().Location;
        private string _connectionString;
        private int _maxParallelism = 1;
        private int _numberOfWorkers = 1;

        public Assembly PluginAssembly()
        {
            return typeof(EformBaneDanmarkLaPlugin).GetTypeInfo().Assembly;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBaneDanmarkLaLocalizationService, BaneDanmarkLaLocalizationService>();
            services.AddTransient<IBaneDanmarkLaPnSettingsService, BaneDanmarkLaPnSettingsService>();
            services.AddTransient<IBaneDanmarkLaService, CaseTemplateService>();

        }

        public void AddPluginConfig(IConfigurationBuilder builder, string connectionString)
        {
            var seedData = new BaneDanmarkLaConfigurationSeedData();
            var contextFactory = new CaseTemplatePnContextFactory();
            builder.AddPluginConfiguration(
                connectionString, 
                seedData, 
                contextFactory);
        }

        public void ConfigureOptionsServices(
            IServiceCollection services, 
            IConfiguration configuration)
        {
            services.ConfigurePluginDbOptions<BaneDanmarkLaBaseSettings>(
                configuration.GetSection("BaneDanmarkLaBaseSettings"));
        }

        public void ConfigureDbContext(IServiceCollection services, string connectionString)
        {
            connectionString = connectionString;
            if (connectionString.ToLower().Contains("convert zero datetime"))
            {
                services.AddDbContext<CaseTemplatePnDbContext>(o => o.UseMySql(connectionString,
                    b => b.MigrationsAssembly(PluginAssembly().FullName)));
            }
            else
            {
                services.AddDbContext<CaseTemplatePnDbContext>(o => o.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(PluginAssembly().FullName)));
            }

            CaseTemplatePnContextFactory contextFactory = new CaseTemplatePnContextFactory();
            var context = contextFactory.CreateDbContext(new[] {connectionString});
            context.Database.Migrate();

            // Seed database
            SeedDatabase(connectionString);

            string temp = context.PluginConfigurationValues
                .SingleOrDefault(x => x.Name == "BaneDanmarkLaBaseSettings:MaxParallelism")?.Value;
            _maxParallelism = string.IsNullOrEmpty(temp) ? 1 : int.Parse(temp);

            temp = context.PluginConfigurationValues
                .SingleOrDefault(x => x.Name == "BaneDanmarkLaBaseSettings:NumberOfWorkers")?.Value;
            _numberOfWorkers = string.IsNullOrEmpty(temp) ? 1 : int.Parse(temp);
        }

        public void Configure(IApplicationBuilder appBuilder)
        {

        }

        public MenuModel HeaderMenu(IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider
                .GetService<IBaneDanmarkLaLocalizationService>();

            var result = new MenuModel();
            result.LeftMenu.Add(new MenuItemModel()
            {
                Name = localizationService.GetString("BaneDanmarkLa"),
                E2EId = "bane-danmark-la-pn",
                Link = "",
                MenuItems = new List<MenuItemModel>()
                {
                    new MenuItemModel()
                    {
                        Name = localizationService.GetString("LA'er"),
                        E2EId = "bane-danmark-la-pn-viewLa",
                        Link = "/plugins/bane-danmark-la-pn/viewLa",
                        Position = 1,
                    },
                    new MenuItemModel()
                    {
                        Name = localizationService.GetString("Reports"),
                        E2EId = "bane-danmark-la-pn-reports",
                        Link = "/plugins/bane-danmark-la-pn/reports",
                        Position = 2,
                    },
                    new MenuItemModel()
                    {
                        Name = localizationService.GetString("Settings"),
                        E2EId = "bane-danmark-la-pn-settings",
                        Link = "/plugins/bane-danmark-la-pn/settings",
                        Position = 3,
                    }
                }
            });
            return result;
        }

        public void SeedDatabase(string connectionString)
        {
            // Get DbContext
            var contextFactory = new CaseTemplatePnContextFactory();
            using (var context = contextFactory.CreateDbContext(new []{connectionString}))
            {
                // Seed configuration
                BaneDanmarkLaPluginSeed.SeedData(context);
            }
        }
    }
}