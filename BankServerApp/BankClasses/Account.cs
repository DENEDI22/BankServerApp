using System.Text.Json.Serialization;

namespace BankServerApp;

[Serializable]
public class Account
{
    [JsonInclude] public string accountName { get; }
    [JsonInclude] public int[] cards { get; private set; }
    [JsonInclude] public string securityKey { get; private set; }
    [JsonInclude] public List<ulong> loggedInDeviceIDs { get; private set; }

    private List<Transaction> allTransactions = new List<Transaction>();

    [JsonConstructor]
    public Account(string AccountName, string SecurityKey, List<ulong> LoggedInDeviceIDs, int[] Cards)
    {
        accountName = AccountName;
        securityKey = SecurityKey;
        loggedInDeviceIDs = LoggedInDeviceIDs;
        cards = Cards;
    }

    public Account(string _accountName, string _securityKey)
    {
        accountName = _accountName;
        securityKey = _securityKey;
        loggedInDeviceIDs = new List<ulong>();
    }

    public void RollbackTransaction(Transaction _transaction)
    {
        if (allTransactions.Remove(_transaction))
        {
            Bank bank = new Bank();
            bool ifTransactionWasIncomming = _transaction.recieverAccountName == accountName;
            bank.m_Cards.Find(x => ifTransactionWasIncomming ? x.cardNumber == _transaction.recieverCard : x.cardNumber == _transaction.senderCard).AddMoney((ifTransactionWasIncomming? _transaction.transactionAmount : -_transaction.transactionAmount), (ifTransactionWasIncomming ? bank.m_Cards.Find(x => x.cardNumber == _transaction.recieverCard).currency : bank.m_Cards.Find(x => x.cardNumber == _transaction.senderCard).currency)); //I know how to do it right. Just wanted to leave some mess right here.
        }
    }

    public bool TryLogIn(string _securityKey, ulong _deviceID)
    {
        if (loggedInDeviceIDs.Contains(_deviceID)) return true;
        if (_securityKey == securityKey)
        {
            loggedInDeviceIDs.Add(_deviceID);
            return true;
        }

        return false;
    }

    public Transaction[] Transactions(ulong _deviceID)
    {
        if (loggedInDeviceIDs.Contains(_deviceID))
        {
            return allTransactions.ToArray();
        }

        return null;
    }

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
        if (_newTransaction.senderAccountName == accountName && cards.Contains(_newTransaction.senderCard))
        {
            Card senderCard = bank.GetCardOfNumber(_newTransaction.senderCard);
            if (senderCard.cardBalance - _newTransaction.transactionAmount > 0 && !senderCard.isCurrentlyFreesed)
                senderCard.AddMoney(-_newTransaction.transactionAmount, senderCard.currency);
            else return 1;
        }
        else if (_newTransaction.recieverAccountName == accountName && cards.Contains(_newTransaction.recieverCard))
        {
            Card receiverCard = bank.GetCardOfNumber(_newTransaction.recieverCard);
            receiverCard.AddMoney(_newTransaction.transactionAmount,
                bank.GetCardOfNumber(_newTransaction.senderCard).currency);
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