using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

/// <summary>The waiter's reservation list: /waiter-reservations.</summary>
public sealed class WaiterReservationsPage(IPage page) : BasePage(page)
{
    public ILocator ReservationCards => Page.Locator("app-card-reservation li.card");

    public ILocator ReservationOn(string date) => ReservationCards.Filter(new() { HasText = date });

    public static ILocator PreOrderButton(ILocator card) => card.GetByRole(AriaRole.Button, new() { Name = "Pre-order", Exact = true });

    public Task NavigateAsync() => NavigateToAsync("/waiter-reservations");
}
