using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Service.EventConsumers;

public class CustomerAddedOrUpdatedEventConsumer : IConsumer<CmCustomerAddedOrUpdatedEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<CustomerAddedOrUpdatedEventConsumer> _logger;

    public CustomerAddedOrUpdatedEventConsumer(ILogger<CustomerAddedOrUpdatedEventConsumer> logger,
        ICustomerRepository customerRepository,
         IApplicationDbContextUnitOfWork unitOfWork)
    {
        _logger = logger;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<CmCustomerAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var succeed = true;
        try
        {
            var existedCustomers = await _customerRepository.GetCustomersByIds(context.Message.Customers.Select(x => x.Id).ToList(), context.Message.TenantId);
            var orgId = context.Message.OrganizationId;

            foreach (var customer in context.Message.Customers)
            {
                var existCustomer = existedCustomers.FirstOrDefault(p => p.Id == customer.Id);

                if (existCustomer == null)
                {
                    var newCustomer = new Customer(customer.Id, customer.FullName, customer.Mobile, customer.NationalId,
                        context.Message.TenantId, customer.IsConfirmShahkar, customer.UniqueIdentifier,
                        (IdentityTypeEnum)customer.CustomerType);

                    if (orgId.HasValue)
                        newCustomer.SetCustomerOrganizations(orgId.Value);

                    await _customerRepository.AddAsync(newCustomer);
                }
                else
                {
                    existCustomer.Update(customer.FullName, customer.Mobile, customer.NationalId,
                         customer.IsConfirmShahkar, customer.UniqueIdentifier,
                         (IdentityTypeEnum)customer.CustomerType);

                    if (orgId.HasValue && !existCustomer.CustomerOrganizations
                        .Any(x => x.OrganizationId == orgId))
                    {
                        existCustomer.SetCustomerOrganizations(orgId.Value);
                    }
                    _customerRepository.Update(existCustomer);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "CustomerAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = "",
                Exception = ex,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus
            });
            throw;
        }
        finally
        {
            _logger.LogTrace(new LogStruct
            {
                Message = "",
                ServiceName = "CustomerAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }
}
