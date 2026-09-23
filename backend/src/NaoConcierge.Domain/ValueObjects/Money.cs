namespace NaoConcierge.Domain.ValueObjects;

public record Money(decimal Amount, string Currency = "USD");
