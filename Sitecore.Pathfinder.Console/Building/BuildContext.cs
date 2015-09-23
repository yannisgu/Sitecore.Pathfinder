﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Framework.ConfigurationModel;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensibility.Pipelines;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.IO;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Building
{
    [Export(typeof(IBuildContext))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildContext : IBuildContext
    {
        [CanBeNull]
        private IProject _project;

        [ImportingConstructor]
        public BuildContext([NotNull] ICompositionService compositionService, [NotNull] IConfiguration configuration, [NotNull] ITraceService traceService, [NotNull] IFileSystemService fileSystem, [NotNull] IPipelineService pipelineService, [NotNull] IProjectService projectService)
        {
            CompositionService = compositionService;
            Configuration = configuration;
            Trace = traceService;
            FileSystem = fileSystem;
            PipelineService = pipelineService;
            ProjectService = projectService;
        }

        public ICompositionService CompositionService { get; }

        public IConfiguration Configuration { get; }

        public bool DisplayDoneMessage { get; set; } = true;

        public IFileSystemService FileSystem { get; }

        public bool IsAborted { get; set; }

        public IList<IProjectItem> ModifiedProjectItems { get; } = new List<IProjectItem>();

        public IList<string> OutputFiles { get; } = new List<string>();

        public IPipelineService PipelineService { get; }

        public IProject Project => _project ?? (_project = ProjectService.LoadProjectFromConfiguration());

        public string SolutionDirectory => Configuration.GetString(Constants.Configuration.SolutionDirectory);

        public ITraceService Trace { get; }

        [NotNull]
        protected IProjectService ProjectService { get; }
    }
}
