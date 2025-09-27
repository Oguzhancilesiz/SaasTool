using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            inv.TaxTotal = 0m; // basit
            inv.GrandTotal = inv.Subtotal + inv.TaxTotal;

            await _uow.SaveChangesAsync();
            return inv.Id;
        }

        public async Task<InvoiceDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var q = (await _uow.Repository<Invoice>().GetAllActives())
                .Include(x => x.Lines);
            var inv = await q.FirstOrDefaultAsync(x => x.Id == id, ct);
            return inv is null ? null : _mapper.Map<InvoiceDto>(inv);
        }

        public async Task<PagedResponse<InvoiceDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct)
        {
            var q = await _uow.Repository<Invoice>().GetAllActives(); // IQueryable<Invoice>

            if (organizationId is not null)
                q = q.Where(x => x.OrganizationId == organizationId);

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(x => x.AutoID)
                               .Skip((req.Page - 1) * req.PageSize)
                               .Take(req.PageSize)
                               .Include(x => x.Lines)                                   // Include'u burada ekle
                               .ProjectToType<InvoiceDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, req.Page, req.PageSize);
        }

    }

}
