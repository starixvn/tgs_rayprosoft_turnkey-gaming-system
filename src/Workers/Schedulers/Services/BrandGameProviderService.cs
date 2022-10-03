using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandGameProviderService
{
    Task<BrandGameProvider> GetBrandGameProviderAsync(
        Expression<Func<BrandGameProvider, bool>> predicate,
        Func<IQueryable<BrandGameProvider>, IQueryable<BrandGameProvider>> preQuery =
            null);

    IQueryable<BrandGameProvider> GetBrandGameProviderQueries(
        Func<IQueryable<BrandGameProvider>, IQueryable<BrandGameProvider>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandGameProviderAsync(BrandGameProvider brandGameProvider);
    Task UpdateBrandGameProvidersAsync(string toMaintenanceIds, string toActiveIds);
}

public class BrandGameProviderService : BaseRepositoryOperations<ITgsRepository>,
    IBrandGameProviderService
{
    private readonly ITgsRepository _repository;

    public BrandGameProviderService(ITgsRepository tgsRepository) : base(tgsRepository)
        => _repository = tgsRepository;

    Task<BrandGameProvider> IBrandGameProviderService.GetBrandGameProviderAsync(
        Expression<Func<BrandGameProvider, bool>> predicate,
        Func<IQueryable<BrandGameProvider>, IQueryable<BrandGameProvider>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<BrandGameProvider> IBrandGameProviderService.GetBrandGameProviderQueries(
        Func<IQueryable<BrandGameProvider>, IQueryable<BrandGameProvider>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task IBrandGameProviderService.UpdateBrandGameProviderAsync(
        BrandGameProvider brandGameProvider) =>
        UpdateEntityAsync(brandGameProvider);

    Task IBrandGameProviderService.UpdateBrandGameProvidersAsync(string toMaintenanceIds, string toActiveIds)
    {
        const string spName = "dbo.brand_game_providers_status_changes";
        return _repository
            .SqlQuerySingleAsync<object>(spName,
                parameters:
                new Dictionary<string, object>
                {
                    { "_to_maintenance_text", toMaintenanceIds },
                    { "_to_active_text", toActiveIds }
                });
    }
}