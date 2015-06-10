// � 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using Sitecore.Pathfinder.Building.Querying;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.IO;
using Sitecore.Pathfinder.Querying;

namespace Sitecore.Pathfinder.Building.Refactoring
{
    [Export(typeof(ITask))]
    public class Rename : QueryTaskBase
    {
        public Rename() : base("rename")
        {
        }

        public override void Run(IBuildContext context)
        {
            context.DisplayDoneMessage = false;

            var qualifiedName = context.Configuration.GetString("name").Trim();
            if (string.IsNullOrEmpty(qualifiedName))
            {
                context.Trace.Writeline(Texts.You_must_specific_the___name_argument);
                return;
            }

            var newShortName = context.Configuration.GetString("to").Trim();
            if (string.IsNullOrEmpty(newShortName))
            {
                context.Trace.Writeline(Texts.You_must_specific_the___to_argument);
                return;
            }

            var projectItem = context.Project.Items.FirstOrDefault(i => string.Compare(i.QualifiedName, qualifiedName, StringComparison.OrdinalIgnoreCase) == 0);
            if (projectItem == null)
            {
                context.Trace.Writeline(Texts.Item_not_found, qualifiedName);
                return;
            }

            projectItem.Rename(newShortName);

            var queryService = context.CompositionService.Resolve<IQueryService>();
            var references = queryService.FindUsages(context.Project, qualifiedName);

            foreach (var reference in references)
            {
                if (reference.SourceTextNode == null)
                {
                    continue;
                }

                var value = reference.SourceTextNode.Value;
                if (PathHelper.IsQualifiedName(value))
                {
                    var n = value.LastIndexOf('/');
                    value = n < 0 ? newShortName : value.Left(n + 1) + newShortName;
                }
                else
                {
                    value = newShortName;
                }

                reference.SourceTextNode.SetValue(value);
            }

            context.Project.SaveChanges();
        }
    }
}