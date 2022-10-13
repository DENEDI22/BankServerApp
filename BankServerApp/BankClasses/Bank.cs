using System.Text.Json;

namespace BankServerApp;

public class Bank
{
    public List<Account> m_registeredAccounts { get; set; }
    public List<Transaction> m_processingTransactionsSet { get; set; }
    
    private const string SAVEPATH = "/registeredAccounts.json";
    private const string TRANSACTIONSSAVEFOLDER = "/Transactions/";

    public void AddTransactionToDatabase(Transaction _transaction)
    {
        
    }
    
    public void UpdateUserDatabase()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + SAVEPATH;
        File.WriteAllTextAsync(path, JsonSerializer.Serialize(m_registeredAccounts));
    }

    public Bank()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder) + SAVEPATH;

        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                m_registeredAccounts = (JsonSerializer.Deserialize<Account[]>(reader.ReadToEnd()) ?? throw new InvalidOperationException()).ToList();
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