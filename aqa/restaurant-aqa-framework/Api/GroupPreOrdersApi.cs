using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>
/// Group pre-order routes: the invite, the guests' sessions and carts (guest token), and the owner's view,
/// submit and removal (Cognito token of the reservation owner).
/// </summary>
public sealed class GroupPreOrdersApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> GetInviteTokenAsync(string reservationId, string? bearerToken = null) =>
        GetAsync(Url($"/reservations/{reservationId}/invite-token"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> GetInviteAsync(string token) =>
        GetAsync(Url($"/guest/invites/{token}"));

    public Task<IAPIResponse> JoinAsync(CreateGuestSessionRequest request) =>
        Api.PostAsync(Url("/guest/sessions"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(),
        });

    public Task<IAPIResponse> AddDishAsync(string guestSessionId, GuestCartDishRequest request, string guestToken) =>
        Api.PostAsync(Url($"/guest/sessions/{guestSessionId}/dishes"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(guestToken),
        });

    public Task<IAPIResponse> RemoveDishAsync(string guestSessionId, GuestCartDishRequest request, string guestToken) =>
        DeleteAsync(Url($"/guest/sessions/{guestSessionId}/dishes"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(guestToken),
        });

    public Task<IAPIResponse> SubmitCartAsync(string guestSessionId, string guestToken) =>
        Api.PostAsync(Url($"/guest/sessions/{guestSessionId}/submit"), new APIRequestContextOptions
        {
            Headers = Headers(guestToken),
        });

    public Task<IAPIResponse> GetParticipantsAsync(string groupReservationId, string? bearerToken = null) =>
        GetAsync(Url($"/guest/groups/{groupReservationId}/sessions"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> GetStatusAsync(string groupReservationId, string? bearerToken = null) =>
        GetAsync(Url($"/guest/groups/{groupReservationId}/status"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> GetOwnerSessionIdAsync(string reservationId, string? bearerToken = null) =>
        GetAsync(Url($"/reservations/{reservationId}/guest-session-id"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> SubmitAsync(string groupReservationId, SubmitGroupPreOrderRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url($"/group-reservations/{groupReservationId}/submit"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> RemoveParticipantAsync(string groupReservationId, string guestSessionId, string? bearerToken = null) =>
        DeleteAsync(Url($"/guest/groups/{groupReservationId}/sessions/{guestSessionId}"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });
}
