using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Common;
using SaasTool.DTO.Orgs;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete
{
    public sealed class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
        public CustomerService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(CustomerCreateDto dto, CancellationToken ct)
        {
            var e = _mapper.Map<Customer>(dto);
            await _uow.Repository<Customer>().AddAsync(e);
            await _uow.SaveChangesAsync();
            return e.Id;
        }

        public async Task UpdateAsync(Guid id, CustomerUpdateDto dto, CancellationToken ct)
        {
            var repo = _uow.Repository<Customer>();
            var e = await repo.GetById(id) ?? throw new InvalidOperationException("Customer not found.");
            _mapper.Map(dto, e);
            await repo.Update(e);
            await _uow.SaveChangesAsync();
        }

        public async Task<CustomerDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var e = await _uow.Repository<Customer>().GetById(id);
            return e is null ? null : _mapper.Map<CustomerDto>(e);
        }

        public async Task<PagedResponse<CustomerDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();
            var q = (await _uow.Repository<Customer>().GetAllActives()).AsNoTracking();

            if (organizationId is not null) q = q.Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrWhiteSpace(n.Search))
                q = q.Where(x => x.Name.Contains(n.Search) || (x.Email ?? "").Contains(n.Search));

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.AutoID)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .ProjectToType<CustomerDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, n.Page, n.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<Customer>();
            var e = await repo.GetById(id);
            if (e is null) return;
            await repo.Delete(e);
            await _uow.SaveChangesAsync();
        }
    }
}
