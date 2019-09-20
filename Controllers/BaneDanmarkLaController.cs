using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using BaneDanmarkLa.Pn.Infrastructure.Models.La;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Controllers
{
    public class BaneDanmarkLaController: Controller
    {
        private readonly IBaneDanmarkLaService _baneDanmarkLaService;

        public BaneDanmarkLaController(IBaneDanmarkLaService baneDanmarkLaService)
        {
            _baneDanmarkLaService = baneDanmarkLaService;
        }

        [HttpGet]
        [Route("api/case-template/get-all")]
        public async Task<OperationDataResult<CaseTemplatesModel>> GetAll(CaseTemplateRequestModel pnRequestModel)
        {
            return await _baneDanmarkLaService.GetAll(pnRequestModel);
        }

        [HttpPost]
        [Route("api/case-template/create-case-template")]
        public async Task<OperationResult> CreateCaseTemplate(int id)
        {
            return await _baneDanmarkLaService.CreateCaseTemplate(id);
        }

    }
}