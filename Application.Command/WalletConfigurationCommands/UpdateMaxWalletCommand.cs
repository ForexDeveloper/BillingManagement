using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.CurrencyTypeAggregate.Exceptions;
using Domain.Core.Entities.CurrencyTypeAggregate;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.ProjectManegerAggregate.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletConfigurationAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.Entities;

namespace Application.Command.WalletConfigurationCommands
{
    public class UpdateMaxWalletCommand : IRequest<int>
    {
        public UpdateMaxWalletCommand(int id, decimal maxWallet, int? tenantId = null)
        {
            Id = id;
            MaxWallet = maxWallet;
            TenantId = tenantId;
        }

        public int Id { get; set; }
        public decimal MaxWallet { get; set; }
        public int? TenantId { get; set; }

    }

    public class UpdateMaxWalletCommandHandler : IRequestHandler<UpdateMaxWalletCommand, int>
    {
        private readonly IWalletConfigurationRepository _walletConfigurationRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        public UpdateMaxWalletCommandHandler(IWalletConfigurationRepository walletConfigurationRepository, IApplicationDbContextUnitOfWork unitOfWork)
        {
            _walletConfigurationRepository = walletConfigurationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(UpdateMaxWalletCommand request, CancellationToken cancellationToken)
        {
            var wealletConfiguration = await _walletConfigurationRepository.GetByIdAsync(request.Id);
            if (wealletConfiguration is null)
                throw new WalletConfigurationNotFoundException("کانفیگ کیف پول پول نشد.");

            if (request.TenantId.HasValue && request.TenantId != wealletConfiguration.TenantId)
            {
                throw new TenantForbiddenException();
            }

            wealletConfiguration.SetMaxWallet(request.MaxWallet);

            _walletConfigurationRepository.Update(wealletConfiguration);
            await _unitOfWork.SaveChangesAsync();
            return wealletConfiguration.Id;
        }
    }
}