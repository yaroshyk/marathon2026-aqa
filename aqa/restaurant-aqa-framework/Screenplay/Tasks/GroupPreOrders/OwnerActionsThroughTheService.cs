using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

/// <summary>Which group a request is about: the owner's group, or one that does not exist.</summary>
public enum GroupTarget
{
    TheOwnersGroup,
    AGroupThatDoesNotExist,
}

internal static class Targets
{
    public const string UnknownId = "00000000-0000-0000-0000-000000000000";

    public static string GroupId(Actor actor, GroupTarget target) =>
        target == GroupTarget.TheOwnersGroup ? actor.Recall<GroupBooking>().GroupId : UnknownId;
}

/// <summary>Someone asks the service for the participants of a group; the answer is the scenario's last response.</summary>
public sealed class AskForTheParticipants(Persona who, GroupTarget target = GroupTarget.TheOwnersGroup) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(who));
        var response = await new GroupPreOrdersApi(actor.UsingTheApi()).GetParticipantsAsync(Targets.GroupId(actor, target), token);
        await actor.TheLastResponse().CaptureAsync(response);
    }
}

/// <summary>Someone submits the consolidated group pre-order through the service.</summary>
public sealed class SubmitTheGroupPreOrderThroughTheService(Persona who, GroupTarget target = GroupTarget.TheOwnersGroup) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(who));
        var reservationId = target == GroupTarget.TheOwnersGroup ? actor.Recall<GroupBooking>().ReservationId : Targets.UnknownId;
        var response = await new GroupPreOrdersApi(actor.UsingTheApi())
            .SubmitAsync(Targets.GroupId(actor, target), new SubmitGroupPreOrderRequest(reservationId), token);
        await actor.TheLastResponse().CaptureAsync(response);
    }
}

/// <summary>
/// Someone removes a participant from the owner's group through the service. <paramref name="guestName"/> names a guest
/// remembered by the scenario (of this group or another one); null means a participant that does not exist.
/// </summary>
public sealed class RemoveTheParticipantThroughTheService(Persona who, string? guestName) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var token = await actor.AsksForAsync(new TheTokenOf(who));
        var guest = guestName is null ? null : actor.Recall<JoinedGuest>(guestName);
        var response = await new GroupPreOrdersApi(actor.UsingTheApi())
            .RemoveParticipantAsync(actor.Recall<GroupBooking>().GroupId, guest?.SessionId ?? Targets.UnknownId, token);
        await actor.TheLastResponse().CaptureAsync(response);

        if (response.Ok && guest is not null)
            guest.Removed = true;
    }
}
