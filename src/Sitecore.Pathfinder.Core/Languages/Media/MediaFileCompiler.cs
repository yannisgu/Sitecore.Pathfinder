﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Pathfinder.Compiling.Compilers;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Text;

namespace Sitecore.Pathfinder.Languages.Media
{
    public class MediaFileCompiler : CompilerBase
    {
        public MediaFileCompiler() : base(1000)
        {
        }

        public override bool CanCompile(ICompileContext context, IProjectItem projectItem)
        {
            return projectItem is MediaFile;
        }

        public override void Compile(ICompileContext context, IProjectItem projectItem)
        {
            var mediaFile = projectItem as MediaFile;
            Assert.Cast(mediaFile, nameof(mediaFile));

            var extension = Path.GetExtension(mediaFile.Snapshot.SourceFile.AbsoluteFileName).TrimStart('.').ToLowerInvariant();

            var templateIdOrPath = context.Configuration.GetString(Constants.Configuration.BuildProject.MediaTemplate + ":" + extension, "/sitecore/templates/System/Media/Unversioned/File");

            var project = context.Project;
            var snapshot = mediaFile.Snapshot;

            var guid = StringHelper.GetGuid(project, mediaFile.ItemPath);

            var item = context.Factory.Item(project, guid, mediaFile.DatabaseName, mediaFile.ItemName, mediaFile.ItemPath, string.Empty).With(new SnapshotTextNode(snapshot));
            item.IsEmittable = false;
            item.OverwriteWhenMerging = true;
            item.MergingMatch = MergingMatch.MatchUsingSourceFile;

            item.ItemNameProperty.AddSourceTextNode(new FileNameTextNode(mediaFile.ItemName, snapshot));
            item.TemplateIdOrPathProperty.SetValue(templateIdOrPath);

            var addedItem = project.AddOrMerge(item);
            mediaFile.MediaItemUri = addedItem.Uri;
        }
    }
}
