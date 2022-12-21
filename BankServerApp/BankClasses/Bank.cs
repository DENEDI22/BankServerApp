using System.Diagnostics;
using System.Text.Json;

namespace BankServerApp;

public class Bank
{
    public List<Account> m_registeredAccounts { get; set; }
    public List<CreditDepositData> m_Deposits { get; set; }
    public List<CreditDepositData> m_Credits { get; set; }
    public List<Transaction> m_processingTransactionsSet { get; set; }
    public List<Card> m_Cards { get; set; }

    private const string ACCOUNTS_SAVEPATH = @"\registeredAccounts.json";
    private const string CARDS_SAVEPATH = @"\allCards.json";
    private const string TRANSACTIONSSAVEFOLDER = @"\Transactions";
    private const string DEPOSITS_SAVEPATH = @"\deposits.json";
    private const string CREDITS_SAVEPATH = @"\credits.json";

    public void AssignCard(string _account, Currencies _currency)
    {
        int cardIndex = m_Cards.FindIndex(x => !x.isCurrentlyActive && x.currency == _currency);
        m_registeredAccounts.Find(x => x.AccountName == _account).AddNewCard(m_Cards[cardIndex]);
        m_Cards[cardIndex].ActivateCard();
    }
    
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
        // UpdateDatabase(ACCOUNTS_SAVEPATH, m_registeredAccounts);
    }

    public Card GetCardOfNumber(int _cardNumber)
    {
        return m_Cards.Find(x => x.cardNumber == _cardNumber);
    }

    public void GiveDepositPayments()
    {
        foreach (var deposit in m_Deposits)
        {
            if (deposit.lastPayoutDate.Date + TimeSpan.FromDays(28) == DateTime.Now.Date)
            {
                var userIndex = m_registeredAccounts.FindIndex(x => x.DepositIDs.Contains(deposit.DepositId));
                Transaction payoutTransaction = new Transaction(deposit.DepositStartSumm * deposit.interestRate / 12,
                    m_registeredAccounts[userIndex].AccountName, "Deposit payout",
                    m_registeredAccounts[userIndex].GetCardOfCurrency(deposit.currency),
                    CentralAccount.cardNumbers[(int)deposit.currency]);
                m_registeredAccounts[userIndex].ProcessTransaction(payoutTransaction);
                CentralAccount.ProcessTranasactionStatic(payoutTransaction);
                AddTransactionToDatabase(payoutTransaction, TransactionTypes.Income);
            }
        }
        UpdateUserDatabase();
    }

    public void OpenCredit(CreditDepositDraft _draft, string _accountName)
    {
        Account account = m_registeredAccounts.Find(x => x.AccountName == _accountName);
        CreditDepositData newCredit = new CreditDepositData(_draft);
        account.CreditIDs.Add(newCredit.DepositId);
        UpdateDatabase(ACCOUNTS_SAVEPATH, m_registeredAccounts);
        UpdateDatabase(CREDITS_SAVEPATH, m_Credits);
    }
    
    public void TakeCreditPayments()
    {
        foreach (var credit in m_Credits)
        {
            Account account = m_registeredAccounts.Find(x => x.CreditIDs.Contains(credit.DepositId));
            int cardID = account.GetCardOfCurrency(credit.currency);
            if (credit.lastPayoutDate.Date + TimeSpan.FromDays(28) == DateTime.Now.Date && credit.creditPayoutLeft > 0)
            {
                int creditPaymentAmount;
                if (m_Cards[cardID].cardBalance >= credit.creditPayoutLeft /
                    (credit.DepositEndDate - credit.DepositOpenDate).Days / 28)
                {
                    creditPaymentAmount = (credit.DepositEndDate - credit.DepositOpenDate).Days / 28;
                    credit.creditPayoutLeft -= creditPaymentAmount;
                }
                else
                {
                    creditPaymentAmount = (int)m_Cards[account.GetCardOfCurrency(credit.currency)].cardBalance;
                    credit.creditPayoutLeft += (credit.DepositEndDate - credit.DepositOpenDate).Days / 28;
                }

                Transaction payment = new Transaction(creditPaymentAmount, account.AccountName, "Credit payout", cardID,
                    CentralAccount.cardNumbers[(int)credit.currency]);

                account.ProcessTransaction(payment);
                AddTransactionToDatabase(payment, TransactionTypes.Outcome);
                if (credit.creditPayoutLeft <= 1)
                {
                    credit.creditPayoutLeft = 0;
                }
            }
        }
        UpdateDatabase(CREDITS_SAVEPATH, m_Credits);
    }

    public List<Card> GetCardsOfUser(string _accountName)
    {
        Account account = m_registeredAccounts.Find(x => x.AccountName == _accountName);
        return m_Cards.FindAll(x => account.Cards.Contains(x.cardNumber));
    }


    public void LoadDeposits()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + DEPOSITS_SAVEPATH;
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                m_Deposits = (JsonSerializer.Deserialize<CreditDepositData[]>(reader.ReadToEnd()) ??
                              throw new InvalidOperationException()).ToList();
            }
        }
        else
        {
            m_Deposits = new List<CreditDepositData>();
        }
    }

    public void UpdateDeposits()
    {
        // var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + DEPOSITS_SAVEPATH;
        // File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_Deposits));
        UpdateDatabase(DEPOSITS_SAVEPATH, m_Deposits);
    }

    public void UpdateDatabase<T>(string _savePath, T _objectToSave)
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + _savePath;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(_objectToSave));
    }

    public void UpdateCardsDatabase()
    {
        // var folder = Environment.SpecialFolder.LocalApplicationData;
        // var path = Environment.GetFolderPath(folder) + CARDS_SAVEPATH;
        // File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_Cards));
        UpdateDatabase(CARDS_SAVEPATH, m_Cards);
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

    public void PayDepositMoney(CreditDepositData _creditDepositData)
    {
        m_registeredAccounts.Find(x => x.DepositIDs.Contains(_creditDepositData.DepositId));
    }

    public void OpenDeposit(string _openerAccount, int _startPayment, Currencies _currency, int _monthCount,
        decimal _interestRate)
    {
        Account account = m_registeredAccounts.Find(x => x.AccountName == _openerAccount);
        Card card = GetCardsOfUser(account.AccountName).Find(x => x.currency == _currency);

        try
        {
            if (!card.isCurrentlyFreesed && card.cardBalance >= _startPayment)
            {
                account.ProcessTransaction(new Transaction(_startPayment, "Deposit", account.AccountName,
                    card.cardNumber, CentralAccount.cardNumbers[(int)card.currency]));
                CreditDepositData newCreditDepositData =
                    new CreditDepositData(_startPayment, _monthCount, _interestRate);
                m_Deposits.Add(newCreditDepositData);
                account.DepositIDs.Add(newCreditDepositData.DepositId);
                UpdateDatabase(DEPOSITS_SAVEPATH, m_Deposits);
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
                string json = reader.ReadToEnd();
                m_registeredAccounts = (JsonSerializer.Deserialize<Account[]>(json) ??
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
            UpdateCardsDatabase();
        }

        #endregion
    }
}