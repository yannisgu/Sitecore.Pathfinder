﻿namespace Sitecore.Pathfinder.Parsing.Items.TreeNodeParsers
{
  using Sitecore.Pathfinder.Diagnostics;
  using Sitecore.Pathfinder.TextDocuments;

  public interface ITextNodeParser
  {
    bool CanParse([NotNull] ItemParseContext context, [NotNull] ITextNode textNode);

    void Parse([NotNull] ItemParseContext context, [NotNull] ITextNode textNode);
  }
}