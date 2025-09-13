using Domain.Base;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;

namespace Domain.Core.Entities.FinancierAggregate;
public class FinancierCreditFlowConfig : BaseEntity<int>
{
    public FinancierCreditFlowEnum Type { get; private set; }
    public string ProductCode { get; private set; }
    public string ClientId { get; private set; }
    public string ClientSecret { get; private set; }
    public int FinancierId { get; private set; }
    public Financier Financier { get; private set; }

    private FinancierCreditFlowConfig()
    {
        
    }

    public FinancierCreditFlowConfig(int id, string productCode, string clientId, string clientSecret, byte type)
    {
        SetFinancierCreditFlowConfig(productCode, clientId, clientSecret);
        Id = id;
        Type = (FinancierCreditFlowEnum)type;
    }

    public void SetFinancierCreditFlowConfig(string productCode, string clientId, string clientSecret)
    {

        if (string.IsNullOrEmpty(productCode))
        {
            throw new ArgumentValidationException(productCode, "وارد نمودن کد محصول اجباریست.");
        }

        if (string.IsNullOrEmpty(clientId))
        {
            throw new ArgumentValidationException(clientId, "وارد نمودن شناسه مشتری اجباریست.");
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new ArgumentValidationException(clientSecret, "وارد نمودن رمز مشتری اجباریست.");
        }

        ProductCode = productCode;
        ClientId = clientId;
        ClientSecret = clientSecret;
    }
}
