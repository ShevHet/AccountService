namespace AccountService.Models
{
    public record PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int Size,
        long TotalCount
    );
}
