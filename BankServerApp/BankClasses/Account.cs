using System.Text.Json.Serialization;

namespace BankServerApp;

[Serializable]
public class Account
{
    [JsonInclude] public string accountName { get; }
    [JsonInclude] public int balance { get; private set; }
    [JsonInclude] public string securityKey { get; private set; }
    [JsonInclude] public List<ulong> loggedInDeviceIDs { get; private set; }

    private List<Transaction> allTransactions = new List<Transaction>();

    [JsonConstructor]
    public Account(string AccountName, int Balance, string SecurityKey, List<ulong> LoggedInDeviceIDs)
    {
        accountName = AccountName;
        balance = Balance;
        securityKey = SecurityKey;
        loggedInDeviceIDs = LoggedInDeviceIDs;
    }

    public Account(string _accountName, string _securityKey)
    {
        accountName = _accountName;
        securityKey = _securityKey;
        loggedInDeviceIDs = new List<ulong>();
    }

    public void RollbackTransaction(Transaction _transaction)
    {
        if (allTransactions.Contains())
        {
            
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
        if (_newTransaction.senderAccountName == accountName)
        {
            if (balance - _newTransaction.transactionAmount > 0) balance -= _newTransaction.transactionAmount;
            else return 1;
        }
        else if (_newTransaction.recieverAccountName == accountName)
        {
            balance += _newTransaction.transactionAmount;
        }
        else return 2;
        
        return 0;
    }
}