using Nuke.Common.CI.AzurePipelines;

[AzurePipelines(
    suffix: "PR",
    AzurePipelinesImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    TriggerBatch = true,
    PullRequestsBranchesInclude = new[] { "main" },
    InvokedTargets = new[] { nameof(Compile), nameof(Pack) },
    NonEntryTargets = new[] { nameof(Clean), nameof(Restore) },
    CacheKeyFiles = new string[0],
    CachePaths = new string[0])]
[AzurePipelines(
    suffix: "Publish",
    AzurePipelinesImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    TriggerBatch = true,
    TriggerTagsInclude = new[] { "'*.*.*'" },
    ImportVariableGroups = new[] { "MyGet-RMDY-Mobile-dotnet-localizer" },
    ImportSecrets = new[] { nameof(MyGetFeedUrl), nameof(MyGetApiKey) },
    InvokedTargets = new[] { nameof(Publish) },
    CacheKeyFiles = new string[0],
    CachePaths = new string[0])]
public partial class Build
{
}