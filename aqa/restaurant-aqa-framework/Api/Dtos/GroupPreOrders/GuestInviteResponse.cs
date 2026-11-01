namespace Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;

/// <summary>What an invite link reveals about the group reservation (GET /guest/invites/{token}).</summary>
public sealed record GuestInviteResponse(
    string GroupReservationId,
    string LocationId,
    string LocationAddress,
    DateTime StartTime,
    DateTime EndTime,
    int MaxGuestCount);
