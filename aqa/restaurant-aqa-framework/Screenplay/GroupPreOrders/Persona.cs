namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>Who takes part in a group pre-order scenario. The actor of a scenario plays all of them in turn.</summary>
public enum Persona
{
    /// <summary>The customer who booked the table (CUSTOMER_*).</summary>
    Owner,

    /// <summary>A signed-in customer who is not the owner (OTHER_CUSTOMER_*).</summary>
    AnotherCustomer,

    /// <summary>The waiter of the test table's location (TESTTABLE_WAITER_*), who serves the waiter scenarios' reservations.</summary>
    AssignedWaiter,

    /// <summary>The standing waiter account (WAITER_*): a waiter, but never the owner of a group.</summary>
    WaiterOfAnotherLocation,

    /// <summary>A guest who joined the group by the invite link; has a guest token, not an account.</summary>
    GuestOfTheGroup,

    /// <summary>Someone who is not signed in at all.</summary>
    Visitor,
}
