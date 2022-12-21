using System.Text.Json.Serialization;

namespace BankServerApp;

public class CreditDepositData
{
    public int DepositId { get; }
    public int DepositStartSumm { get; }
    public DateTime DepositOpenDate { get; }
    public DateTime DepositEndDate { get; }
    public DateTime lastPayoutDate { get; set; }

    /// <summary>
    /// use only in credits
    /// </summary>
    public decimal creditPayoutLeft { get; set; }
    public bool canBeTerminated { get; }
    public decimal interestRate { get; }
    public Currencies currency { get; }

    [JsonConstructor]
    public CreditDepositData(int DepositId, int DepositStartSumm, DateTime DepositOpenDate, DateTime DepositEndDate,
        DateTime LastPayOutDate, bool CanBeTerminated, decimal InterestRate, int Currency, int CreditPayoutLeft)
    {
        this.DepositId = DepositId;
        this.DepositStartSumm = DepositStartSumm;
        this.DepositOpenDate = DepositOpenDate;
        this.DepositEndDate = DepositEndDate;
        lastPayoutDate = LastPayOutDate;
        canBeTerminated = CanBeTerminated;
        interestRate = InterestRate;
        currency = (Currencies)Currency;
        creditPayoutLeft = CreditPayoutLeft;
    }

    public CreditDepositData(CreditDepositDraft _draft)
    {
        DepositId = new Random().Next();
        DepositStartSumm = _draft.startsumm;
        DepositOpenDate = DateTime.Today;
        DepositEndDate = DateTime.Today + TimeSpan.FromDays(28 * (_draft.monthCount));
        lastPayoutDate = DateTime.Today;
        interestRate = _draft.interestRate;
        creditPayoutLeft = _draft.startsumm + _draft.monthCount * (DepositStartSumm * interestRate / 12);
    }

    public CreditDepositData(int _depositStartSumm, int _monthCount, decimal _interestRate)
    {
        DepositId = new Random().Next();
        DepositStartSumm = _depositStartSumm;
        DepositOpenDate = DateTime.Today;
        DepositEndDate = DateTime.Today + TimeSpan.FromDays(28 * _monthCount);
        lastPayoutDate = DateTime.Today;
        interestRate = _interestRate;
    }
}

[Serializable]
public struct CreditDepositDraft
{
    [JsonInclude] public int monthCount, startsumm;
    [JsonInclude] public decimal interestRate;
    [JsonInclude] public Currencies currency;
}