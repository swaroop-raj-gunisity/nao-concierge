namespace NaoConcierge.Domain.ValueObjects;

public record ConfidenceScore
{
    public double Value { get; init; }

    public bool RequiresClarification => Value < 0.70;

    private ConfidenceScore(double value) => Value = value;

    public static ConfidenceScore Create(double value)
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Confidence score must be between 0.0 and 1.0.");

        return new ConfidenceScore(value);
    }
}
