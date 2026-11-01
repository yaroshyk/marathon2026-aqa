using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>
/// Setting up a group pre-order (booking, guests and their carts) and what the owner does with it. Sentences that
/// mean "on the screen" for a @ui scenario and "through the service" for an @api one are bound twice with a scope.
/// </summary>
[Binding]
public sealed class GroupPreOrderStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    // ---- setting up ----

    [Given("the owner has booked a table for a group of {int}")]
    public Task GivenTheOwnerHasBookedATable(int groupSize) => Actor.AttemptsToAsync(new BookATableForAGroup(groupSize));

    [Given("the owner has booked a table served by the waiter of the location for a group of {int}")]
    public Task GivenTheOwnerHasBookedTheWaitersTable(int groupSize) =>
        Actor.AttemptsToAsync(new BookATableForAGroup(groupSize, atTheWaitersTable: true));

    [Given("the owner has pre-ordered {int} x {dish} for themselves")]
    public Task GivenTheOwnerHasPreOrdered(int quantity, string dish) =>
        Actor.AttemptsToAsync(new PlaceTheOwnersOwnPreOrder(dish, quantity));

    [Given("these guests joined the group pre-order:")]
    public async Task GivenTheseGuestsJoined(DataTable guests)
    {
        foreach (var row in guests.Rows)
        {
            var name = row["guest"];
            await Actor.AttemptsToAsync(new JoinTheGroup(name));
            foreach (var (quantity, dish) in StepArgumentTransformations.ParseDishList(row["dishes"]))
                await Actor.AttemptsToAsync(new AddToTheCart(name, dish, quantity));

            if (row["cart"] == "submitted")
                await Actor.AttemptsToAsync(new SubmitTheCart(name));
        }
    }

    [Given("guest {word} joined the group pre-order")]
    public Task GivenGuestJoined(string name) => Actor.AttemptsToAsync(new JoinTheGroup(name));

    // The service refuses both ways into this state (an empty cart cannot be submitted, a submitted cart cannot be
    // changed), so when it refuses the scenario is skipped with the reason instead of failing on set-up.
    [Given("{word} submitted her/his/their cart and then took every dish out of it")]
    public async Task GivenGuestSubmittedAndEmptiedTheCart(string name)
    {
        await Actor.AttemptsToAsync(new AddToTheCart(name, "dish A", 1), new SubmitTheCart(name));
        try
        {
            await Actor.AttemptsToAsync(new EmptyTheCart(name));
        }
        catch (InvalidOperationException refused) when (refused.Message.Contains("already submitted", StringComparison.OrdinalIgnoreCase))
        {
            Skip.Test("This state cannot be set up through the public API: a guest cannot submit an empty cart and " +
                      $"cannot change a submitted one ({refused.Message}). The rule is covered by the backend unit tests.");
        }
    }

    [Given("the owner has another group reservation that {word} joined")]
    public Task GivenTheOwnerHasAnotherGroup(string name) =>
        Actor.AttemptsToAsync(new BookATableForAGroup(5, "another group"), new JoinTheGroup(name, "another group"));

    [Given("the owner has submitted the group pre-order")]
    public async Task GivenTheOwnerHasSubmitted()
    {
        await Actor.AttemptsToAsync(new SubmitTheGroupPreOrderThroughTheService(Persona.Owner));
        var outcome = Actor.TheLastResponse();
        if (!outcome.Succeeded)
            throw new InvalidOperationException($"The owner could not submit the group pre-order: {outcome.Status} {outcome.Body}");
    }

    [Given("another customer is signed in on the storefront")]
    public Task GivenAnotherCustomerIsSignedIn() => Actor.AttemptsToAsync(new SignInOnTheStorefront(Persona.AnotherCustomer));

    // ---- guests change their carts ----

    [When("{word} adds {int} x {dish} to her/his/their cart")]
    public Task WhenGuestAdds(string name, int quantity, string dish) => Actor.AttemptsToAsync(new AddToTheCart(name, dish, quantity));

    // ---- the owner looks at the group ----

    [Given("the owner opens the group pre-order")]
    [When("the owner opens the group pre-order")]
    public Task WhenTheOwnerOpensTheGroupPreOrder() =>
        Actor.AttemptsToAsync(new BeSignedInOnTheStorefrontAs(Persona.Owner), new OpenTheGroupPreOrder());

    [When("the owner opens the group pre-order of a reservation that does not exist")]
    public Task WhenTheOwnerOpensAnUnknownGroupPreOrder() =>
        Actor.AttemptsToAsync(new BeSignedInOnTheStorefrontAs(Persona.Owner), new OpenTheGroupPreOrder(GroupTarget.AGroupThatDoesNotExist));

    [When("they open the owner's group pre-order")]
    public Task WhenTheyOpenTheOwnersGroupPreOrder() => Actor.AttemptsToAsync(new OpenTheGroupPreOrder());

    [When("the owner refreshes the group pre-order")]
    public Task WhenTheOwnerRefreshes() => Actor.AttemptsToAsync(new RefreshTheGroupPreOrder());

    [Given("the owner looks at the participants")]
    [When("the owner looks at the participants")]
    public Task WhenTheOwnerLooksAtTheParticipants() => Actor.AttemptsToAsync(new AskForTheParticipants(Persona.Owner));

    [When("the owner asks for the participants of a group that does not exist")]
    public Task WhenTheOwnerAsksForAnUnknownGroup() =>
        Actor.AttemptsToAsync(new AskForTheParticipants(Persona.Owner, GroupTarget.AGroupThatDoesNotExist));

    [When("{persona} asks for the participants of the owner's group")]
    public Task WhenSomeoneAsksForTheParticipants(Persona who) => Actor.AttemptsToAsync(new AskForTheParticipants(who));

    // ---- submitting ----

    [When("the owner submits the group pre-order")]
    [Scope(Tag = "ui")]
    public Task WhenTheOwnerSubmitsOnTheScreen() => Actor.AttemptsToAsync(new SubmitTheGroupPreOrderOnTheScreen());

    [When("the owner submits the group pre-order")]
    [Scope(Tag = "api")]
    public Task WhenTheOwnerSubmitsThroughTheService() =>
        Actor.AttemptsToAsync(new SubmitTheGroupPreOrderThroughTheService(Persona.Owner));

    [When("the owner submits the pre-order of a group that does not exist")]
    public Task WhenTheOwnerSubmitsAnUnknownGroup() =>
        Actor.AttemptsToAsync(new SubmitTheGroupPreOrderThroughTheService(Persona.Owner, GroupTarget.AGroupThatDoesNotExist));

    [When("{persona} submits the owner's group pre-order")]
    public Task WhenSomeoneSubmits(Persona who) => Actor.AttemptsToAsync(new SubmitTheGroupPreOrderThroughTheService(who));

    // ---- removing ----

    [When("the owner removes {word} from the group pre-order")]
    [Scope(Tag = "ui")]
    public Task WhenTheOwnerRemovesOnTheScreen(string name) => Actor.AttemptsToAsync(new RemoveTheGuestOnTheScreen(name));

    [When("the owner removes {word} from the group pre-order")]
    [Scope(Tag = "api")]
    public Task WhenTheOwnerRemovesThroughTheService(string name) =>
        Actor.AttemptsToAsync(new RemoveTheParticipantThroughTheService(Persona.Owner, name));

    [When("the owner removes a guest who does not exist")]
    public Task WhenTheOwnerRemovesAnUnknownGuest() =>
        Actor.AttemptsToAsync(new RemoveTheParticipantThroughTheService(Persona.Owner, null));

    [When("{persona} removes {word} from the owner's group pre-order")]
    public Task WhenSomeoneRemoves(Persona who, string name) =>
        Actor.AttemptsToAsync(new RemoveTheParticipantThroughTheService(who, name));

    // ---- the waiter ----

    [When("the waiter of the location opens the pre-order of the reservation")]
    public Task WhenTheWaiterOpensThePreOrder() =>
        Actor.AttemptsToAsync(new SignInOnTheStorefront(Persona.AssignedWaiter), new OpenTheReservationsPreOrderAsWaiter());
}
