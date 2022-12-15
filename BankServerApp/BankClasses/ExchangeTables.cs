using System.Xml;

namespace BankServerApp;

public class ExchangeTables
{
    private Dictionary<Currencies, decimal> comparedToUAHExchangeRate = new Dictionary<Currencies, decimal>();

    public decimal GetFromExchangeRateTable(Currencies _innitialCurrency, Currencies _returnCurrency, decimal amount)
    {
        var tmp = amount / comparedToUAHExchangeRate[_innitialCurrency];
        return tmp * comparedToUAHExchangeRate[_returnCurrency];
    }

    public decimal GetFromExchangeRateTable(int _innitialCurrency, int _returnCurrency, decimal amount)
    {
        return GetFromExchangeRateTable((Currencies)_innitialCurrency, (Currencies)_returnCurrency, amount);
    }

    public ExchangeTables()
    {
        LoadRates();
    }

    private static Currencies? ParseCurrency(string _string)
    {
        switch (_string)
        {
            case "usd":
                return Currencies.USD;
            case "eur":
                return Currencies.EUR;
            case "uah":
                return Currencies.UAH;
            case "gbp":
                return Currencies.GBP;
            case "pln":
                return Currencies.PLN;
            default:
                return null;
        }
    }

    public void LoadRates()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange");
        foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
        {
            var parsedCurrency = ParseCurrency(node.Attributes["cc"].Value);
            if (parsedCurrency != null)
            {
                comparedToUAHExchangeRate.Add((Currencies)parsedCurrency, decimal.Parse(node.Attributes["rate"].Value));
            }
        }
    }
}