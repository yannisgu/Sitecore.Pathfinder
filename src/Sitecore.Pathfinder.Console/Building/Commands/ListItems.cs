// � 2015 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel.Composition;
using System.Linq;
using Sitecore.Pathfinder.Projects.Items;

namespace Sitecore.Pathfinder.Building.Commands
{
    [Export(typeof(ITask))]
    public class ListItems : TaskBase
    {
        public ListItems() : base("list-items")
        {
        }

        public override void Run(IBuildContext context)
        {
            foreach (var item in context.Project.Items.OfType<ItemBase>().Where(i => !i.IsExternalReference).OrderBy(i => i.ItemIdOrPath))
            {
                context.Trace.Writeline(item.ItemIdOrPath);
            }

            context.DisplayDoneMessage = false;
        }

        public override void WriteHelp(HelpWriter helpWriter)
        {
            helpWriter.Summary.Write("Lists the Sitecore items in the project.");
        }
    }
}