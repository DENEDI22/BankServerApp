using System.Text.Json;

namespace BankServerApp;

public class Bank
{
    public List<Account> m_registeredAccounts { get; set; }
    public List<Transaction> m_processingTransactionsSet { get; set; }

    private const string SAVEPATH = "/registeredAccounts.json";
    private const string TRANSACTIONSSAVEFOLDER = "/Transactions";

    public void AddTransactionToDatabase(Transaction _transaction, TransactionTypes _transactionType)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                            TRANSACTIONSSAVEFOLDER;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string transactionSaveFileName = _transactionType == TransactionTypes.Income
            ? _transaction.recieverAccountName
            : _transaction.senderAccountName;
        string transactionSavePath = $"{folderPath}/{transactionSaveFileName}.json";
        if (!File.Exists(transactionSavePath))
        {
            using FileStream stream = new FileStream(transactionSavePath, FileMode.Create);
            stream.Dispose();
            // m_processingTransactionsSet.Add(_transaction);
            // File.WriteAllTextAsync(transactionSavePath, JsonSerializer.Serialize(m_registeredAccounts));
            // return;
        }

        m_processingTransactionsSet = new List<Transaction>();
        try
        {
            using (StreamReader reader = new StreamReader(transactionSavePath))
            {
                m_processingTransactionsSet = (JsonSerializer.Deserialize<Transaction[]>(reader.ReadToEnd()) ??
                                               Array.Empty<Transaction>()).ToList();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        m_processingTransactionsSet.Add(_transaction);
        File.WriteAllTextAsync(transactionSavePath, JsonSerializer.Serialize(m_processingTransactionsSet));
    }

    public void UpdateUserDatabase()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + SAVEPATH;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_registeredAccounts));
    }

    public Transaction[]? GetUsersTransactions(string _accountName)
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                            TRANSACTIONSSAVEFOLDER;
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string transactionSavePath = $"{folderPath}/{_accountName}.json";
        if (File.Exists(transactionSavePath))
        {
            using (StreamReader reader = new StreamReader(transactionSavePath))
            {
                return JsonSerializer.Deserialize<Transaction[]>(reader.ReadToEnd());
            }
        }

        return null;
    }

    public Bank()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + SAVEPATH;

        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                m_registeredAccounts = (JsonSerializer.Deserialize<Account[]>(reader.ReadToEnd()) ??
                                        throw new InvalidOperationException()).ToList();
            }
        }
        else
        {
            m_registeredAccounts = new List<Account>();
            using FileStream stream = new FileStream(path, FileMode.Create);
            stream.Dispose();
        }
    }
}