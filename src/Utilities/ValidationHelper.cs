// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Utility class for data validation
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates if email address is valid
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if cryptocurrency ticker is valid
    /// </summary>
    public static bool IsValidTicker(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return false;

        return Regex.IsMatch(ticker, "^[A-Z0-9]{1,20}$");
    }

    /// <summary>
    /// Validates if fiat currency code is valid
    /// </summary>
    public static bool IsValidFiatCode(string fiatCode)
    {
        if (string.IsNullOrWhiteSpace(fiatCode))
            return false;

        return Regex.IsMatch(fiatCode, "^[A-Z]{3}$");
    }

    /// <summary>
    /// Validates if price is within acceptable range
    /// </summary>
    public static bool IsValidPrice(decimal price, decimal minPrice = 0.00000001m, decimal maxPrice = 1000000000)
    {
        return price >= minPrice && price <= maxPrice;
    }

    /// <summary>
    /// Validates if threshold percentage is valid
    /// </summary>
    public static bool IsValidThreshold(decimal threshold, decimal min = 0, decimal max = 100)
    {
        return threshold >= min && threshold <= max;
    }

    /// <summary>
    /// Validates if Telegram chat ID is valid
    /// </summary>
    public static bool IsValidTelegramChatId(long chatId)
    {
        return chatId > 0;
    }

    /// <summary>
    /// Validates if date range is valid
    /// </summary>
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
    {
        return startDate < endDate && startDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Validates if a collection is not null and contains items
    /// </summary>
    public static bool IsValidCollection<T>(IEnumerable<T>? collection)
    {
        return collection != null && collection.Any();
    }

    /// <summary>
    /// Validates decimal precision (number of decimal places)
    /// </summary>
    public static bool IsValidPrecision(decimal value, int maxDecimalPlaces)
    {
        var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
        return decimalPlaces <= maxDecimalPlaces;
    }

    /// <summary>
    /// Validates if a string matches a specific pattern
    /// </summary>
    public static bool MatchesPattern(string value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(pattern))
            return false;

        return Regex.IsMatch(value, pattern);
    }
}
