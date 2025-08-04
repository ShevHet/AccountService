namespace AccountService.Models;

/// <summary>
/// Представляет банковский счет
/// </summary>
public class Account
{
    /// <summary>
    /// Уникальный идентификатор счета
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор владельца счета
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Тип счета (Текущий, Депозитный, Кредитный)
    /// </summary>
    public EAccountType Type { get; set; }

    /// <summary>
    /// Валюта счета (ISO 4217 код)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Текущий баланс счета
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Процентная ставка (для депозитных и кредитных счетов)
    /// </summary>
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// Дата открытия счета
    /// </summary>
    public DateTime OpeningDate { get; set; }

    /// <summary>
    /// Дата закрытия счета (если счет закрыт)
    /// </summary>
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    /// Список транзакций по счету
    /// </summary>
    public ICollection<Transaction> Transactions { get; } = new List<Transaction>();
}

/// <summary>
/// Типы банковских счетов
/// </summary>
public enum EAccountType
{
    /// <summary>
    /// Текущий счет
    /// </summary>
    Checking,

    /// <summary>
    /// Депозитный счет
    /// </summary>
    Deposit,

    /// <summary>
    /// Кредитный счет
    /// </summary>
    Credit
}