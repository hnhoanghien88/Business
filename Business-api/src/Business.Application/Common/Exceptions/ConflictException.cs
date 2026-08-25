namespace Business.Application.Common.Exceptions;

public sealed class ConflictException : Exception
{
    public ConflictException() { }
    public ConflictException(string message, string? field = null) : base(message) => Field = field;
    public string? Field { get; }
}
