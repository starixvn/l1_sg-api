using System.Threading.Tasks;

namespace SG.Core.Features.Operators
{
    public interface IOperatorDataService
    {
        Task<Operator> GetById(string operatorId);

        Task<bool> VerifyWithCurrency(string operatorId, string currency);
    }

    public class OperatorDataService : IOperatorDataService
    {
        private readonly IOperatorDataAccess operatorDataAccess;
        private readonly ICurrencyDataAccess currencyDataAccess;

        public OperatorDataService(IOperatorDataAccess operatorDataAccess, ICurrencyDataAccess currencyDataAccess)
        {
            this.operatorDataAccess = operatorDataAccess;
            this.currencyDataAccess = currencyDataAccess;
        }

        public Task<Operator> GetById(string operatorId)
        => string.IsNullOrWhiteSpace(operatorId) ? Task.FromResult<Operator>(default) : operatorDataAccess.GetById(operatorId);

        public Task<bool> VerifyWithCurrency(string operatorId, string currency)
            => currencyDataAccess.Verify(operatorId, currency);
    }
}