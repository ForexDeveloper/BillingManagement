using Shared.EventBus.Contracts;

namespace Shared.EventBus.Events;

public class CmRichTenantAddedOrUpdatedEvent : IEventSign
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string OwnerName { get; set; }
    public int PrefixCode { get; set; }
    public byte IdentityType { get; set; }
    public string CreditProjectName { get; set; }
    public string BrandName { get; set; }
    public int OrganizationId { get; set; }
    public int FinancierId { get; set; }
    public int FacilitatorId { get; set; }
    public int GuarantorId { get; set; }
}
