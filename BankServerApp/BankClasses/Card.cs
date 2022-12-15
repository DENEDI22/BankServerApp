using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace BankServerApp;

[Serializable]
public struct Card
{
    public int cardNumber { get; private set; }
    public int additionalSecurityCode { get; private set; }
    public Currencies currency { get; private set; }
    public DateTime expireDate { get; private set; }
    public bool isCurrentlyActive { get; private set; }
    public bool isCurrentlyFreesed { get; private set; } = false;
    public decimal cardBalance { get; private set; }

    /// <summary>
    /// Use this to create completely new card. Creates inactive card with specific number and generated CVV code,
    /// </summary>
    /// <param name="_cardNumber"></param>
    /// <param name="_currency"></param>
    public Card(int _cardNumber, Currencies _currency)
    {
        cardNumber = _cardNumber;
        currency = _currency;
        expireDate = DateTime.Now + new TimeSpan(365 * 4, 0, 0, 0);
        isCurrentlyActive = false;
        Random random = new Random();
        additionalSecurityCode = random.Next(1000);
        cardBalance = 0;
    }

    public void ActivateCard()
    {
        Random random = new Random();
        additionalSecurityCode = random.Next(1000);
        isCurrentlyActive = true;
    }
    
    /// <summary>
    /// Use this only for JSON. Creates a copy of card serialized in JSON.
    /// </summary>
    /// <param name="CardNumber"></param>
    /// <param name="AdditionalSecurityCode"></param>
    /// <param name="Currency"></param>
    /// <param name="ExpireDate"></param>
    /// <param name="IsCurrentlyActive"></param>
    [JsonConstructor]
    public Card(int CardNumber, int AdditionalSecurityCode, int Currency, string ExpireDate, bool IsCurrentlyActive,
        decimal CardBalance, bool IsCurrentlyFreesed)
    {
        cardNumber = CardNumber;
        additionalSecurityCode = AdditionalSecurityCode;
        currency = (Currencies)Currency;
        expireDate = DateTime.Parse(ExpireDate);
        isCurrentlyActive = IsCurrentlyActive;
        cardBalance = CardBalance;
        isCurrentlyFreesed = IsCurrentlyFreesed;
    }

    public void AddMoney(decimal _amount, Currencies _currency)
    {
        if (_currency == currency)
        {
            cardBalance += _amount;
        }
        else
        {
            ExchangeTables exchangeTables = new ExchangeTables();
            cardBalance += exchangeTables.GetFromExchangeRateTable(_currency, currency, _amount);
        }
    }
}