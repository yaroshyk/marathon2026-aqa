using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>Business-level outcomes shared by several features ("the visitor is told ...", "could not be
/// found"). How an outcome shows up on the wire — which status code, which error body — is decided here,
/// so feature files never mention it.</summary>
[Binding]
public sealed class OutcomeStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    [Then("the visitor is told {string}")]
    [Scope(Tag = "api")]
    public async Task ThenVisitorIsTold(string message)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());

        await Assert.That(outcome.Succeeded).IsFalse().Because($"the request unexpectedly succeeded: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).IsEqualTo(message);
    }

    [Then("sign-in is refused")]
    public async Task ThenSignInIsRefused()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(401).Because($"response body was: {outcome.Body}");
    }

    [Then("the guest is told that the {item} could not be found")]
    public async Task ThenGuestIsToldNotFound(string item)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());

        await Assert.That(outcome.Status).IsEqualTo(404).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).Contains("was not found");
    }

    [Then("the location is not created")]
    public async Task ThenLocationIsNotCreated()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsGreaterThanOrEqualTo(400).Because($"the location was created: {outcome.Body}");
    }

    [Then("the information is available")]
    [Then("the menu is shown")]
    public async Task ThenInformationIsAvailable()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"response status {outcome.Status}, body: {outcome.Body}");
    }

    // ---- group pre-order: what a refusal means in protocol terms ----

    [Then("they are refused as they are not the owner")]
    public async Task ThenTheyAreRefusedAsNotTheOwner()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(403).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).IsEqualTo("Only the reservation owner can perform this action.");
    }

    [Then("they are asked to sign in")]
    public async Task ThenTheyAreAskedToSignIn()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(401).Because($"response body was: {outcome.Body}");
    }

    [Then("the owner is told that the group reservation was not found")]
    public async Task ThenTheOwnerIsToldTheGroupWasNotFound()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(404).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).IsEqualTo("Group reservation was not found");
    }

    [Then("the owner is told that the guest was not found")]
    public async Task ThenTheOwnerIsToldTheGuestWasNotFound()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(404).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).EndsWith("not found.");
    }

    [Then("the owner is told that their own pre-order for the reservation was not found")]
    public async Task ThenTheOwnerIsToldTheOrderWasNotFound()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(400).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).StartsWith("Order for reservation").And.EndsWith("not found.");
    }

    [Then("the owner is told {string}")]
    public async Task ThenTheOwnerIsTold(string message)
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Status).IsEqualTo(400).Because($"response body was: {outcome.Body}");
        await Assert.That(outcome.ErrorMessage).IsEqualTo(message);
    }

    [Then("the owner is told that the pre-order was sent to the restaurant")]
    [Scope(Tag = "api")]
    public async Task ThenTheServiceAcceptsTheSubmit()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());
        await Assert.That(outcome.Succeeded).IsTrue().Because($"response status {outcome.Status}, body: {outcome.Body}");
    }
}
