namespace Finora.Messages.Users;

public class Paginated<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; } = 0;
}