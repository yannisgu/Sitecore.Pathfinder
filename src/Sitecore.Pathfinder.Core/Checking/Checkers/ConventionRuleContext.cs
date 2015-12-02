﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Rules.Contexts;

namespace Sitecore.Pathfinder.Checking.Checkers
{
    public class ConventionRuleContext : IRuleContext
    {
        public ConventionRuleContext([NotNull] IProjectItem projectItem)
        {
            Objects = new[]
            {
                projectItem
            };
        }

        public bool IsAborted { get; set; }

        public IEnumerable<object> Objects { get; }
    }
}
