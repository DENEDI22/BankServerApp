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
        [HttpGet("newtransaction/{_transactionAmount}&{_receiverName}&{_senderName}")]
        public int AddTransaction(int _transactionAmount, string _receiverName, string _senderName)
        {
            var newTransaction = new Transaction(_transactionAmount, _receiverName, _senderName);
            switch (bank.m_registeredAccounts.Find(x => x.accountName == _senderName).ProcessTransaction(newTransaction))
            {
                case 0:
                    var senderAccount = bank.m_registeredAccounts.Find(x => x.accountName == _receiverName);
                    if (senderAccount.ProcessTransaction(newTransaction) == 0)
                    {
                        newTransaction.transactionStatus = TransactionStatus.Succeed;
                        bank.AddTransactionToDatabase(newTransaction);
                    }
                    else
                    {
                        newTransaction.transactionStatus = TransactionStatus.RecieverNumberNotFound;
                        ;
                    }
                    break;
                case 1:
                    break;
                case 2: 
                    break;
            }
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