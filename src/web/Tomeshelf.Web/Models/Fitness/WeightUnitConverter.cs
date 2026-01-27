using System;

namespace Tomeshelf.Web.Models.Fitness;

public static class WeightUnitConverter
{
    private const double PoundsPerKilogram = 2.2046226218;
    private const double KilogramsPerStone = 6.35029318;
    private const double PoundsPerStone = 14d;

    public static WeightUnit Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return WeightUnit.Stones;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "st" or "stone" or "stones" => WeightUnit.Stones,
            "lb" or "lbs" or "pound" or "pounds" => WeightUnit.Pounds,
            "kg" or "kgs" or "kilogram" or "kilograms" => WeightUnit.Kilograms,
            _ => WeightUnit.Stones
        };
    }

    public static string ToQueryValue(WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Stones => "st",
            WeightUnit.Pounds => "lb",
            _ => "kg"
        };
    }

    public static string GetUnitLabel(WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Stones => "st",
            WeightUnit.Pounds => "lb",
            _ => "kg"
        };
    }

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

    public static (int Stones, double Pounds) ToStonesAndPounds(double kilograms)
    {
        var totalPounds = kilograms * PoundsPerKilogram;
        var stones = (int)Math.Floor(totalPounds / PoundsPerStone);
        var pounds = totalPounds - (stones * PoundsPerStone);

        return (stones, pounds);
    }
}
