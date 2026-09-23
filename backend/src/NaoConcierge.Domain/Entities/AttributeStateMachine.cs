using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Domain.Entities;

public static class AttributeStateMachine
{
    private static readonly Dictionary<AttributeState, HashSet<AttributeState>> ValidTransitions = new()
    {
        [AttributeState.CapturedDirect] = new(),
        [AttributeState.ExtractedUnconfirmed] = new()
        {
            AttributeState.ExtractedConfirmed,
            AttributeState.ClarificationRequired,
            AttributeState.Rejected
        },
        [AttributeState.ClarificationRequired] = new()
        {
            AttributeState.ExtractedUnconfirmed,
            AttributeState.Rejected
        },
        [AttributeState.ExtractedConfirmed] = new(),
        [AttributeState.Rejected] = new()
    };

    public static bool CanTransition(AttributeState from, AttributeState to)
    {
        return ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static void ValidateTransition(AttributeState from, AttributeState to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Invalid state transition from {from} to {to}.");
    }
}
