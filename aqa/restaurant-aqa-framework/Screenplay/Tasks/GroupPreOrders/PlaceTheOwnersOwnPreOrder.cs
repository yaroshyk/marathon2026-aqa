using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Orders;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

/// <summary>The owner pre-orders dishes for themselves; the group's dishes are added to this order on submit.</summary>
public sealed class PlaceTheOwnersOwnPreOrder(string dishAlias, int quantity) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var booking = actor.Recall<GroupBooking>();
        var dish = booking.Dish(dishAlias);
        var token = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        var orders = new OrdersApi(actor.UsingTheApi());

        var response = await orders.CreateOrderAsync(
            new CreateOrderRequest(booking.ReservationId, [new OrderItemRequest(dish.Id, quantity)]), token);
        if (!response.Ok)
            throw new InvalidOperationException($"The owner could not place their own pre-order: {response.Status} {await response.ReadErrorMessageAsync()}");

        actor.Remember(new OwnersOwnPreOrder(dish.Id, quantity));
        actor.Recall<CleanupScope>().TrackDelete($"order:{booking.ReservationId}", () => orders.DeleteOrderAsync(booking.ReservationId, token));
    }
}
