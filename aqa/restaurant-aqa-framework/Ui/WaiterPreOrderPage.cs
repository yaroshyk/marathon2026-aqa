using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

/// <summary>The waiter's view of one reservation's pre-order: /waiter-pre-order.</summary>
public sealed class WaiterPreOrderPage(IPage page) : BasePage(page)
{
    public ILocator Title => Page.Locator("h2.title");
    public ILocator DishCards => Page.Locator(".preorder-list app-dish-card");

    public ILocator DishName(int index) => DishCards.Nth(index).Locator(".dish-name");
    public ILocator DishQuantity(int index) => DishCards.Nth(index).Locator(".pre-order-quantity");
}
