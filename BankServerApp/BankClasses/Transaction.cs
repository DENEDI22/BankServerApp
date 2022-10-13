using System.Text.Json.Serialization;

namespace BankServerApp;
[Serializable]
public struct Transaction
{
    [JsonInclude] public long transactionID;
    [JsonInclude] public int transactionAmount;
    [JsonInclude] public TransactionStatus transactionStatus;
    [JsonInclude] public string recieverAccountName, senderAccountName;
    [JsonInclude] public DateTime dateOfTransaction;

    [JsonConstructor]
    public Transaction(int TransactionAmount, TransactionStatus TransactionStatus, long TransactionID,
        string? RecieverAccountName, string? SenderAccountName, DateTime DateOfTransaction)
    {
        transactionID = TransactionID;
        transactionAmount = TransactionAmount;
        transactionStatus = TransactionStatus;
        recieverAccountName = RecieverAccountName;
        senderAccountName = SenderAccountName;
        dateOfTransaction = DateOfTransaction;
    }
    
    public Transaction(int _transactionAmount, string _recieverName, string _senderName)
    {
        Random random = new Random();
        transactionID = random.NextInt64(0, Int64.MaxValue);
        transactionStatus = TransactionStatus.InProgress;
        recieverAccountName = _recieverName;
        senderAccountName = _senderName;
        transactionAmount = _transactionAmount;
        dateOfTransaction = DateTime.Now;
    }
}