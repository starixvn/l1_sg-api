using System.Threading.Tasks;

namespace SG.Core.Features.Operators
{
    public interface ICurrencyGroupDataService
    {
        Task<CurrencyGroup> GetById(string currencyGroupId);
    }

    public class CurrencyGroupDataService : ICurrencyGroupDataService
    {
        private readonly ICurrencyGroupDataAccess currencyGroupDataAccess;

        public CurrencyGroupDataService(ICurrencyGroupDataAccess currencyGroupDataAccess, ICurrencyDataAccess currencyDataAccess)
        {
            this.currencyGroupDataAccess = currencyGroupDataAccess;
        }

        public Task<CurrencyGroup> GetById(string currencyGroupId)
        => string.IsNullOrWhiteSpace(currencyGroupId) ? Task.FromResult<CurrencyGroup>(default) : currencyGroupDataAccess.GetById(currencyGroupId);
    }
}