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

        [HttpPost(
            "deposits/createNew/{_openerAccount}&{_startPayment}&{_currency}&{_monthCount}&{_interestRate}")]
        public void AddNewDeposit(string _openerAccount, int _startPayment, int _currency, int _monthCount,
            decimal _interestRate) =>
            bank.OpenDeposit(_openerAccount, _startPayment, (Currencies)_currency, _monthCount, _interestRate);

        [HttpGet("deposits/getDepositsof{_accountName}")]
        public CreditDepositData[] GetDepositsOfUser(string _accountName)
        {
            return bank.m_Deposits.FindAll(x =>
                    bank.m_registeredAccounts.Find(x => x.AccountName == _accountName).DepositIDs.Contains(x.DepositId))
                .ToArray();
        }

        [HttpGet("credits/getCreditsOf{_accountName}")]
        public CreditDepositData[] GetCreditsOfUser(string _accountName)
        {
            return bank.m_Credits.FindAll(x =>
                    bank.m_registeredAccounts.Find(x => x.AccountName == _accountName).DepositIDs.Contains(x.DepositId))
                .ToArray();
        }

        [HttpPost("credits/createNew/{_openerAccount}&{_startPayment}&{_currency}&{_monthCount}&{_interestRate}")]
        public void TakeCredit(string _openerAccount, int _startPayment, int _currency, int _monthCount,
            decimal _interestRate)
        {
            bank.OpenCredit(new CreditDepositDraft
            {
                monthCount = _monthCount,
                startsumm = _startPayment,
                interestRate = _interestRate,
                currency = (Currencies)_currency
            }, _openerAccount);
        }

        [HttpGet("transactions/{_accountName}")]
        public Transaction[]? GetTransactions(string _accountName)
        {
            return bank.GetUsersTransactions(_accountName);
        }

        [HttpGet(
            "newtransaction/{_transactionAmount}&{_receiverName}&{_senderName}&{_senderCardNumber}&{_senderCardCVV}&{_receiverCardNumber}")]
        public int AddTransaction(int _transactionAmount, string _receiverName, string _senderName,
            int _senderCardNumber, int _senderCardCVV, int _receiverCardNumber)
        {
            var newTransaction = new Transaction(_transactionAmount, _receiverName, _senderName, _receiverCardNumber,
                _senderCardNumber);
            decimal additionalPayment = 0;
            if (newTransaction.recieverCard.ToString()[1] == newTransaction.senderCard.ToString()[1])
            {
                ExchangeTables tables = new ExchangeTables();
                additionalPayment =
                    tables.GetFromExchangeRateTable(Int32.Parse(newTransaction.recieverCard.ToString()[1].ToString()),
                        Int32.Parse(newTransaction.senderCard.ToString()[1].ToString()),
                        newTransaction.transactionAmount) - newTransaction.transactionAmount;
            }

            var senderAccount = bank.m_registeredAccounts.Find(x => x.AccountName == _senderName);
            newTransaction.transactionAmount += additionalPayment;
            switch (senderAccount.ProcessTransaction(newTransaction))
            {
                case 0:
                    newTransaction.transactionAmount -= additionalPayment;
                    int transactionResult = bank.m_registeredAccounts.Find(x => x.AccountName == _receiverName)
                        .ProcessTransaction(newTransaction);
                    if (transactionResult == 0)
                    {
                        newTransaction.transactionStatus = TransactionStatus.Succeed;
                        bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Outcome);
                        bank.AddTransactionToDatabase(newTransaction, TransactionTypes.Income);
                        bank.UpdateUserDatabase();
                    }
                    else
                    {
                        newTransaction.transactionStatus = TransactionStatus.ReceiverNumberNotFound;
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
                    newTransaction.transactionStatus = TransactionStatus.ReceiverNumberNotFound;
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
                bank.AssignCard(_accountName, Currencies.UAH);
                bank.UpdateUserDatabase();
                return account;
            }

            return null;
        }

        [HttpGet("login/{_accountName}&{_securityCode}&{_deviceID}")]
        public Account LogIn(string _accountName, string _securityCode, ulong _deviceID)
        {
            var account = bank.m_registeredAccounts.Find(_account => _account.AccountName == _accountName);
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
        [HttpPost("cards/assign/{_accountName}&{_currency}")]
        public void AssignNewCard(string _accountName, int _currency)
        {
            bank.AssignCard(_accountName, (Currencies)_currency);
        }
    }
}