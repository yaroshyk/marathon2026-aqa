namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>
/// A guest who joined a group pre-order by its invite link: their guest session, their cart as the test built it
/// (dish id → quantity, in the order dishes were added) and whether they confirmed it. This is what the
/// owner is expected to see.
/// </summary>
public sealed class JoinedGuest(string name, string groupId, string sessionId, string sessionToken)
{
    public string Name { get; } = name;
    public string GroupId { get; } = groupId;
    public string SessionId { get; } = sessionId;
    public string SessionToken { get; } = sessionToken;
    public Dictionary<string, int> Cart { get; } = new();
    public bool Submitted { get; set; }
    public bool Removed { get; set; }

    public void Add(string dishId, int quantity) => Cart[dishId] = Cart.GetValueOrDefault(dishId) + quantity;

    public void Remove(string dishId, int quantity)
    {
        var left = Cart.GetValueOrDefault(dishId) - quantity;
        if (left > 0)
            Cart[dishId] = left;
        else
            Cart.Remove(dishId);
    }
}
