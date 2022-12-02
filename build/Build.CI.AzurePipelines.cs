using Nuke.Common.CI.AzurePipelines;

[AzurePipelines(
    suffix: "PR",
    AzurePipelinesImage.UbuntuLatest,
    AutoGenerate = true,
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
    AutoGenerate = true,
    FetchDepth = 0,
    TriggerBatch = true,
    TriggerTagsInclude = new[] { "*.*.*" },
    InvokedTargets = new[] { nameof(Compile), nameof(Publish) },
    NonEntryTargets = new[] { nameof(Clean), nameof(Restore) },
    CacheKeyFiles = new string[0],
    CachePaths = new string[0])]
public partial class Build
{
}