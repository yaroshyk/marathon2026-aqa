using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;
using Restaurant_Aqa_Framework.Api.Dtos.Reservations;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Configuration;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

/// <summary>
/// The owner books the test table for a group and opens it for guests: creates the reservation (which creates the
/// group behind it), reads the invite link and the location's available dishes, and schedules the cancellation.
/// Every booking gets its own random slot months ahead (start time = now + N days), so parallel scenarios and
/// repeated runs never meet on the same table and time; a slot that is taken anyway is retried with another one.
/// </summary>
public sealed class BookATableForAGroup(int groupSize, string? name = null, bool atTheWaitersTable = false) : ITask
{
    private const int Attempts = 6;

    public async Task PerformAsAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(Persona.Owner));
        var api = actor.UsingTheApi();
        var reservations = new ReservationsApi(api);
        var tableId = atTheWaitersTable ? Settings.Current.RequireTestTable().TableId : Settings.Current.RequireGroupTable();

        ReservationResponse? reservation = null;
        var lastError = "";
        for (var attempt = 1; attempt <= Attempts && reservation is null; attempt++)
        {
            var start = RandomFutureSlot();
            var response = await reservations.CreateReservationAsync(
                new CreateReservationRequest(tableId, start, start.AddMinutes(90), groupSize), token);
            if (response.Ok)
                reservation = await response.ReadAsAsync<ReservationResponse>();
            else
                lastError = $"{response.Status}: {await response.ReadErrorMessageAsync()}";
        }

        if (reservation is null)
            throw new InvalidOperationException($"The owner could not book table {tableId} for {groupSize} after {Attempts} attempts. Last answer: {lastError}");

        actor.Recall<CleanupScope>().TrackDelete(
            $"reservation:{reservation.Id}", () => reservations.CancelReservationAsync(reservation.Id, token));

        var groups = new GroupPreOrdersApi(api);
        var inviteToken = await (await groups.GetInviteTokenAsync(reservation.Id, token)).ReadAsAsync<InviteTokenResponse>();
        var invite = await (await groups.GetInviteAsync(inviteToken.Token)).ReadAsAsync<GuestInviteResponse>();

        var dishes = await (await new LocationsApi(api).GetDishesForLocationAsync(invite.LocationId)).ReadAsAsync<List<DishResponse>>();
        var menu = dishes
            .Where(d => string.Equals(d.State, "Available", StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.Name, StringComparer.Ordinal)
            .ToList();

        var booking = new GroupBooking(reservation, invite, menu);
        if (name is null)
        {
            actor.Remember(booking);
            return;
        }

        // A named booking is a second group next to the scenario's main one: keep the main one "the current" booking.
        var hasMain = actor.TryRecall<GroupBooking>(out var main);
        actor.Remember(name, booking);
        if (hasMain)
            actor.Remember(main);
    }

    private static DateTime RandomFutureSlot()
    {
        var day = DateTime.UtcNow.Date.AddDays(Random.Shared.Next(60, 900));
        // 08:00–17:45 UTC keeps the whole visit on the same calendar day in any European time zone.
        return DateTime.SpecifyKind(day.AddHours(Random.Shared.Next(8, 18)).AddMinutes(15 * Random.Shared.Next(0, 4)), DateTimeKind.Utc);
    }
}
