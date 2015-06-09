﻿namespace Sitecore.Pathfinder.Parsing.Files
{
  using System;
  using System.ComponentModel.Composition;

  [Export(typeof(IParser))]
  public class BinFileParser : ParserBase
  {
    private const string FileExtension = ".dll";

    public BinFileParser() : base(Constants.Parsers.BinFiles)
    {
    }

    public override bool CanParse(IParseContext context)
    {
      return context.Snapshot.SourceFile.FileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase);
    }

    public override void Parse(IParseContext context)
    {
      var binFile = context.Factory.BinFile(context.Project, context.Snapshot);
      context.Project.AddOrMerge(binFile);
    }
  }
}
