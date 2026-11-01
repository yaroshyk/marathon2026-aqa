using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Screenplay.GroupPreOrders;
using Restaurant_Aqa_Framework.Support;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions.GroupPreOrders;

/// <summary>Every guest card on the owner's screen (the owner's own "My Pre-Order" card left out).</summary>
public sealed class TheGuestsOnTheScreen : IQuestion<IReadOnlyList<ParticipantView>>
{
    public async Task<IReadOnlyList<ParticipantView>> AnsweredByAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.ParticipantCards.First.WaitForAsync();

        var guests = new List<ParticipantView>();
        for (var i = 0; i < await page.ParticipantCards.CountAsync(); i++)
        {
            var card = page.Card(i);
            var title = (await card.Title.InnerTextAsync()).Trim();
            if (!title.StartsWith("Guest:", StringComparison.Ordinal))
                continue;

            var dishes = new List<DishLine>();
            for (var j = 0; j < await card.Items.CountAsync(); j++)
                dishes.Add(new DishLine(
                    (await card.ItemName(j).InnerTextAsync()).Trim(),
                    Money.FirstNumber(await card.ItemQuantity(j).InnerTextAsync())));

            guests.Add(new ParticipantView(
                title["Guest:".Length..].Trim(),
                dishes,
                Money.Parse(await card.Subtotal.InnerTextAsync()),
                (await card.CartStatus.InnerTextAsync()).Trim() == "Submitted"));
        }

        return guests;
    }
}

/// <summary>Everything outside the cards: status badge, grand total, consolidated dishes, notice and what the owner can still do.</summary>
public sealed class TheGroupPreOrderSummaryOnTheScreen : IQuestion<GroupPreOrderSummary>
{
    public async Task<GroupPreOrderSummary> AnsweredByAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.GrandTotal.WaitForAsync();

        var consolidated = new List<DishLine>();
        foreach (var item in await page.ConsolidatedItems.AllAsync())
        {
            var label = (await item.Locator("span").First.InnerTextAsync()).Trim();
            var cut = label.LastIndexOf('×');
            consolidated.Add(new DishLine(label[..cut].Trim(), Money.FirstNumber(label[cut..])));
        }

        return new GroupPreOrderSummary(
            (await page.StatusBadge.InnerTextAsync()).Trim(),
            Money.Parse(await page.GrandTotal.InnerTextAsync()),
            consolidated.OrderBy(d => d.Dish, StringComparer.Ordinal).ToList(),
            await page.PendingNotice.IsVisibleAsync() ? (await page.PendingNotice.InnerTextAsync()).Trim() : null,
            await page.SubmitButton.IsVisibleAsync() && await page.SubmitButton.IsEnabledAsync(),
            await page.RemoveButtons.CountAsync());
    }
}

/// <summary>Whether the screen says that nobody has added any dishes yet.</summary>
public sealed class TheEmptyGroupMessageIsShown : IQuestion<bool>
{
    public async Task<bool> AnsweredByAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.LoadedState.First.WaitForAsync();
        return await page.NothingAddedYet.IsVisibleAsync();
    }
}

/// <summary>The problem the screen reports instead of the participants ("Group reservation not found", …), or null.</summary>
public sealed class TheGroupPreOrderProblem : IQuestion<string?>
{
    public async Task<string?> AnsweredByAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.LoadedState.First.WaitForAsync();
        return await page.ProblemTitle.IsVisibleAsync() ? (await page.ProblemTitle.InnerTextAsync()).Trim() : null;
    }
}

/// <summary>The message of the toast that pops up (success or error).</summary>
public sealed class TheToast(bool error) : IQuestion<string>
{
    public async Task<string> AnsweredByAsync(Actor actor)
    {
        var toast = new GroupPreOrderPage(actor.LookingAtThePage()).Toast;
        var messages = error ? toast.Error : toast.Success;
        await messages.First.WaitForAsync();
        // A toast of an earlier action may still be fading out: the newest one is the last.
        return (await messages.Last.InnerTextAsync()).Trim();
    }
}

/// <summary>The dishes and quantities on the waiter's pre-order screen.</summary>
public sealed class TheDishesTheWaiterSees : IQuestion<IReadOnlyList<DishLine>>
{
    public async Task<IReadOnlyList<DishLine>> AnsweredByAsync(Actor actor)
    {
        var page = new WaiterPreOrderPage(actor.LookingAtThePage());
        await page.DishCards.First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var dishes = new List<DishLine>();
        for (var i = 0; i < await page.DishCards.CountAsync(); i++)
            dishes.Add(new DishLine(
                (await page.DishName(i).InnerTextAsync()).Trim(),
                Money.FirstNumber(await page.DishQuantity(i).InnerTextAsync())));

        return dishes.OrderBy(d => d.Dish, StringComparer.Ordinal).ToList();
    }
}

/// <summary>How many guest cards the owner's screen shows, once it has loaded (zero when it shows a problem instead).</summary>
public sealed class TheNumberOfGuestsOnTheScreen : IQuestion<int>
{
    public async Task<int> AnsweredByAsync(Actor actor)
    {
        var page = new GroupPreOrderPage(actor.LookingAtThePage());
        await page.LoadedState.First.WaitForAsync();
        return await page.ParticipantCards
            .Filter(new() { Has = actor.LookingAtThePage().GetByRole(AriaRole.Heading, new() { NameRegex = new System.Text.RegularExpressions.Regex("^Guest:") }) })
            .CountAsync();
    }
}
