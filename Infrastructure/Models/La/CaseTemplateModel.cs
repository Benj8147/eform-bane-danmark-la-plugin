using System;

namespace BaneDanmarkLa.Pn.Infrastructure.Models.La
{
    public class CaseTemplateModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string Title { get; set; }
        public string ShowFrom { get; set; }
        public string ShowTo { get; set; }
        public bool Status { get; set; }
    }
}