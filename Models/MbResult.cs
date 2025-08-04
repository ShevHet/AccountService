namespace AccountService.Models;

/// <summary>
/// ��������� ��������, ���������� ���� ��������, ���� ������
/// </summary>
/// <typeparam name="T">��� ������������� ��������</typeparam>
public record MbResult<T>(T? Value, MbError? Error)
{
    /// <summary>
    /// ������� ��������� ���������� ��������
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// ������� �������� ���������
    /// </summary>
    /// <param name="value">��������� ��������</param>
    public static MbResult<T> Success(T value) => new(value, null);

    /// <summary>
    /// ������� ��������� � �������
    /// </summary>
    /// <param name="error">���������� �� ������</param>
    public static MbResult<T> Failure(MbError error) => new(default, error);
}

/// <summary>
/// ���������� �� ������
/// </summary>
public record MbError(
    /// <summary>
    /// ��������� �� ������
    /// </summary>
    string Message,

    /// <summary>
    /// HTTP ������-��� ������
    /// </summary>
    int StatusCode,

    /// <summary>
    /// ������ ��������� (���� - ��� ����, �������� - ������ ������)
    /// </summary>
    Dictionary<string, string[]>? ValidationErrors = null
);