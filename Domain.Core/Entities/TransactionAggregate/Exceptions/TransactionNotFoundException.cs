using Shared.Exception.Abstraction.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Core.Entities.TransactionAggregate.Exceptions
{
    public class TransactionNotFoundException: NotFoundException
    {
        public TransactionNotFoundException(string message):base($"{message}")
        {
            
        }
    }
}
