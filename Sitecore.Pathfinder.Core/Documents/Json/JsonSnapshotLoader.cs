﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel.Composition;
using System.IO;
using Sitecore.Pathfinder.Projects;

namespace Sitecore.Pathfinder.Documents.Json
{
    [Export(typeof(ISnapshotLoader))]
    public class JsonSnapshotLoader : ISnapshotLoader
    {
        public JsonSnapshotLoader()
        {
            Priority = 1000;
        }

        public double Priority { get; }

        public virtual bool CanLoad(ISnapshotService snapshotService, IProject project, ISourceFile sourceFile)
        {
            return string.Compare(Path.GetExtension(sourceFile.FileName), ".json", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public virtual ISnapshot Load(ISnapshotService snapshotService, IProject project, ISourceFile sourceFile)
        {
            var contents = sourceFile.ReadAsText();

            contents = snapshotService.ReplaceTokens(project, sourceFile, contents);

            return new JsonTextSnapshot(sourceFile, contents);
        }
    }
}