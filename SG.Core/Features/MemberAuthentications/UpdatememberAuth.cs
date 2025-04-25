namespace SG.Core.Features.MemberAuthentications
{
    public class UpdatememberAuth
    {
        public UpdatememberAuth(string token, string operatorId, string currencyGroupId, int? gameTypeId)
        {
            this.Token = token;
            this.OperatorId = operatorId;
            this.CurrencyGroupId = currencyGroupId;
            this.GameId = gameTypeId;
        }
        public string OperatorId { get; set; }
        public string Token { get; set; }
        public string CurrencyGroupId {  get; set; }
        public int? GameId { get; set; }
    }
}
