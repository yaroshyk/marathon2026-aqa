namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>One dish line: a dish and how many of it.</summary>
public sealed record DishLine(string Dish, int Quantity)
{
    public override string ToString() => $"{Quantity} x {Dish}";
}

/// <summary>How a guest's cart looks to the owner, whether read from the screen or from the service.</summary>
public sealed record ParticipantView(string Name, IReadOnlyList<DishLine> Dishes, decimal Subtotal, bool Submitted)
{
    /// <summary>Order-insensitive comparison key, readable in an assertion message.</summary>
    public string Describe() =>
        $"{Name} [{(Submitted ? "submitted" : "still choosing")}] {Subtotal:0.00}: " +
        (Dishes.Count == 0 ? "no dishes" : string.Join(", ", Dishes.OrderBy(d => d.Dish, StringComparer.Ordinal)));
}

/// <summary>The summary part of the owner's screen.</summary>
public sealed record GroupPreOrderSummary(
    string StatusBadge,
    decimal GrandTotal,
    IReadOnlyList<DishLine> Consolidated,
    string? PendingNotice,
    bool CanSubmit,
    int RemoveButtons);

/// <summary>Who is signed in on the storefront in this scenario's browser.</summary>
public sealed record StorefrontUser(Persona Persona);
