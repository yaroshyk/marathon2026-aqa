using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Orders;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

/// <summary>The group snapshot the service returned last (participants list or the list left after a removal).</summary>
public sealed class TheGroupSnapshot : IQuestion<GroupSnapshotResponse>
{
    public Task<GroupSnapshotResponse> AnsweredByAsync(Actor actor) =>
        Task.FromResult(actor.TheLastResponse().Json<GroupSnapshotResponse>());
}

/// <summary>The guests in the last group snapshot, as the owner sees them (the owner's own session left out).</summary>
public sealed class TheGuestsInTheSnapshot : IQuestion<IReadOnlyList<ParticipantView>>
{
    public async Task<IReadOnlyList<ParticipantView>> AnsweredByAsync(Actor actor)
    {
        var snapshot = await new TheGroupSnapshot().AnsweredByAsync(actor);
        return snapshot.Participants
            .Where(p => !p.IsOwner)
            .Select(p => new ParticipantView(
                p.Name,
                p.Items.Select(i => new DishLine(i.Dish.Name, i.Quantity)).ToList(),
                p.Total,
                p.IsSubmitted))
            .ToList();
    }
}

/// <summary>The consolidated dishes the service answered a successful submit with, by dish name.</summary>
public sealed class TheSubmittedDishes : IQuestion<IReadOnlyList<DishLine>>
{
    public Task<IReadOnlyList<DishLine>> AnsweredByAsync(Actor actor)
    {
        var names = actor.Recall<GroupBooking>().Menu.ToDictionary(d => d.Id, d => d.Name);
        IReadOnlyList<DishLine> dishes = actor.TheLastResponse().Json<List<ConsolidatedDishResponse>>()
            .Select(d => new DishLine(names.GetValueOrDefault(d.DishId, d.DishId), d.Quantity))
            .OrderBy(d => d.Dish, StringComparer.Ordinal)
            .ToList();
        return Task.FromResult(dishes);
    }
}

/// <summary>The reservation's order as the owner reads it from the service.</summary>
public sealed class TheRestaurantOrder : IQuestion<GetOrderResponse>
{
    public async Task<GetOrderResponse> AnsweredByAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        var response = await new OrdersApi(actor.UsingTheApi()).GetOrderAsync(actor.Recall<GroupBooking>().ReservationId, token);
        return response.Ok
            ? await response.ReadAsAsync<GetOrderResponse>()
            : throw new InvalidOperationException($"The owner could not read the order: {response.Status} {await response.ReadErrorMessageAsync()}");
    }
}

/// <summary>The status of the owner's group ("Open", "Submitted", "Closed").</summary>
public sealed class TheGroupStatus : IQuestion<string>
{
    public async Task<string> AnsweredByAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        var response = await new GroupPreOrdersApi(actor.UsingTheApi()).GetStatusAsync(actor.Recall<GroupBooking>().GroupId, token);
        return (await response.ReadAsAsync<GroupStatusResponse>()).Status;
    }
}

/// <summary>The names of the guests in the owner's group, read fresh from the service by the owner.</summary>
public sealed class TheGuestNamesTheOwnerSees : IQuestion<IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> AnsweredByAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        var response = await new GroupPreOrdersApi(actor.UsingTheApi()).GetParticipantsAsync(actor.Recall<GroupBooking>().GroupId, token);
        var snapshot = await response.ReadAsAsync<GroupSnapshotResponse>();
        return snapshot.Participants.Where(p => !p.IsOwner).Select(p => p.Name).ToList();
    }
}
