using FinalSuspect.Attributes;

namespace FinalSuspect.Modules.Core.Plugin;

public class LaunchingInfo
{
    [PluginModuleInitializer(InitializePriority.Low)]
    public static void OnInitialization()
    {
        Info($"{Application.version}", "AmongUs Version");

        var handler = Handler("GitVersion");
        handler.Info($"{nameof(GitBaseTag)}: {GitBaseTag}");
        handler.Info($"{nameof(GitCommit)}: {GitCommit}");
        handler.Info($"{nameof(GitCommits)}: {GitCommits}");
        handler.Info($"{nameof(GitIsDirty)}: {GitIsDirty}");
        handler.Info($"{nameof(GitSha)}: {GitSha}");
        handler.Info($"{nameof(GitTag)}: {GitTag}");
    }

#pragma warning disable CS0618 // 类型或成员已过时
    public const string GitBaseTag = ThisAssembly.Git.BaseTag;
    public const string GitCommit = ThisAssembly.Git.Commit;
    public const string GitCommits = ThisAssembly.Git.Commits;
    public const string GitBranch = ThisAssembly.Git.Branch;
    public const bool GitIsDirty = ThisAssembly.Git.IsDirty;
    public const string GitSha = ThisAssembly.Git.Sha;
    public const string GitTag = ThisAssembly.Git.Tag;
#pragma warning restore CS0618
}