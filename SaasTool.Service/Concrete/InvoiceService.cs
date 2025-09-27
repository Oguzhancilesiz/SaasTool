using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete
{
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
        public InvoiceService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(InvoiceCreateDto dto, CancellationToken ct)
        {
            var inv = _mapper.Map<Invoice>(dto);
            await _uow.Repository<Invoice>().AddAsync(inv);

            decimal subtotal = 0m;
            if (dto.Lines is { Count: > 0 })
            {
                foreach (var l in dto.Lines)
                {
                    var line = _mapper.Map<InvoiceLine>(l);
                    line.InvoiceId = inv.Id;
                    subtotal += (l.Quantity * l.UnitPrice);
                    await _uow.Repository<InvoiceLine>().AddAsync(line);
                }
            }

            inv.Subtotal = subtotal;
            inv.TaxTotal = 0m;
            inv.GrandTotal = inv.Subtotal + inv.TaxTotal;

            await _uow.SaveChangesAsync();
            return inv.Id;
        }

        public async Task<InvoiceDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var q = (await _uow.Repository<Invoice>().GetAllActives())
                .AsNoTracking()
                .Include(x => x.Lines);

            var inv = await q.FirstOrDefaultAsync(x => x.Id == id, ct);
            return inv is null ? null : _mapper.Map<InvoiceDto>(inv);
        }

        public async Task<PagedResponse<InvoiceDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();
            var q = (await _uow.Repository<Invoice>().GetAllActives()).AsNoTracking();

            if (organizationId is not null)
                q = q.Where(x => x.OrganizationId == organizationId);

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(x => x.AutoID)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .Include(x => x.Lines)
                               .ProjectToType<InvoiceDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, n.Page, n.PageSize);
        }
    }
}
