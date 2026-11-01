using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

/// <summary>A guest opens the invite link of a group and joins it under their name; the owner removes them again on cleanup.</summary>
public sealed class JoinTheGroup(string guestName, string? groupName = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var booking = actor.Recall<GroupBooking>(groupName);
        var api = new GroupPreOrdersApi(actor.UsingTheApi());
        var response = await api.JoinAsync(new CreateGuestSessionRequest(booking.GroupId, guestName));
        await GuestCart.EnsureOkAsync(response, $"{guestName} could not join the group");

        var session = await response.ReadAsAsync<CreateGuestSessionResponse>();
        var guest = new JoinedGuest(guestName, booking.GroupId, session.SessionId, session.SessionToken);
        actor.Remember(guestName, guest);

        var ownerToken = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        actor.Recall<CleanupScope>().TrackDelete(
            $"guest-session:{guestName}", () => api.RemoveParticipantAsync(booking.GroupId, session.SessionId, ownerToken));
    }
}

/// <summary>A guest adds dishes to their own cart.</summary>
public sealed class AddToTheCart(string guestName, string dishAlias, int quantity, string? groupName = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var guest = actor.Recall<JoinedGuest>(guestName);
        var dish = actor.Recall<GroupBooking>(groupName).Dish(dishAlias);
        var response = await new GroupPreOrdersApi(actor.UsingTheApi())
            .AddDishAsync(guest.SessionId, new GuestCartDishRequest(dish.Id, quantity), guest.SessionToken);
        await GuestCart.EnsureOkAsync(response, $"{guestName} could not add {quantity} x {dish.Name}");
        guest.Add(dish.Id, quantity);
    }
}

/// <summary>A guest takes every dish out of their cart.</summary>
public sealed class EmptyTheCart(string guestName) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var guest = actor.Recall<JoinedGuest>(guestName);
        var api = new GroupPreOrdersApi(actor.UsingTheApi());
        foreach (var (dishId, quantity) in guest.Cart.ToList())
        {
            var response = await api.RemoveDishAsync(guest.SessionId, new GuestCartDishRequest(dishId, quantity), guest.SessionToken);
            await GuestCart.EnsureOkAsync(response, $"{guestName} could not remove a dish");
            guest.Remove(dishId, quantity);
        }
    }
}

/// <summary>A guest confirms their cart.</summary>
public sealed class SubmitTheCart(string guestName) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var guest = actor.Recall<JoinedGuest>(guestName);
        var response = await new GroupPreOrdersApi(actor.UsingTheApi()).SubmitCartAsync(guest.SessionId, guest.SessionToken);
        await GuestCart.EnsureOkAsync(response, $"{guestName} could not submit the cart");
        guest.Submitted = true;
    }
}

internal static class GuestCart
{
    public static async Task EnsureOkAsync(Microsoft.Playwright.IAPIResponse response, string what)
    {
        if (!response.Ok)
            throw new InvalidOperationException($"{what}: {response.Status} {await response.ReadErrorMessageAsync()}");
    }
}
