using Microsoft.AspNetCore.Mvc;

namespace BankServerApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankServerController : ControllerBase
    {
        private Bank bank;

        private readonly ILogger<BankServerController> _logger;

        public BankServerController(ILogger<BankServerController> logger)
        {
            _logger = logger;
            bank = new Bank();
        }

        [HttpGet("transactions/{_accountName}")]
        public Transaction[]? GetTransactions(string _accountName)
        {
            return bank.GetUsersTransactions(_accountName);
        }

        [HttpGet("newtransaction/{_transactionAmount}&{_receiverName}&{_senderName}")]
        public int AddTransaction(int _transactionAmount, string _receiverName, string _senderName)
        {
            var newTransaction = new Transaction(_transactionAmount, _receiverName, _senderName);
            var senderAccount = bank.m_registeredAccounts.Find(x => x.accountName == _senderName);
            switch (senderAccount.ProcessTransaction(newTransaction))
            {
                case 0:
                    int transactionResult = bank.m_registeredAccounts.Find(x => x.accountName == _receiverName).ProcessTransaction(newTransaction);
                    if (transactionResult == 0)
                    {
                        newTransaction.transactionStatus = TransactionStatus.Succeed;
                        bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Outcome);
                        bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Income);
                        bank.UpdateUserDatabase();
                    }
                    else
                    {
                        newTransaction.transactionStatus = TransactionStatus.RecieverNumberNotFound;
                        senderAccount.RollbackTransaction(newTransaction);
                        bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Outcome);
                    }
                    return transactionResult;
                case 1:
                    newTransaction.transactionStatus = TransactionStatus.NotEnoughMoney;
                    senderAccount.RollbackTransaction(newTransaction);
                    bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Outcome);
                    return 1;
                case 2:
                    newTransaction.transactionStatus = TransactionStatus.RecieverNumberNotFound;
                    senderAccount.RollbackTransaction(newTransaction);
                    bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Outcome);
                    break;
            }

            return 2;
        }

        [HttpGet]
        public IEnumerable<Account> Get()
        {
            return bank.m_registeredAccounts.ToArray();
        }

        [HttpGet("register/{_accountName}&{_securityCode}&{_deviceID}")]
        public Account Register(string _accountName, string _securityCode, ulong _deviceID)
        {
            Account account = new Account(_accountName, _securityCode);
            bank.m_registeredAccounts.Add(account);
            if (bank.m_registeredAccounts[^1].TryLogIn(_securityCode, _deviceID))
            {
                bank.UpdateUserDatabase();
                return account;
            }

            return null;
        }

        [HttpGet("login/{_accountName}&{_securityCode}&{_deviceID}")]
        public Account LogIn(string _accountName, string _securityCode, ulong _deviceID)
        {
            var account = bank.m_registeredAccounts.Find(_account => _account.accountName == _accountName);
            if (account != null)
            {
                if (account.TryLogIn(_securityCode, _deviceID))
                {
                    bank.UpdateUserDatabase();
                    return account;
                }
            }

            return null;
        }
    }
}