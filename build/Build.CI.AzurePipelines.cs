using Nuke.Common.CI.AzurePipelines;

[AzurePipelines(
    suffix: "PR",
    AzurePipelinesImage.UbuntuLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    TriggerBatch = true,
    PullRequestsBranchesInclude = new[] { "main" },
    ImportVariableGroups = new[] { "dotRMDY-MyGet" },
    ImportSecrets = new[] { nameof(MyGetUsername), nameof(MyGetApiKey) },
    InvokedTargets = new[] { nameof(Pack) },
    CacheKeyFiles = new string[0],
    CachePaths = new string[0])]
[AzurePipelines(
    suffix: "Publish",
    AzurePipelinesImage.UbuntuLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    TriggerBatch = true,
    TriggerTagsInclude = new[] { "'*.*.*'" },
    ImportVariableGroups = new[] { "dotRMDY-MyGet" },
    ImportSecrets = new[] { nameof(MyGetUsername), nameof(MyGetApiKey) },
    InvokedTargets = new[] { nameof(Publish) },
    CacheKeyFiles = new string[0],
    CachePaths = new string[0])]
partial class Build
{
}