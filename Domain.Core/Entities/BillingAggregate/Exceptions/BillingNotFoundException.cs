using Shared.Exception.Abstraction.Domain;
using Domain.Core.Entities.Shared.Exceptions;

namespace Domain.Core.Entities.BillingAggregate.Exceptions;

public class BillingNotFoundException(string message) : NotFoundException($"{message}"), IBusinessException;