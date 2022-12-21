using System.Text.Json.Serialization;

namespace BankServerApp;

public class Account
{
    [JsonInclude] [JsonPropertyName("AccountName")] public string AccountName { get; set; }
    [JsonInclude] [JsonPropertyName("Cards")] public List<int> Cards { get; set; }
    [JsonInclude] [JsonPropertyName("SecurityKey")] public string SecurityKey { get;  set; }
    [JsonInclude] [JsonPropertyName("LoggedInDeviceIDs")] public List<ulong> LoggedInDeviceIDs { get;  set; }
    [JsonInclude] [JsonPropertyName("DepositIDs")] public List<int> DepositIDs { get; set; }

    [JsonInclude] [JsonPropertyName("CreditIDs")] public List<int> CreditIDs { get; set; }

    private List<Transaction>? allTransactions;

    public int GetCardOfCurrency(Currencies _currencies) => Cards.Find(x => x.ToString()[1] == (int)_currencies);
    
    [JsonConstructor]
    public Account(string AccountName, string SecurityKey, List<ulong> LoggedInDeviceIDs, int[] Cards, int[] DepositIDs,
        int[] CreditIDs)
    {
        this.AccountName = AccountName;
        this.SecurityKey = SecurityKey;
        this.LoggedInDeviceIDs = LoggedInDeviceIDs;
        this.CreditIDs = CreditIDs.ToList();
        this.DepositIDs = DepositIDs.ToList();
        this.Cards = Cards.ToList();
    }
    
    public void AddNewCard(Card _newCard)
    {
        Cards.Add(_newCard.cardNumber);
    }

    public Account(string _accountName, string _securityKey)
    {
        AccountName = _accountName;
        SecurityKey = _securityKey;
        DepositIDs = new List<int>();
        Cards = new List<int>();
        CreditIDs = new List<int>();
        LoggedInDeviceIDs = new List<ulong>();
        allTransactions = new List<Transaction>();
    }

    public void RollbackTransaction(Transaction _transaction)
    {
        if (allTransactions.Remove(_transaction))
        {
            Bank bank = new Bank();
            bool ifTransactionWasIncomming = _transaction.recieverAccountName == AccountName;
            bank.m_Cards
                .Find(x => ifTransactionWasIncomming
                    ? x.cardNumber == _transaction.recieverCard
                    : x.cardNumber == _transaction.senderCard).AddMoney(
                    (ifTransactionWasIncomming ? _transaction.transactionAmount : -_transaction.transactionAmount),
                    (ifTransactionWasIncomming
                        ? bank.m_Cards.Find(x => x.cardNumber == _transaction.recieverCard).currency
                        : bank.m_Cards.Find(x => x.cardNumber == _transaction.senderCard)
                            .currency)); //I know how to do it right. Just wanted to leave some mess right here.
        }
    }

    public bool TryLogIn(string _securityKey, ulong _deviceID)
    {
        if (LoggedInDeviceIDs.Contains(_deviceID)) return true;
        if (_securityKey == SecurityKey)
        {
            LoggedInDeviceIDs.Add(_deviceID);
            return true;
        }

        return false;
    }

    // public Transaction[] Transactions(ulong _deviceID)
    // {
    //     if (loggedInDeviceIDs.Contains(_deviceID))
    //     {
    //         return allTransactions.ToArray();
    //     }
    //
    //     return null;
    // }

    /// <summary>
    /// returns 0 if transaction is successful. 
    /// returns 1 if there is not enough money. 
    /// returns 2 if sender or receiver account name is invalid. 
    /// </summary>
    /// <param name="_newTransaction">Transaction to process</param>
    /// <returns></returns>
    public int ProcessTransaction(Transaction _newTransaction)
    {
        Bank bank = new();
        if (_newTransaction.senderAccountName == AccountName && Cards.Contains(_newTransaction.senderCard))
        {
            Card senderCard = bank.GetCardOfNumber(_newTransaction.senderCard);
            if (senderCard.cardBalance - _newTransaction.transactionAmount > 0 && !senderCard.isCurrentlyFreesed)
            {
                senderCard.AddMoney(-_newTransaction.transactionAmount, senderCard.currency);
                bank.UpdateCardsDatabase();
            }
            else return 1;
        }
        else if (_newTransaction.recieverAccountName == AccountName && Cards.Contains(_newTransaction.recieverCard))
        {
            Card receiverCard = bank.GetCardOfNumber(_newTransaction.recieverCard);
            receiverCard.AddMoney(_newTransaction.transactionAmount,
                bank.GetCardOfNumber(_newTransaction.senderCard).currency);
            bank.UpdateCardsDatabase();
        }
        else return 2;
        
        return 0;
    }

    /// <summary>
    /// returns 0 if transaction is successful. 
    /// returns 1 if there is not enough money. 
    /// returns 2 if sender or receiver account name is invalid. 
    /// returns 3 if sender CVV code is invalid.
    /// </summary>
    /// <param name="_newTransaction">Transaction to process</param>
    /// <returns></returns>
    public int ProcessTransaction(Transaction _newTransaction, int CVVCode)
    {
        Bank bank = new();
        if (bank.m_Cards.Find(x => x.cardNumber == _newTransaction.senderCard).additionalSecurityCode == CVVCode)
        {
            return ProcessTransaction(_newTransaction);
        }
        return 3;
    }
}