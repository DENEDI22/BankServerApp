﻿namespace BankServerApp;

public enum TransactionStatus
{
    Succeed = 0,
    NotEnoughMoney = 1,
    ReceiverNumberNotFound = 2,
    DeviceCodeExpired = 4,
    FailedForOtherReason = 3,
    InProgress = 5
}