using System.Collections;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankServerApp;

public static class DepositTables
{
    public static readonly CreditDepositDraft[] depositDrafts = new CreditDepositDraft[]
    {
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 5, monthCount = 6, startsumm = 5000},
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 10, monthCount = 18, startsumm = 10000},
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 10, monthCount = 12, startsumm = 15000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(0.5), monthCount = 6, startsumm = 1000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(1), monthCount = 6, startsumm = 3000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(2), monthCount = 24, startsumm = 10000}
    };
    public static readonly CreditDepositDraft[] creditDrafts = new CreditDepositDraft[]
    {
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 20, monthCount = 6, startsumm = 5000},
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 15, monthCount = 18, startsumm = 10000},
        new CreditDepositDraft(){currency = Currencies.UAH, interestRate = 15, monthCount = 12, startsumm = 15000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(20), monthCount = 6, startsumm = 1000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(15), monthCount = 6, startsumm = 3000},
        new CreditDepositDraft(){currency = Currencies.EUR, interestRate = new decimal(15), monthCount = 24, startsumm = 10000}
    };

    public static string GetJSONDrafts() => JsonSerializer.Serialize(depositDrafts);
}

public class ValueRange
{
    public int minValue, maxValue;

    public bool IsInRange(int _valueToCheck) => _valueToCheck >= minValue && _valueToCheck <= maxValue;
}