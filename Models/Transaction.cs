namespace AccountService.Models;

/// <summary>
/// Представляет финансовую транзакцию
/// </summary>
public class Transaction
{
    /// <summary>
    /// Уникальный идентификатор транзакции
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор счета
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Идентификатор счета контрагента (для переводов)
    /// </summary>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>
    /// Сумма транзакции
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Валюта транзакции (ISO 4217 код)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Тип транзакции (Кредит/Дебет)
    /// </summary>
    public ETransactionType Type { get; set; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время совершения транзакции
    /// </summary>
    public DateTime DateTime { get; set; }
}

/// <summary>
/// Типы финансовых транзакций
/// </summary>
public enum ETransactionType
{
    /// <summary>
    /// Кредит (зачисление средств)
    /// </summary>
    Credit,

    /// <summary>
    /// Дебет (списание средств)
    /// </summary>
    Debit
}