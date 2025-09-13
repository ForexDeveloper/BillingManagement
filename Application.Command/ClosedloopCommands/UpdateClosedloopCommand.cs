using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.ClosedloopAggregate.Exceptions;
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
    public class UpdateClosedloopCommand : IRequest<int>
    {
        public UpdateClosedloopCommand(int id, int walletConfigurationId, int tenantId, string title, List<int> categories, List<int> merchants)
        {
            Id = id;
            WalletConfigurationId = walletConfigurationId;
            TenantId = tenantId;
            Title = title;
            Categories = categories;
            Merchants = merchants;
        }
        public int Id { get; set; }
        public int WalletConfigurationId { get; set; }
        public int TenantId { get; set; }
        public string Title { get; set; }
        public List<int> Categories { get; set; }
        public List<int> Merchants { get; set; }
    }

    public class UpdateClosedloopCommandHandler : IRequestHandler<UpdateClosedloopCommand, int>
    {

        private readonly ITenantRepository _tenantRepository;
        private readonly IWalletConfigurationRepository _walletConfigurationRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IClosedloopRepository _closedloopRepository;

        public UpdateClosedloopCommandHandler(IClosedloopRepository closedloopRepository, IMerchantRepository merchantRepository, ICategoryRepository categoryRepository,
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

        public async Task<int> Handle(UpdateClosedloopCommand request, CancellationToken cancellationToken)
        {
            request.Categories ??= new List<int>();
            request.Merchants ??= new List<int>();

            var closedloop = await _closedloopRepository.GetByIdAsync(request.Id);
            if (closedloop is null)
                throw new ClosedloopNotFoundException("شناسه  معتبر نیست");

            if (closedloop.WalletConfigurationId!= request.WalletConfigurationId)
                throw new ArgumentValidationException(nameof(request.WalletConfigurationId),"امکان ویرایش پیکربندی نیست.");


            await CheckValidation(request);

            closedloop.SetClosedloop(request.WalletConfigurationId, request.Title);
            SetCategories(request.Categories, closedloop);
            SetMerchants(request.Merchants, closedloop);

            closedloop.SetCreatorUserId(_currentUserService.UserId);
            closedloop.SetClientId(_currentUserService.ClientId);

            _closedloopRepository.Update(closedloop);
            await _unitOfWork.SaveChangesAsync();
            return closedloop.Id;
        }

        private async Task CheckValidation(UpdateClosedloopCommand request)
        {
            var hasTenant = await _tenantRepository.CheckTenant(request.TenantId);
            if (!hasTenant)
                throw new ArgumentValidationException(nameof(request.TenantId), "شناسه پذیرنده معتبر نیست");

            var hasWalletConfiguration = await _walletConfigurationRepository.CheckWalletConfigurationByTenantIdAsync(request.WalletConfigurationId, request.TenantId);
            if (!hasWalletConfiguration)
                throw new ArgumentValidationException(nameof(request.WalletConfigurationId), "شناسه تنظیمات کیف پول معتبر نیست");
            if (request.Categories.Any())
            {
                var hasCategory = await _categoryRepository.CheckCategoryAsync(request.Categories);
                if (!hasCategory)
                    throw new ArgumentValidationException(nameof(request.Categories), "شناسه دسته‌بندی پذیرنده‌ها معتبر نیست.");
            }
           
            if (request.Merchants.Any())
            {
                var hasMerchant = await _merchantRepository.CheckMerchantByTenantIdAsync(request.TenantId, request.Merchants);
                if (!hasMerchant)
                    throw new ArgumentValidationException(nameof(request.Merchants), "شناسه پذیرنده  معتبر نیست.");
            }
          
        }

        private void SetCategories(List<int> categories, Closedloop closedloop)
        {
            var closedloopCategories = closedloop.ClosedloopCategories.Where(c => !categories.Contains(c.CategoryId)).ToList();
            foreach (var item in closedloopCategories)
            {
                item.SetDeleted();
            }
            foreach (var category in categories)
            {
                if (!closedloop.ClosedloopCategories.Any(c => c.CategoryId == category))
                {
                    closedloop.ClosedloopCategories.Add(new ClosedloopCategory(closedloop.Id, category));
                }
            }
        }

        private void SetMerchants(List<int> merchants, Closedloop closedloop)
        {
            var closedloopCategories = closedloop.ClosedloopMerchants.Where(c => !merchants.Contains(c.MerchantId)).ToList();
            foreach (var item in closedloopCategories)
            {
                item.SetDeleted();
            }
            foreach (var merchant in merchants)
            {
                if (!closedloop.ClosedloopMerchants.Any(c => c.MerchantId == merchant))
                {
                    closedloop.ClosedloopMerchants.Add(new ClosedloopMerchant(closedloop.Id, merchant));
                }
            }
        }
    }
}
