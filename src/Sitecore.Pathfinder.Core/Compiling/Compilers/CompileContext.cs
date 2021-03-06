﻿// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sitecore.Pathfinder.Configuration;
using Sitecore.Pathfinder.Configuration.ConfigurationModel;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensibility.Pipelines;
using Sitecore.Pathfinder.Parsing.References;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Compiling.Compilers
{
    [Export(typeof(ICompileContext))]
    public class CompileContext : ICompileContext
    {
        [ImportingConstructor]
        public CompileContext([NotNull] IConfiguration configuration, [NotNull] ICompositionService compositionService, [NotNull] IFactoryService factory, [NotNull] IPipelineService pipelines, [NotNull] ITraceService trace, [NotNull] IReferenceParserService referenceParser, [ImportMany, NotNull, ItemNotNull] IEnumerable<ICompiler> compilers)
        {
            Configuration = configuration;
            CompositionService = compositionService;
            Factory = factory;
            Pipelines = pipelines;
            Trace = trace;
            ReferenceParser = referenceParser;
            Compilers = compilers;
        }

        public IEnumerable<ICompiler> Compilers { get; }

        public ICompositionService CompositionService { get; }

        public IConfiguration Configuration { get; }

        public IFactoryService Factory { get; }

        public IPipelineService Pipelines { get; }

        public IProject Project { get; private set; }

        public IReferenceParserService ReferenceParser { get; }

        public ITraceService Trace { get; }

        public ICompileContext With(IProject project)
        {
            Project = project;
            return this;
        }
    }
}
