using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Configuration;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

/// <summary>
/// The bearer token a persona sends: an id token for the signed-in personas (signed in through the service once
/// per scenario), the guest token of the first guest who joined for a guest, and none for a visitor.
/// </summary>
public sealed class TheTokenOf(Persona persona) : IQuestion<string?>
{
    public async Task<string?> AnsweredByAsync(Actor actor)
    {
        switch (persona)
        {
            case Persona.Visitor:
                return null;
            case Persona.GuestOfTheGroup:
                var guests = actor.RecallAll<JoinedGuest>();
                return (guests.Count > 0 ? guests[0].SessionToken : null)
                    ?? throw new InvalidOperationException("No guest has joined the group yet, so there is no guest token to use.");
        }

        if (actor.TryRecallNamed(persona.ToString(), out SignedInPersona signedIn))
            return signedIn.IdToken;

        var (username, password) = CredentialsOf(persona);
        var response = await new AuthApi(actor.UsingTheApi()).SignInAsync(new SignInRequest(username, password));
        if (!response.Ok)
            throw new InvalidOperationException(
                $"{persona} could not sign in as '{username}': {response.Status} {await response.ReadErrorMessageAsync()}");

        var token = (await response.ReadAsAsync<SignInResponse>()).IdToken;
        actor.Remember(persona.ToString(), new SignedInPersona(persona, token));
        return token;
    }

    public static (string Username, string Password) CredentialsOf(Persona persona)
    {
        var settings = Settings.Current;
        return persona switch
        {
            Persona.Owner => settings.RequireCustomer(),
            Persona.AnotherCustomer => settings.RequireOtherCustomer(),
            Persona.AssignedWaiter => (settings.RequireTestTable().WaiterUsername, settings.RequireTestTable().WaiterPassword),
            Persona.WaiterOfAnotherLocation => settings.RequireWaiter(),
            _ => throw new ArgumentOutOfRangeException(nameof(persona), persona, "This persona has no account."),
        };
    }
}
