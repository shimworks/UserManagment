namespace UserManagement.Application.Common.DTOs;
  public sealed record PagedResponse<T>(
      IEnumerable<T> Items,
      int Page,
      int PageSize,
      int TotalCount
  );