using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;

namespace Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

/// <summary>
/// A table the owner booked for a group: the reservation, the group behind its invite link and the dishes the
/// guests can choose from. Scenarios name dishes "dish A", "dish B", … — the n-th available dish of the
/// location — so the feature files never depend on what a particular deployment has on its menu.
/// </summary>
public sealed record GroupBooking(ReservationResponse Reservation, GuestInviteResponse Invite, IReadOnlyList<DishResponse> Menu)
{
    public string ReservationId => Reservation.Id;

    public string GroupId => Invite.GroupReservationId;

    public DishResponse Dish(string alias)
    {
        var letter = alias.Trim().Split(' ')[^1];
        var index = char.ToUpperInvariant(letter[0]) - 'A';
        return index >= 0 && index < Menu.Count
            ? Menu[index]
            : throw new InvalidOperationException(
                $"'{alias}' needs at least {index + 1} available dishes at the location, but it has {Menu.Count}.");
    }
}
