using System.Threading.Tasks;
using BaneDanmarkLa.Pn.Infrastructure.Models;
using BaneDanmarkLa.Pn.Infrastructure.Models.La;
using Microting.eFormApi.BasePn.Infrastructure.Models.API;

namespace BaneDanmarkLa.Pn.Abstractions
{
    public interface IBaneDanmarkLaService
    {
        Task<OperationDataResult<CaseTemplatesModel>> GetAll(CaseTemplateRequestModel pnRequestModel);
        Task<OperationResult> CreateCaseTemplate(int id);

    }
}