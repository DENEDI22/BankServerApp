using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BankServerApp;

[Serializable]
public class CentralAccount
{
    public static readonly int[] cardNumbers = new int[]
    {
        20321,
        21321,
        22321,
        23321,
        24321
    };

    public readonly Card[] balances = new[]
    {
        new Card(20321, 0, (int)Currencies.USD, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
        new Card(21321, 0, (int)Currencies.EUR, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
        new Card(22321, 0, (int)Currencies.UAH, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
        new Card(23321, 0, (int)Currencies.GBP, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
        new Card(24321, 0, (int)Currencies.PLN, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
    };

    private const string CENTRAL_ACCOUNT_DATA_SAVEPATH = @"\centralAccount.json";


    private Transaction RequestDepositPayout(Account _account, CreditDepositData _creditDepositData)
    {
        int paymentInterval = (DateTime.Today - _creditDepositData.lastPayoutDate).Days;
        decimal depositPaymentAmount = paymentInterval * _creditDepositData.interestRate / paymentInterval;
        int recieverCard = _account.Cards.Find(x => x.ToString().ToCharArray()[1] == (int)_creditDepositData.currency);
        return new Transaction(depositPaymentAmount, _account.AccountName, "Deposit Payout", recieverCard,
            balances[(int)_creditDepositData.currency].cardNumber);
    }

    /// <summary>
    /// Make 
    /// </summary>
    /// <param name="_jsonInput">Card array in string format JSON</param>
    /// <exception cref="InvalidOperationException"></exception>
    public CentralAccount(string _jsonInput)
    {
        balances = JsonSerializer.Deserialize<Card[]>(_jsonInput) ?? throw new InvalidOperationException();
    }

    public static void ProcessTranasactionStatic(Transaction _transaction)
    {
        var newCA = new CentralAccount();
        newCA.ProcessTransaction(_transaction);
        newCA.UpdateSaveFile();
    }

    private void UpdateSaveFile()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + CENTRAL_ACCOUNT_DATA_SAVEPATH;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(balances));
    }
    
    private void ProcessTransaction(Transaction _transaction)
    {
        List<Card> cardslist = balances.ToList();
        int indexOfCard;
        if (cardslist.Exists(x => x.cardNumber == _transaction.recieverCard))
        {
            indexOfCard = cardslist.FindIndex(x => x.cardNumber == _transaction.recieverCard);
            balances[indexOfCard].AddMoney(_transaction.transactionAmount, balances[indexOfCard].currency);
        }
        else if (cardslist.Exists(x => x.cardNumber == _transaction.senderCard))
        {
            indexOfCard = cardslist.FindIndex(x => x.cardNumber == _transaction.recieverCard);
            balances[indexOfCard].AddMoney(-_transaction.transactionAmount, balances[indexOfCard].currency);
        }
    }

    private CentralAccount()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + CENTRAL_ACCOUNT_DATA_SAVEPATH;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                balances = JsonSerializer.Deserialize<Card[]>(reader.ReadToEnd());
            }
        }
        else
        {
            balances = new[]
            {
                new Card(20321, 0, (int)Currencies.USD, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
                new Card(21321, 0, (int)Currencies.EUR, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
                new Card(22321, 0, (int)Currencies.UAH, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
                new Card(23321, 0, (int)Currencies.GBP, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
                new Card(24321, 0, (int)Currencies.PLN, new DateOnly(9998, 12, 1).ToString(), true, 100000000, false),
            };
            using FileStream stream = new FileStream(path, FileMode.Create);
            stream.Dispose();
        }
    }
}