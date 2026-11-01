using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupPreOrders;

/// <summary>A persona signs in on the storefront's login form; whoever was signed in before is signed out first.</summary>
public sealed class SignInOnTheStorefront(Persona who) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = actor.LookingAtThePage();
        await page.Context.ClearCookiesAsync();
        if (page.Url.StartsWith("http", StringComparison.Ordinal))
            await page.EvaluateAsync("() => localStorage.clear()");

        var (username, password) = TheTokenOf.CredentialsOf(who);
        await actor.AttemptsToAsync(new SignInThroughTheLoginForm(username, password));
        // The app leaves the login page before it has loaded and stored the signed-in user's profile; navigating
        // away earlier cancels that request and the next page reports an expired session.
        await page.WaitForFunctionAsync("() => localStorage.getItem('user') !== null");
        actor.Remember(new StorefrontUser(who));
    }
}

/// <summary>Makes sure the given persona is the one signed in on the storefront, signing in only when needed.</summary>
public sealed class BeSignedInOnTheStorefrontAs(Persona who) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        if (actor.TryRecall<StorefrontUser>(out var user) && user.Persona == who)
            return;

        await actor.AttemptsToAsync(new SignInOnTheStorefront(who));
    }
}

/// <summary>The signed-in persona opens the group pre-order screen of the owner's reservation (or of one that does not exist).</summary>
public sealed class OpenTheGroupPreOrder(GroupTarget target = GroupTarget.TheOwnersGroup) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        var reservationId = target == GroupTarget.TheOwnersGroup ? actor.Recall<GroupBooking>().ReservationId : Targets.UnknownId;
        await page.NavigateAsync(reservationId);
        await page.LoadedState.First.WaitForAsync();
    }
}

/// <summary>The owner asks the screen to sync now and waits for the fresh list of participants.</summary>
public sealed class RefreshTheGroupPreOrder : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await actor.LookingAtThePage().RunAndWaitForResponseAsync(
            () => page.RefreshButton.ClickAsync(),
            response => response.Url.Contains("/sessions", StringComparison.Ordinal) && response.Request.Method == "GET");
        await Assertions.Expect(page.RefreshButton).ToBeEnabledAsync();
    }
}

/// <summary>The owner submits the group pre-order on the screen and confirms it in the dialog.</summary>
public sealed class SubmitTheGroupPreOrderOnTheScreen : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.SubmitButton.ClickAsync();
        await actor.LookingAtThePage().RunAndWaitForResponseAsync(
            () => page.ConfirmSubmitButton.ClickAsync(),
            response => response.Url.Contains("/submit", StringComparison.Ordinal));
        await page.Dialog.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }
}

/// <summary>The owner removes a guest on the screen and confirms it in the dialog.</summary>
public sealed class RemoveTheGuestOnTheScreen(string guestName) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.RemoveButtonFor(guestName).ClickAsync();
        var response = await actor.LookingAtThePage().RunAndWaitForResponseAsync(
            () => page.ConfirmRemoveButton.ClickAsync(),
            r => r.Request.Method == "DELETE" && r.Url.Contains("/sessions/", StringComparison.Ordinal));
        await page.Dialog.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

        if (response.Ok)
            actor.Recall<JoinedGuest>(guestName).Removed = true;

        // Close the "… was removed" toast, so it is not mistaken for the answer to the owner's next action.
        var close = actor.LookingAtThePage().Locator(".toast-success .toast-close");
        if (await close.IsVisibleAsync())
            await close.ClickAsync();
        await page.Toast.Success.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }
}

/// <summary>The waiter opens the reservation from their list and goes to its pre-order.</summary>
public sealed class OpenTheReservationsPreOrderAsWaiter : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = actor.LookingAtThePage();
        var reservations = new WaiterReservationsPage(page);
        await reservations.NavigateAsync();

        var date = actor.Recall<GroupBooking>().Reservation.StartTime.ToUniversalTime()
            .ToString("MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
        var card = reservations.ReservationOn(date).First;
        // The list loads the pre-order, feedback and customer of every reservation first, which takes a while.
        var preOrder = WaiterReservationsPage.PreOrderButton(card);
        await preOrder.WaitForAsync(new() { Timeout = 90_000 });
        await preOrder.ClickAsync();
        await page.WaitForURLAsync(url => url.Contains("/waiter-pre-order", StringComparison.Ordinal));
        await new WaiterPreOrderPage(page).Title.WaitForAsync();
    }
}
