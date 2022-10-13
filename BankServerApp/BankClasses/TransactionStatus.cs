namespace BankServerApp;

public enum TransactionStatus
{
    Succeed,
    NotEnoughMoney,
    RecieverNumberNotFound,
    DeviceCodeExpired,
    FailedForOtherReason,
    InProgress
}