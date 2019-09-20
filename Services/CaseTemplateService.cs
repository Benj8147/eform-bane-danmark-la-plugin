using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using BaneDanmarkLa.Pn.Infrastructure.Models.La;
using eFormCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microting.eForm.Dto;
using Microting.eFormApi.BasePn.Abstractions;
using Microting.eFormApi.BasePn.Infrastructure.Extensions;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;
using Microting.eFormCaseTemplateBase.Infrastructure.Data;
using Microting.eFormCaseTemplateBase.Infrastructure.Data.Entities;
using Microting.eForm.Infrastructure.Constants;
using Microting.eForm.Infrastructure.Models;

namespace BaneDanmarkLa.Pn.Services
{
    public class CaseTemplateService: IBaneDanmarkLaService
    {
        private readonly CaseTemplatePnDbContext _dbContext;
        private readonly IBaneDanmarkLaLocalizationService _baneDanmarkLaLocalizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEFormCoreService _core;

        public CaseTemplateService(
            CaseTemplatePnDbContext dbcontext,
            IBaneDanmarkLaLocalizationService baneDanmarkLaLocalizationService,
            IHttpContextAccessor httpContextAccessor,
            IEFormCoreService core)
        {
            _dbContext = dbcontext;
            _baneDanmarkLaLocalizationService = baneDanmarkLaLocalizationService;
            _httpContextAccessor = httpContextAccessor;
            _core = core;
        }
        
        public async Task<OperationDataResult<CaseTemplatesModel>> GetAll(CaseTemplateRequestModel pnRequestModel)
        {
            try
            {
                CaseTemplatesModel caseTemplatesModel = new CaseTemplatesModel();

                IQueryable<CaseTemplate> caseTemplatesQuery = _dbContext.CaseTemplates.AsQueryable();
                if (!string.IsNullOrEmpty(pnRequestModel.Sort))
                {
                    if (pnRequestModel.IsSortDsc)
                    {
                        caseTemplatesQuery = caseTemplatesQuery.CustomOrderByDescending(pnRequestModel.Sort);
                    }
                    else
                    {
                        caseTemplatesQuery = caseTemplatesQuery.CustomOrderBy(pnRequestModel.Sort);
                    }
                }
                else
                {
                    caseTemplatesQuery = _dbContext.CaseTemplates.OrderBy(x => x.Id);
                }

                if (!string.IsNullOrEmpty(pnRequestModel.NameFilter))
                {
                    caseTemplatesQuery = caseTemplatesQuery.Where(x => x.Title.Contains(pnRequestModel.NameFilter));
                }

                caseTemplatesQuery
                    = caseTemplatesQuery
                        .Where(x => x.WorkflowState != Constants.WorkflowStates.Removed)
                        .Skip(pnRequestModel.Offset)
                        .Take(pnRequestModel.PageSize);

                List<CaseTemplateModel> caseTemplates = await caseTemplatesQuery.Select(x => new CaseTemplateModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
//                    CreatedBy = x.,
                    ShowFrom = x.StartAt.ToString(),
                    ShowTo = x.EndAt.ToString()
//                    Status = x.Status
                }).ToListAsync();

                caseTemplatesModel.Total = await _dbContext.CaseTemplates.CountAsync(x =>
                    x.WorkflowState != Constants.WorkflowStates.Removed);
                caseTemplatesModel.CaseTemplates = caseTemplates;

                return new OperationDataResult<CaseTemplatesModel>(true, caseTemplatesModel);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<CaseTemplatesModel>(false,
                    _baneDanmarkLaLocalizationService.GetString("ErrorObtainingLists"));
            }
        }

        public async Task<OperationResult> CreateCaseTemplate(int id)
        {
            try
            {
                Core _core = new Core();
                _core.StartSqlOnly(
                    "host= localhost;Database=759_SDK;user = root;port=3306;Convert Zero Datetime = true;SslMode=none;");

                Dictionary<int, string> laRoutes = new Dictionary<int, string>()
                {
                    {24, "Århus H - Aalborg"},
                    {25, "Aalborg - Lindholm"}
                };

                DateTime dt = DateTime.Now;
                var dayofWeek = dt.AddDays(1).ToString("dddd", new System.Globalization.CultureInfo("da-DK"));
                var dayOfWeek2 = dt.AddDays(2).ToString("dddd", new System.Globalization.CultureInfo("da-DK"));
                var date = dt.AddDays(1).ToString("dd");
                var date2 = dt.AddDays(2).ToString("dd");
                var monthOfYear = dt.ToString("MMMM", new System.Globalization.CultureInfo("da-DK"));
                var year = dt.ToString("yyyy");

                foreach (KeyValuePair<int, string> entry in laRoutes)
                {
                    CaseTemplate existingCaseTemplate = await _dbContext.CaseTemplates.SingleOrDefaultAsync(x =>
                        x.Title == entry.Value && x.StartAt.ToString("dd-MM-yyyy") == dt.AddDays(1).ToString("dd-MM-yyyy") &&
                        x.EndAt.ToString("dd-MM-yyyy") == dt.AddDays(2).ToString("dd-MM-yyyy"));

                    if (existingCaseTemplate == null)
                    {
                        string fileCheckSum = GetLa(entry.Key, _core);

                        
                        MainElement mainElement = _core.TemplateRead(9);
                        mainElement.Repeated = 1;
                        mainElement.Label = $"Banedanmark LA: {entry.Value}";
                        mainElement.ElementList[0].Label = mainElement.Label;
                        mainElement.ElementList[0].Description = new CDataValue()

                        {
                            InderValue =
                                $"Fra: {dayofWeek} den {date}. {monthOfYear} {year}<br>Til: {dayOfWeek2} den {date2}. {monthOfYear} {year}"
                        };

                        DataElement dataElement = (DataElement) mainElement.ElementList[0];
                        dataElement.DataItemList[0].Label = entry.Value;
                        ShowPdf showPdf = (ShowPdf) dataElement.DataItemList[1];
                        showPdf.Value = fileCheckSum;
                        
                        CaseTemplate newCaseTemplate = new CaseTemplate();
                        newCaseTemplate.Title = entry.Value;
                        newCaseTemplate.StartAt = dt.AddDays(1);
                        newCaseTemplate.EndAt = dt.AddDays(2);
                        newCaseTemplate.PdfTitle = fileCheckSum;
                        newCaseTemplate.Create(_dbContext);

                        List<SiteName_Dto> sites = _core.Advanced_SiteItemReadAll(false);

                        foreach (var site in sites)
                        {
                            string sdkCaseId = _core.CaseCreate(mainElement, "", site.SiteUId);

                            CaseTemplateSite caseTemplateSite = new CaseTemplateSite()
                            {
                                CaseTemplateId = newCaseTemplate.Id,
                                SdkSiteId = site.SiteUId,
                                SdkCaseId = Int32.Parse(sdkCaseId)
                            };
                        }
                    }
                }
                return new OperationResult(true, "Created successfully");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                return new OperationDataResult<CaseTemplatesModel>(false,
                    _baneDanmarkLaLocalizationService.GetString("LA creating failed") + $" {e.Message}");
            } 
        }
        
        public string GetLa(int laNumber, Core core)
        {

            DateTime twoOclock = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 0, 0);
            DateTime date = DateTime.Now;
            string dateFormatted = date.ToString("-yyyy-MM-dd-yyyy-MM-dd");

            if (DateTime.Now > twoOclock)
            {
                dateFormatted = date.AddDays(1).ToString("-yyyy-MM-dd-yyyy-MM-dd");
            }

            WebClient wc = new WebClient();
            Directory.CreateDirectory("output");
            wc.DownloadFile(
                $"https://www.bane.dk/temp/FileFetch/RouteInformationFolder/La/La-{laNumber}{dateFormatted}.pdf",
                $"output/La-{laNumber}{dateFormatted}.pdf");

            return core.PdfUpload($"output/La-{laNumber}{dateFormatted}.pdf");
        }

    }
}