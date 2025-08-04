/// <summary>Ответ с пагинацией</summary>
/// <typeparam name="T">Тип элементов в коллекции</typeparam>
public record PagedResponse<T>(
    /// <summary>Элементы на текущей странице</summary>
    IEnumerable<T> Items,
    
    /// <summary>Текущая страница</summary>
    int Page,
    
    /// <summary>Размер страницы (количество элементов)</summary>
    int Size,
    
    /// <summary>Общее количество элементов</summary>
    long TotalCount
);