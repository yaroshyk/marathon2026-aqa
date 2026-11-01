namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>The owner's own pre-order for the reservation (the order the group's dishes are added to).</summary>
public sealed record OwnersOwnPreOrder(string DishId, int Quantity);
