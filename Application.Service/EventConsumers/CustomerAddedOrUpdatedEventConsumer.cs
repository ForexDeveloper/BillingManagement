using System;
using System.Linq;
using MassTransit;
using Domain.Core.Enums;
using System.Diagnostics;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Domain.Core.UnitOfWorkContracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Abstraction.Extensions;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Service.EventConsumers;

public sealed class CustomerAddedOrUpdatedEventConsumer(
    ICustomerRepository customerRepository,
    IApplicationDbContextUnitOfWork unitOfWork,
    ILogger<CustomerAddedOrUpdatedEventConsumer> logger) : IConsumer<CmCustomerAddedOrUpdatedEvent>
{
    public async Task Consume(ConsumeContext<CmCustomerAddedOrUpdatedEvent> context)
    {
        var succeed = true;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        const string SERVICE_NAME = $"{nameof(CustomerAddedOrUpdatedEventConsumer)}_{nameof(Consume)}";
        try
        {
            var existedCustomers = await customerRepository.GetCustomersByIds(context.Message.Customers.Select(x => x.Id).ToList(), context.Message.TenantId);

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

                    await customerRepository.AddAsync(newCustomer);
                }
                else
                {
                    existCustomer.Update(customer.FullName, customer.Mobile, customer.NationalId,
                         customer.IsConfirmShahkar, customer.UniqueIdentifier,
                         (IdentityTypeEnum)customer.CustomerType);

                    if (orgId.HasValue && existCustomer.CustomerOrganizations.All(x => x.OrganizationId != orgId))
                    {
                        existCustomer.SetCustomerOrganizations(orgId.Value);
                    }

                    customerRepository.Update(existCustomer);
                }
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            succeed = false;

            if (exception is IBusinessException)
            {
                logger.LogWarning(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch
                });
            }
            else
            {
                logger.LogCritical(new LogStruct
                {
                    Exception = exception,
                    Results = string.Empty,
                    ServiceName = SERVICE_NAME,
                    Message = exception.Message,
                    InputParams = context.Message,
                    Tags = LogMessageTag.EventBus,
                    ResponseTimeStopWatcher = stopWatch,
                });

                throw;
            }
        }
        finally
        {
            logger.LogTrace(new LogStruct
            {
                Results = succeed,
                Message = string.Empty,
                ServiceName = SERVICE_NAME,
                InputParams = context.Message,
                Tags = LogMessageTag.EventBus,
                ResponseTimeStopWatcher = stopWatch
            });
        }
    }
}