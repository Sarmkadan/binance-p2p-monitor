#nullable enable
namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<string> errors) : base(string.Join("; ", errors))
    {
        Errors = errors;
    }

    public ValidationException(string message, List<string> errors) : base(message)
    {
        Errors = errors;
    }

    public void AddError(string error)
    {
        if (!Errors.Contains(error))
            Errors.Add(error);
    }

    public bool HasErrors => Errors.Any();
}
