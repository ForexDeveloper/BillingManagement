using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.CashOut;

public class GetCashOutListModel:BasePaginatedListRequest
{
    public CashOutRequestStatus  Status { get; set; }

}