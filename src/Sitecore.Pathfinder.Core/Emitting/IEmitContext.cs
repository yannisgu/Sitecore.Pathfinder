﻿// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Pathfinder.Configuration.ConfigurationModel;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.IO;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Emitting
{
    public interface IEmitContext
    {
        [NotNull]
        IConfiguration Configuration { get; }

        [NotNull]
        IFileSystemService FileSystem { get; }

        bool ForceUpdate { get; }

        [NotNull]
        IProjectBase Project { get; }

        [NotNull]
        IProjectEmitter ProjectEmitter { get; }

        [NotNull]
        ITraceService Trace { get; }

        [NotNull]
        IEmitContext With([NotNull] IProjectEmitter projectEmitter, [NotNull] IProjectBase project);
    }
}
