using System.Diagnostics;
using System.Text.Json;

namespace BankServerApp;

public class Bank
{
    public List<Account> m_registeredAccounts { get; set; }
    public List<Deposit> m_Deposits { get; set; }
    public List<Transaction> m_processingTransactionsSet { get; set; }
    public List<Card> m_Cards { get; set; }

    private const string ACCOUNTS_SAVEPATH = "/registeredAccounts.json";
    private const string CARDS_SAVEPATH = "/allCards.json";
    private const string TRANSACTIONSSAVEFOLDER = "/Transactions";



    public void AddTransactionToDatabase(Transaction _transaction, TransactionTypes _transactionType)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                            TRANSACTIONSSAVEFOLDER;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string transactionSaveFileName = _transactionType == TransactionTypes.Income
            ? _transaction.recieverAccountName
            : _transaction.senderAccountName;
        string transactionSavePath = $"{folderPath}/{transactionSaveFileName}.json";
        if (!File.Exists(transactionSavePath))
        {
            using FileStream stream = new FileStream(transactionSavePath, FileMode.Create);
            stream.Dispose();
            // m_processingTransactionsSet.Add(_transaction);
            // File.WriteAllTextAsync(transactionSavePath, JsonSerializer.Serialize(m_registeredAccounts));
            // return;
        }

        m_processingTransactionsSet = new List<Transaction>();
        try
        {
            using (StreamReader reader = new StreamReader(transactionSavePath))
            {
                m_processingTransactionsSet = (JsonSerializer.Deserialize<Transaction[]>(reader.ReadToEnd()) ??
                                               Array.Empty<Transaction>()).ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        m_processingTransactionsSet.Add(_transaction);
        File.WriteAllTextAsync(transactionSavePath, JsonSerializer.Serialize(m_processingTransactionsSet));
    }

    public void UpdateUserDatabase()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + ACCOUNTS_SAVEPATH;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_registeredAccounts));
    }

    public Card GetCardOfNumber(int _cardNumber)
    {
        return m_Cards.Find(x => x.cardNumber == _cardNumber);
    }

    public void GiveDepositPayments()
    {
        foreach (var deposit in m_Deposits)
        {
            if (deposit.lastPayoutDate.Day == DateTime.Now.Day)
            {
                PayDepositMoney(deposit);
            }
        }
    }
    
    public List<Card> GetCardsOfUser(string _accountName)
    {
        Account account = m_registeredAccounts.Find(x => x.accountName == _accountName);
        return m_Cards.FindAll(x => account.cards.Contains(x.cardNumber));
    }

    public void UpdateCardsDatabase()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + CARDS_SAVEPATH;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_Cards));
    }

    public Transaction[]? GetUsersTransactions(string _accountName)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                            TRANSACTIONSSAVEFOLDER;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string transactionSavePath = $"{folderPath}/{_accountName}.json";
        if (File.Exists(transactionSavePath))
        {
            using (StreamReader reader = new StreamReader(transactionSavePath))
            {
                return JsonSerializer.Deserialize<Transaction[]>(reader.ReadToEnd());
            }
        }

        return Array.Empty<Transaction>();
    }

    private void GenerateCards()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1000; j < 10000; j++)
            {
                m_Cards.Add(new Card(Int32.Parse($"2{i}33{j}"), (Currencies)i));
            }
        }
    }

    public void PayDepositMoney(Deposit _deposit)
    {
        m_registeredAccounts.Find(x => x.depositIDs.Contains(_deposit.depositID));
    }
    
    public void OpenDeposit(string _openerAccount, int _startPayment, Currencies _currency, int _monthCount)
    {
        Account account = m_registeredAccounts.Find(x => x.accountName == _openerAccount);
        Card card = GetCardsOfUser(account.accountName).Find(x => x.currency == _currency);

        try
        {
            if (!card.isCurrentlyFreesed && card.cardBalance >= _startPayment)
            {
                account.ProcessTransaction(new Transaction(_startPayment, "Deposit", account.accountName,
                    card.cardNumber, CentralAccount.cardNumbers[(int)card.currency]));
                Deposit newDeposit = new Deposit(_startPayment, _monthCount);
                m_Deposits.Add(newDeposit);
                account.depositIDs.Add(newDeposit.depositID);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine();
        }
    }

    public Bank()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        string path;

        #region AccountsLoad

        path = Environment.GetFolderPath(folder) + ACCOUNTS_SAVEPATH;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                m_registeredAccounts = (JsonSerializer.Deserialize<Account[]>(reader.ReadToEnd()) ??
                                        throw new InvalidOperationException()).ToList();
            }
        }
        else
        {
            m_registeredAccounts = new List<Account>();
            using FileStream stream = new FileStream(path, FileMode.Create);
            stream.Dispose();
        }
        #endregion

        #region CardsLoad

        path = Environment.GetFolderPath(folder) + CARDS_SAVEPATH;  
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                m_Cards = (JsonSerializer.Deserialize<Card[]>(reader.ReadToEnd()) ??
                           throw new InvalidOperationException()).ToList();
            }
        }
        else
        {
            m_Cards = new List<Card>();
            GenerateCards();
            using FileStream stream = new FileStream(path, FileMode.Create);
            stream.Dispose();
        }

        #endregion
    }
}