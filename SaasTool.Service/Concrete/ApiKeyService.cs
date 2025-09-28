using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Common;
using SaasTool.DTO.Security;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Concrete
{
    public sealed class ApiKeyService : IApiKeyService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IConfiguration _cfg;

        public ApiKeyService(IUnitOfWork uow, IMapper mapper, IConfiguration cfg)
        { _uow = uow; _mapper = mapper; _cfg = cfg; }

        public async Task<ApiKeyCreatedDto> CreateAsync(ApiKeyCreateDto dto, CancellationToken ct)
        {
            // Generate key: prefix + random
            var prefix = "sk_live_"; // dev: sk_test_
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                              .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var key = prefix + raw;

            var entity = _mapper.Map<ApiKey>(dto);
            entity.KeyHash = Hash(key);
            await _uow.Repository<ApiKey>().AddAsync(entity);
            await _uow.SaveChangesAsync();

            var last4 = key.Length >= 4 ? key[^4..] : "****";
            return new ApiKeyCreatedDto { Id = entity.Id, Key = key, KeyLast4 = last4 };
        }

        public async Task RevokeAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<ApiKey>();
            var e = await repo.GetById(id) ?? throw new InvalidOperationException("ApiKey not found.");
            e.IsRevoked = true;
            await repo.Update(e);
            await _uow.SaveChangesAsync();
        }

        public async Task<PagedResponse<ApiKeyDto>> ListAsync(Guid? orgId, Guid? appId, PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();

            // 1) Taban sorgu
            IQueryable<ApiKey> baseQ = (await _uow.Repository<ApiKey>().GetAllActives())
                .AsNoTracking();

            // 2) Filtreler
            if (orgId is not null) baseQ = baseQ.Where(x => x.OrganizationId == orgId);
            if (appId is not null) baseQ = baseQ.Where(x => x.AppId == appId);

            // 3) Include’lar EN SON
            var q = baseQ
                .Include(x => x.Organization)
                .Include(x => x.App);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.CreatedDate)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .ProjectToType<ApiKeyDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, n.Page, n.PageSize);
        }

        private string Hash(string value)
        {
            var secret = _cfg["ApiKeys:HashSecret"] ?? "ChangeMeNow";
            using var h = SHA256.Create();
            var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(value + secret));
            return Convert.ToHexString(bytes); // upper-case hex
        }
    }

}
