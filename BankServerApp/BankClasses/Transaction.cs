using System.Text.Json.Serialization;

namespace BankServerApp;
[Serializable]
public struct Transaction
{
    [JsonInclude] public long transactionID { get; }
    [JsonInclude] public decimal transactionAmount{ get; set; }
    [JsonInclude] public TransactionStatus transactionStatus{ get; set; }
    [JsonInclude] public string recieverAccountName{ get; set; }
    [JsonInclude] public string senderAccountName{ get; set; }
    [JsonInclude] public int recieverCard { get; set; }
    [JsonInclude] public int senderCard{ get; set; }
    [JsonInclude] public DateTime dateOfTransaction{ get; set; }

    [JsonConstructor]
    public Transaction(int TransactionAmount, TransactionStatus TransactionStatus, long TransactionID,
        string? RecieverAccountName, string? SenderAccountName, DateTime DateOfTransaction, int ReceiverCard, int SenderCard)
    {
        transactionID = TransactionID;
        transactionAmount = TransactionAmount;
        transactionStatus = TransactionStatus;
        recieverAccountName = RecieverAccountName;
        senderAccountName = SenderAccountName;
        dateOfTransaction = DateOfTransaction;
        recieverCard = ReceiverCard;
        senderCard = SenderCard;
    }
    
    public Transaction(decimal _transactionAmount, string _recieverName, string _senderName, int _recieverCard, int _senderCard)
    {
        Random random = new Random();
        transactionID = random.NextInt64(0, Int64.MaxValue);
        transactionStatus = TransactionStatus.InProgress;
        recieverAccountName = _recieverName;
        senderAccountName = _senderName;
        transactionAmount = _transactionAmount;
        dateOfTransaction = DateTime.Now;
        recieverCard = _recieverCard;
        senderCard = _senderCard;
    }
}