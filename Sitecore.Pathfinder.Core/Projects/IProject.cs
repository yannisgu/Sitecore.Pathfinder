﻿namespace Sitecore.Pathfinder.Projects
{
  using System.Collections.Generic;
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.IO;
  using Sitecore.Pathfinder.TextDocuments;

  public interface IProject
  {
    [NotNull]
    string DatabaseName { get; set; }

    [NotNull]
    IFileSystemService FileSystem { get; }

    bool HasErrors { get; }

    [NotNull]
    IEnumerable<IProjectItem> Items { get; }

    [NotNull]
    ICollection<ProjectMessage> Messages { get; }

    [NotNull]
    string ProjectDirectory { get; }

    [NotNull]
    string ProjectUniqueId { get; }

    [NotNull]
    ICollection<ISourceFile> SourceFiles { get; }

    void Add([NotNull] string sourceFileName);

    T AddOrMerge<T>([NotNull] T projectItem) where T : IProjectItem;

    void Remove([NotNull] IProjectItem projectItem);

    void Remove([NotNull] string sourceFileName);
  }
}
