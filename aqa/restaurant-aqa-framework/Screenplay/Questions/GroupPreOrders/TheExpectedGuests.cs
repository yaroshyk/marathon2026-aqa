using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

/// <summary>
/// What the owner should see for each guest of the owner's group who is still in it, built from what the scenario's
/// guests put in their carts and the menu prices — never from the application's own answer.
/// </summary>
public sealed class TheExpectedGuests : IQuestion<IReadOnlyList<ParticipantView>>
{
    public Task<IReadOnlyList<ParticipantView>> AnsweredByAsync(Actor actor)
    {
        var booking = actor.Recall<GroupBooking>();
        var dishes = booking.Menu.ToDictionary(d => d.Id);

        IReadOnlyList<ParticipantView> expected = actor.RecallAll<JoinedGuest>()
            .Where(g => g.GroupId == booking.GroupId && !g.Removed)
            .DistinctBy(g => g.SessionId)
            .Select(g => new ParticipantView(
                g.Name,
                g.Cart.Select(c => new DishLine(dishes[c.Key].Name, c.Value)).ToList(),
                g.Cart.Sum(c => dishes[c.Key].Price * c.Value),
                g.Submitted))
            .ToList();

        return Task.FromResult(expected);
    }
}

/// <summary>The consolidated pre-order the restaurant should get: the submitted carts of guests still in the group, merged per dish.</summary>
public sealed class TheExpectedConsolidatedPreOrder : IQuestion<IReadOnlyList<DishLine>>
{
    public async Task<IReadOnlyList<DishLine>> AnsweredByAsync(Actor actor)
    {
        var guests = await new TheExpectedGuests().AnsweredByAsync(actor);
        return guests
            .Where(g => g.Submitted)
            .SelectMany(g => g.Dishes)
            .GroupBy(d => d.Dish)
            .Select(g => new DishLine(g.Key, g.Sum(d => d.Quantity)))
            .OrderBy(d => d.Dish, StringComparer.Ordinal)
            .ToList();
    }
}
