using ReportPortal.Client.Abstractions.Models;
using ReportPortal.Client.Abstractions.Responses;
using ReportPortal.ReqnrollPlugin;

namespace Restaurant_Aqa_Framework.Hooks;

/// <summary>
/// Report Portal agent tuning (the agent itself is enabled by RP__ENABLED and configured by RP__SERVER__* variables).
/// The agent reports a feature as skipped when the run is filtered to some of its scenarios, and a skipped feature
/// marks the whole launch failed. A feature is reported as passed instead: a failed scenario inside it still fails the
/// launch, because Report Portal counts every scenario on its own.
/// </summary>
[Binding]
public static class ReportPortalHooks
{
    [BeforeTestRun(Order = -1000)]
    public static void DoNotFailTheLaunchForAFilteredFeature() =>
        ReportPortalAddin.BeforeFeatureFinished += (_, e) =>
        {
            if (e.FinishTestItemRequest.Status == Status.Skipped)
                e.FinishTestItemRequest.Status = Status.Passed;
        };

    /// <summary>A scenario skipped on purpose (Skip.Test, with its reason) is reported as skipped, not failed.</summary>
    [BeforeTestRun(Order = -1000)]
    public static void ReportSkippedScenariosAsSkipped() =>
        ReportPortalAddin.BeforeScenarioFinished += (_, e) =>
        {
            if (e.ScenarioContext?.TestError is TUnit.Core.Exceptions.SkipTestException)
            {
                e.FinishTestItemRequest.Status = Status.Skipped;
                e.FinishTestItemRequest.Issue = new Issue { Type = WellKnownIssueType.NotDefect, Comment = e.ScenarioContext.TestError.Message };
            }
        };
}
