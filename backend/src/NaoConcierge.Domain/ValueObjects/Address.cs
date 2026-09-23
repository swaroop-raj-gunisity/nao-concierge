namespace NaoConcierge.Domain.ValueObjects;

public record Address(string Line1, string? Line2, string City, string State, string PostalCode, string Country);
