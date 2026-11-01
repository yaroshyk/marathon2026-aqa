using Restaurant_Aqa_Framework.Api.Dtos.Orders;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>What the owner and the waiter see. Expected values come from what the scenario's guests did, never from the app.</summary>
[Binding]
public sealed class GroupPreOrderCheckStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    // ---- participants ----

    [Given("the owner sees every guest with their dishes, quantities and subtotal")]
    [Then("the owner sees every guest with their dishes, quantities and subtotal")]
    [Scope(Tag = "ui")]
    public async Task ThenTheOwnerSeesEveryGuestOnTheScreen() =>
        await ShouldMatchAsync(await Actor.AsksForAsync(new TheGuestsOnTheScreen()));

    [Then("the owner sees every guest with their dishes, quantities and subtotal")]
    [Scope(Tag = "api")]
    public async Task ThenTheOwnerSeesEveryGuestInTheService() =>
        await ShouldMatchAsync(await Actor.AsksForAsync(new TheGuestsInTheSnapshot()));

    [Then("the grand total is the sum of the submitted carts")]
    public async Task ThenTheGrandTotalIsTheSumOfSubmittedCarts()
    {
        var expected = (await Actor.AsksForAsync(new TheExpectedGuests())).Where(g => g.Submitted).Sum(g => g.Subtotal);
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.GrandTotal).IsEqualTo(expected).Because($"the screen shows {summary}");
    }

    [Then("the group total is the sum of every participant's total")]
    public async Task ThenTheGroupTotalIsTheSumOfParticipants()
    {
        var snapshot = await Actor.AsksForAsync(new TheGroupSnapshot());
        var expected = (await Actor.AsksForAsync(new TheExpectedGuests())).Sum(g => g.Subtotal);

        await Assert.That(snapshot.Total).IsEqualTo(snapshot.Participants.Sum(p => p.Total)).Because(Actor.TheLastResponse().Body);
        await Assert.That(snapshot.Total).IsEqualTo(expected).Because(Actor.TheLastResponse().Body);
    }

    [Then("{word} is listed as still choosing")]
    public async Task ThenGuestIsStillChoosing(string name)
    {
        var guest = await GuestInSnapshotAsync(name);
        await Assert.That(guest.Submitted).IsFalse().Because(guest.Describe());
        await Assert.That(guest.Dishes.Count).IsGreaterThan(0).Because(guest.Describe());
    }

    [Then("{word} is listed with no dishes and a total of zero")]
    public async Task ThenGuestHasNoDishes(string name)
    {
        var guest = await GuestInSnapshotAsync(name);
        await Assert.That(guest.Dishes.Count).IsEqualTo(0).Because(guest.Describe());
        await Assert.That(guest.Subtotal).IsEqualTo(0m).Because(guest.Describe());
    }

    [Then("nobody is listed and the group total is zero")]
    public async Task ThenNobodyIsListed()
    {
        var snapshot = await Actor.AsksForAsync(new TheGroupSnapshot());
        await Assert.That(snapshot.Participants.Count).IsEqualTo(0).Because(Actor.TheLastResponse().Body);
        await Assert.That(snapshot.Total).IsEqualTo(0m).Because(Actor.TheLastResponse().Body);
    }

    [Then("the owner is warned that some participants have not submitted their cart")]
    public async Task ThenTheOwnerIsWarned()
    {
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.PendingNotice ?? "").Contains("haven't submitted their cart yet").Because($"the screen shows {summary}");
    }

    [Then("the owner is told that nobody has added any dishes yet")]
    public async Task ThenTheOwnerIsToldNobodyAddedDishes() =>
        await Assert.That(await Actor.AsksForAsync(new TheEmptyGroupMessageIsShown())).IsTrue();

    [Then("no guest is listed")]
    public async Task ThenNoGuestIsListed() =>
        await Assert.That(await Actor.AsksForAsync(new TheNumberOfGuestsOnTheScreen())).IsEqualTo(0);

    [Then("the group pre-order cannot be loaded")]
    public async Task ThenTheGroupPreOrderCannotBeLoaded() =>
        await Assert.That(await Actor.AsksForAsync(new TheGroupPreOrderProblem())).IsEqualTo("Couldn't load the group pre-order");

    [Then("the owner is told on the screen that the group reservation was not found")]
    public async Task ThenTheScreenSaysNotFound() =>
        await Assert.That(await Actor.AsksForAsync(new TheGroupPreOrderProblem())).IsEqualTo("Group reservation not found");

    [Then("{word} is no longer listed")]
    [Scope(Tag = "ui")]
    public async Task ThenGuestIsNoLongerOnTheScreen(string name)
    {
        var names = (await Actor.AsksForAsync(new TheGuestsOnTheScreen())).Select(g => g.Name).ToList();
        await Assert.That(names).DoesNotContain(name).Because($"the screen lists {string.Join(", ", names)}");
    }

    [Then("{word} is no longer listed")]
    [Scope(Tag = "api")]
    public async Task ThenGuestIsNoLongerInTheSnapshot(string name)
    {
        var names = (await Actor.AsksForAsync(new TheGuestsInTheSnapshot())).Select(g => g.Name).ToList();
        await Assert.That(names).DoesNotContain(name).Because(Actor.TheLastResponse().Body);
    }

    [Then("{word} is still listed")]
    public async Task ThenGuestIsStillListed(string name)
    {
        var names = await Actor.AsksForAsync(new TheGuestNamesTheOwnerSees());
        await Assert.That(names).Contains(name).Because($"the owner sees {string.Join(", ", names)}");
    }

    // ---- submitting ----

    [Then("the owner is told that the pre-order was sent to the restaurant")]
    [Scope(Tag = "ui")]
    public async Task ThenTheScreenSaysSent()
    {
        await Assert.That(await Actor.AsksForAsync(new TheToast(error: false))).IsEqualTo("Pre-order sent to the restaurant");
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.StatusBadge).IsEqualTo("Sent to Restaurant");
    }

    [Then("the consolidated pre-order is:")]
    [Scope(Tag = "ui")]
    public async Task ThenTheConsolidatedPreOrderOnTheScreenIs(DataTable dishes) =>
        await ShouldBeAsync(dishes, (await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen())).Consolidated);

    [Then("the consolidated pre-order is:")]
    [Scope(Tag = "api")]
    public async Task ThenTheSubmittedDishesAre(DataTable dishes) =>
        await ShouldBeAsync(dishes, await Actor.AsksForAsync(new TheSubmittedDishes()));

    [Then("the restaurant's order holds the owner's dishes and the consolidated pre-order")]
    public async Task ThenTheOrderHoldsEverything()
    {
        var booking = Recall<GroupBooking>();
        var own = Recall<OwnersOwnPreOrder>();
        var expected = (await Actor.AsksForAsync(new TheExpectedConsolidatedPreOrder()))
            .Append(new DishLine(booking.Menu.Single(d => d.Id == own.DishId).Name, own.Quantity))
            .GroupBy(d => d.Dish)
            .Select(g => new DishLine(g.Key, g.Sum(d => d.Quantity)));

        var order = await Actor.AsksForAsync(new TheRestaurantOrder());
        var actual = order.OrderItems.Select(i => new DishLine(i.Dish.Name, i.Quantity));
        await Assert.That(Describe(actual)).IsEqualTo(Describe(expected));
    }

    [Then("the order is marked as submitted by the customer")]
    public async Task ThenTheOrderIsSubmittedByTheCustomer() =>
        await Assert.That((await Actor.AsksForAsync(new TheRestaurantOrder())).Status).IsEqualTo(OrderStatus.SubmittedByCustomer);

    [Then("the group pre-order is marked as submitted")]
    public async Task ThenTheGroupIsSubmitted() =>
        await Assert.That(await Actor.AsksForAsync(new TheGroupStatus())).IsEqualTo("Submitted");

    [Then("the group pre-order is still open")]
    public async Task ThenTheGroupIsStillOpen() =>
        await Assert.That(await Actor.AsksForAsync(new TheGroupStatus())).IsEqualTo("Open");

    [Then("the owner cannot submit the group pre-order")]
    public async Task ThenTheOwnerCannotSubmit()
    {
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.CanSubmit).IsFalse().Because($"the screen shows {summary}");
    }

    [Then("the owner is told on the screen that their own pre-order for the reservation was not found")]
    public async Task ThenTheScreenSaysTheOrderIsMissing()
    {
        var message = await Actor.AsksForAsync(new TheToast(error: true));
        await Assert.That(message).StartsWith("Order for reservation").And.EndsWith("not found.");
    }

    [Then("the group pre-order is still collecting pre-orders")]
    public async Task ThenTheScreenStillCollects()
    {
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.StatusBadge).IsEqualTo("Collecting pre-orders");
        await Assert.That(summary.CanSubmit).IsTrue().Because("the owner can try again");
    }

    [Then("the group pre-order can no longer be changed")]
    public async Task ThenTheGroupCanNoLongerBeChanged()
    {
        var summary = await Actor.AsksForAsync(new TheGroupPreOrderSummaryOnTheScreen());
        await Assert.That(summary.StatusBadge).IsEqualTo("Sent to Restaurant");
        await Assert.That(summary.CanSubmit).IsFalse().Because($"the screen shows {summary}");
        await Assert.That(summary.RemoveButtons).IsEqualTo(0).Because($"the screen shows {summary}");
    }

    // ---- the waiter ----

    [Then("the waiter sees these dishes:")]
    public async Task ThenTheWaiterSees(DataTable dishes) =>
        await ShouldBeAsync(dishes, await Actor.AsksForAsync(new TheDishesTheWaiterSees()));

    // ---- helpers ----

    private async Task ShouldMatchAsync(IReadOnlyList<ParticipantView> actual)
    {
        var expected = await Actor.AsksForAsync(new TheExpectedGuests());
        var expectedText = expected.Select(g => g.Describe()).Order(StringComparer.Ordinal).ToList();
        var actualText = actual.Select(g => g.Describe()).Order(StringComparer.Ordinal).ToList();
        await Assert.That(string.Join("\n", actualText)).IsEqualTo(string.Join("\n", expectedText));
    }

    private async Task ShouldBeAsync(DataTable table, IEnumerable<DishLine> actual)
    {
        var booking = Recall<GroupBooking>();
        var expected = table.Rows.Select(r => new DishLine(booking.Dish(r["dish"]).Name, int.Parse(r["quantity"], System.Globalization.CultureInfo.InvariantCulture)));
        await Assert.That(Describe(actual)).IsEqualTo(Describe(expected));
    }

    private static string Describe(IEnumerable<DishLine> dishes) =>
        string.Join(", ", dishes.OrderBy(d => d.Dish, StringComparer.Ordinal));

    private async Task<ParticipantView> GuestInSnapshotAsync(string name)
    {
        var guests = await Actor.AsksForAsync(new TheGuestsInTheSnapshot());
        return guests.SingleOrDefault(g => g.Name == name)
            ?? throw new InvalidOperationException($"{name} is not in the list: {Actor.TheLastResponse().Body}");
    }
}
