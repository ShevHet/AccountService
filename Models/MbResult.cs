namespace AccountService.Models;

/// <summary>
/// Результат операции, содержащий либо значение, либо ошибку
/// </summary>
/// <typeparam name="T">Тип возвращаемого значения</typeparam>
public record MbResult<T>(T? Value, MbError? Error)
{
    /// <summary>
    /// Признак успешного выполнения операции
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Создает успешный результат
    /// </summary>
    /// <param name="value">Результат операции</param>
    public static MbResult<T> Success(T value) => new(value, null);

    /// <summary>
    /// Создает результат с ошибкой
    /// </summary>
    /// <param name="error">Информация об ошибке</param>
    public static MbResult<T> Failure(MbError error) => new(default, error);
}

/// <summary>
/// Информация об ошибке
/// </summary>
public record MbError(
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    string Message,

    /// <summary>
    /// HTTP статус-код ошибки
    /// </summary>
    int StatusCode,

    /// <summary>
    /// Ошибки валидации (ключ - имя поля, значение - список ошибок)
    /// </summary>
    Dictionary<string, string[]>? ValidationErrors = null
);