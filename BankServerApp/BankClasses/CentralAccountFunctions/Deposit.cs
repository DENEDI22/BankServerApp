using System.Text.Json.Serialization;

namespace BankServerApp;

public class Deposit
{
    public readonly int depositID;
    public readonly int depositStartSumm;
    public readonly DateTime depositOpenDate, depositEndDate, lastPayoutDate;
    public readonly bool canBeTerminated;
    public readonly decimal interestRate;
    public readonly Currencies currency;
    
    [JsonConstructor]
    public Deposit(int DepositId, int DepositStartSumm, DateTime DepositOpenDate, DateTime DepositEndDate, DateTime LastPayOutDate, bool CanBeTerminated, decimal InterestRate, int Currency)
    {
        depositID = DepositId;
        depositStartSumm = DepositStartSumm;
        depositOpenDate = DepositOpenDate;
        depositEndDate = DepositEndDate;
        lastPayoutDate = LastPayOutDate;
        canBeTerminated = CanBeTerminated;
        interestRate = InterestRate;
        currency = (Currencies)Currency;
    }

    public Deposit(CreditDepositDraft _draft)
    {
        depositID = new Random().Next();
        depositStartSumm = _draft.startsumm;
        depositOpenDate = DateTime.Today;
        depositEndDate = DateTime.Today + TimeSpan.FromDays(28 * (_draft.monthCount));
        lastPayoutDate = DateTime.Today;
        interestRate = _draft.interestRate;
    }
    
    public Deposit(int _depositStartSumm, int _monthCount)
    {
        depositID = new Random().Next();
        depositStartSumm = _depositStartSumm;
        depositOpenDate = DateTime.Today;
        depositEndDate = DateTime.Today + TimeSpan.FromDays(28 * _monthCount);
        lastPayoutDate = DateTime.Today;
    }
}

[Serializable]
public struct CreditDepositDraft
{
    [JsonInclude] public int monthCount, startsumm;
    [JsonInclude] public decimal interestRate;
    [JsonInclude] public Currencies currency;
}