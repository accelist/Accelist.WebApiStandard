namespace Accelist.WebApiStandard.Entities.EntityDescriptors
{
    public interface IHaveCreateAndUpdateAudit : IHaveCreateOnlyAudit
    {
        public DateTime UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
