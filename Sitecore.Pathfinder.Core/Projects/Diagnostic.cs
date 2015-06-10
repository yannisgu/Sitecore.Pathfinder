﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Documents;

namespace Sitecore.Pathfinder.Projects
{
    public class Diagnostic
    {
        public Diagnostic([NotNull] string fileName, TextPosition position, Severity severity, [NotNull] string text)
        {
            FileName = fileName;
            Position = position;
            Severity = severity;
            Text = text;
        }

        [NotNull]
        public string FileName { get; }

        public TextPosition Position { get; }

        public Severity Severity { get; }

        [NotNull]
        public string Text { get; }
    }
}