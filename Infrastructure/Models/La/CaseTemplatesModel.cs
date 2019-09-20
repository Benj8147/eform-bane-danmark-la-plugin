using System.Collections.Generic;

namespace BaneDanmarkLa.Pn.Infrastructure.Models.La
{
    public class CaseTemplatesModel
    {
        public int Total { get; set; }
        public List<CaseTemplateModel> CaseTemplates { get; set; }

        public CaseTemplatesModel()
        {
            CaseTemplates = new List<CaseTemplateModel>();
        }
    }
}