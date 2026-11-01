using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

/// <summary>The owner's group pre-order screen: /reservations/{reservationId}/group-order.</summary>
public sealed class GroupPreOrderPage(IPage page) : BasePage(page)
{
    public ToastComponent Toast { get; } = new(page);

    public ILocator Heading => Page.GetByRole(AriaRole.Heading, new() { Level = 1, Name = "Group Pre-Order" });
    public ILocator StatusBadge => Page.Locator(".page-header .badge");
    public ILocator ParticipantCards => Page.GetByRole(AriaRole.List, new() { Name = "Participants" }).Locator("article");
    public ILocator ProblemTitle => Page.Locator(".state--problem .state-title");
    public ILocator NothingAddedYet => Page.GetByText("Participants haven't added any dishes yet");
    public ILocator PendingNotice => Page.Locator("p.notice");
    public ILocator ConsolidatedItems => Page.Locator(".consolidated-list .consolidated-item");
    public ILocator GrandTotal => Page.Locator(".grand-total-value");
    public ILocator RefreshButton => Page.GetByRole(AriaRole.Button, new() { Name = "Refresh", Exact = true });
    public ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new() { Name = "Submit Pre-Order", Exact = true });
    public ILocator Dialog => Page.GetByRole(AriaRole.Dialog);
    public ILocator ConfirmSubmitButton => Dialog.GetByRole(AriaRole.Button, new() { Name = "Submit", Exact = true });
    public ILocator ConfirmRemoveButton => Dialog.GetByRole(AriaRole.Button, new() { Name = "Remove Guest", Exact = true });
    public ILocator RemoveButtons => Page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^Remove ") });

    /// <summary>The screen has finished loading when it shows either the participants or a problem.</summary>
    public ILocator LoadedState => ParticipantCards.First.Or(ProblemTitle).Or(NothingAddedYet);

    public ILocator RemoveButtonFor(string guestName) =>
        Page.GetByRole(AriaRole.Button, new() { Name = $"Remove {guestName}", Exact = true });

    public ParticipantCardComponent Card(int index) => new(ParticipantCards.Nth(index));

    public Task NavigateAsync(string reservationId) => NavigateToAsync($"/reservations/{reservationId}/group-order");
}
