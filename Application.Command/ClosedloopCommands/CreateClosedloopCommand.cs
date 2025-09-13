using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.ClosedloopCommands
{
    public class CreateClosedloopCommand : IRequest<int>
    {
        public CreateClosedloopCommand(int walletConfigurationId, int tenantId, string title, List<int> categories, List<int> merchants)
        {
            WalletConfigurationId = walletConfigurationId;
            TenantId = tenantId;
            Title = title;
            Categories = categories;
            Merchants = merchants;
        }

        public int WalletConfigurationId { get; set; }
        public int TenantId { get; set; }
        public string Title { get; set; }
        public List<int> Categories { get; set; }
        public List<int> Merchants { get; set; }
    }

    public class CreateClosedloopCommandHandler : IRequestHandler<CreateClosedloopCommand, int>
    {

        private readonly ITenantRepository _tenantRepository;
        private readonly IWalletConfigurationRepository _walletConfigurationRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IClosedloopRepository _closedloopRepository;

        public CreateClosedloopCommandHandler(IClosedloopRepository closedloopRepository, IMerchantRepository merchantRepository, ICategoryRepository categoryRepository,
            ICurrentUserService currentUserService, ITenantRepository tenantRepository,
           IWalletConfigurationRepository walletConfigurationRepository, IApplicationDbContextUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _walletConfigurationRepository = walletConfigurationRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _categoryRepository = categoryRepository;
            _merchantRepository = merchantRepository;
            _closedloopRepository = closedloopRepository;
        }

        public async Task<int> Handle(CreateClosedloopCommand request, CancellationToken cancellationToken)
        {
            await CheckValidation(request);

            var closedloop = new Closedloop(request.WalletConfigurationId, request.Title);
            if (request.Merchants != null && request.Merchants.Any())
                closedloop.SetClosedloopMerchant(request.Merchants.Select(c => new ClosedloopMerchant(closedloop.Id, c)).ToList());

            if (request.Categories != null && request.Categories.Any())
                closedloop.SetClosedloopCategory(request.Categories.Select(c => new ClosedloopCategory(closedloop.Id, c)).ToList());

            closedloop.SetCreatorUserId(_currentUserService.UserId);
            closedloop.SetClientId(_currentUserService.ClientId);

            await _closedloopRepository.AddAsync(closedloop);
            await _unitOfWork.SaveChangesAsync();
            return closedloop.Id;
        }

        private async Task CheckValidation(CreateClosedloopCommand request)
        {
            var hasTenant = await _tenantRepository.CheckTenant(request.TenantId);
            if (!hasTenant)
                throw new ArgumentValidationException(nameof(request.TenantId), "شناسه پذیرنده معتبر نیست");

            var hasWalletConfiguration = await _walletConfigurationRepository.CheckWalletConfigurationByTenantIdAsync(request.WalletConfigurationId, request.TenantId);
            if (!hasWalletConfiguration)
                throw new ArgumentValidationException(nameof(request.WalletConfigurationId), "شناسه تنظیمات کیف پول معتبر نیست");

            if (request.Categories != null && request.Categories.Any())
            {
                var hasCategory = await _categoryRepository.CheckCategoryAsync(request.Categories);
                if (!hasCategory)
                    throw new ArgumentValidationException(nameof(request.Categories), "شناسه دسته‌بندی پذیرنده‌ها معتبر نیست.");
            }

            if (request.Merchants != null && request.Merchants.Any())
            {
                var hasMerchant = await _merchantRepository.CheckMerchantByTenantIdAsync(request.TenantId, request.Merchants);
                if (!hasMerchant)
                    throw new ArgumentValidationException(nameof(request.Merchants), "شناسه پذیرنده  معتبر نیست.");
            }

        }
    }
}
