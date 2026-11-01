using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>One participant's card on the group pre-order screen: title, cart status, dishes and subtotal.</summary>
public sealed class ParticipantCardComponent(ILocator root)
{
    public ILocator Title => root.GetByRole(AriaRole.Heading, new() { Level = 2 });
    public ILocator CartStatus => root.Locator(".chip");
    public ILocator Items => root.Locator("li.item");
    public ILocator NoDishes => root.Locator(".empty");
    public ILocator Subtotal => root.Locator(".subtotal span").Last;

    public ILocator ItemName(int index) => Items.Nth(index).Locator(".name");
    public ILocator ItemQuantity(int index) => Items.Nth(index).Locator(".qty");
}
