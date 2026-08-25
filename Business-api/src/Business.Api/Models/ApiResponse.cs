namespace Business.Api.Models;

public sealed record ApiResponse<T>(bool Success, T? Data, string Message);
