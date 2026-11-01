using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;

/// <summary>The owner's view of a group pre-order: every participant and the total of all carts.</summary>
public sealed record GroupSnapshotResponse(IReadOnlyList<ParticipantResponse> Participants, decimal Total);

public sealed record ParticipantResponse(
    string Id,
    string Type,
    string Name,
    IReadOnlyList<ParticipantItemResponse> Items,
    decimal Total,
    bool IsSubmitted)
{
    public bool IsOwner => string.Equals(Type, "Owner", StringComparison.OrdinalIgnoreCase);
}

public sealed record ParticipantItemResponse(DishResponse Dish, int Quantity);
