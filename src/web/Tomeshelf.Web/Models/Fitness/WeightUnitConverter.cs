using System;

namespace Tomeshelf.Web.Models.Fitness;

public static class WeightUnitConverter
{
    private const double PoundsPerKilogram = 2.2046226218;
    private const double KilogramsPerStone = 6.35029318;
    private const double PoundsPerStone = 14d;

    /// <summary>
    ///     Converts.
    /// </summary>
    /// <param name="kilograms">The kilograms.</param>
    /// <param name="unit">The unit.</param>
    /// <returns>The result of the operation.</returns>
    public static double? Convert(double? kilograms, WeightUnit unit)
    {
        if (!kilograms.HasValue)
        {
            return null;
        }

        return unit switch
        {
            WeightUnit.Pounds => kilograms.Value * PoundsPerKilogram,
            WeightUnit.Stones => kilograms.Value / KilogramsPerStone,
            _ => kilograms.Value
        };
    }

    /// <summary>
    ///     Gets the unit label.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <returns>The resulting string.</returns>
    public static string GetUnitLabel(WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Stones => "st",
            WeightUnit.Pounds => "lb",
            _ => "kg"
        };
    }

    /// <summary>
    ///     Parses.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the operation.</returns>
    public static WeightUnit Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return WeightUnit.Stones;
        }

        return value.Trim()
                    .ToLowerInvariant() switch
        {
            "st" or "stone" or "stones" => WeightUnit.Stones,
            "lb" or "lbs" or "pound" or "pounds" => WeightUnit.Pounds,
            "kg" or "kgs" or "kilogram" or "kilograms" => WeightUnit.Kilograms,
            _ => WeightUnit.Stones
        };
    }

    /// <summary>
    ///     Tos the query value.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <returns>The resulting string.</returns>
    public static string ToQueryValue(WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Stones => "st",
            WeightUnit.Pounds => "lb",
            _ => "kg"
        };
    }

    /// <summary>
    ///     Tos the stones and pounds.
    /// </summary>
    /// <param name="kilograms">The kilograms.</param>
    /// <returns>The result of the operation.</returns>
    public static (int Stones, double Pounds) ToStonesAndPounds(double kilograms)
    {
        var totalPounds = kilograms * PoundsPerKilogram;
        var stones = (int)Math.Floor(totalPounds / PoundsPerStone);
        var pounds = totalPounds - (stones * PoundsPerStone);

        return (stones, pounds);
    }
}