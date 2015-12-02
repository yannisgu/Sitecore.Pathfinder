// � 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Projects.Items;

namespace Sitecore.Pathfinder.XPath
{
    public class Descendant : Step
    {
        public Descendant([NotNull] ElementBase element)
        {
            Name = element.Name;

            var itemElement = element as ItemElement;
            if (itemElement != null)
            {
                Predicate = itemElement.Predicate;
            }
            else if (element is FieldElement)
            {
            }
            else
            {
                throw new ArgumentException("Node or Field type expected", nameof(element));
            }
        }

        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public Predicate Predicate { get; }

        public override object Evaluate(Query query, object context)
        {
            object result = null;

            /*
      if (MainUtil.IsID(m_name)) {
        Sitecore.Data.Items.QueryContext item = node.Database.Items[m_name];

        if (item != null) {
          if (node.ID == ID.Empty || item.Paths.UniquePath.IndexOf(node.ID.ToString()) >= 0) {
            TestNode(query, item, ref result);
          }
        }
      }
      else {
      */
            Iterate(query, context, ref result);

            //}

            return result;
        }

        protected void Iterate([NotNull] Query query, [CanBeNull] object context, [CanBeNull] ref object result)
        {
            var item = context as Item;
            if (item == null)
            {
                return;
            }

            foreach (var child in item.GetChildren())
            {
                TestNode(query, child, ref result);

                if (Break(query, result))
                {
                    break;
                }

                Iterate(query, child, ref result);

                if (Break(query, result))
                {
                    break;
                }
            }
        }

        protected void TestNode([NotNull] Query query, [CanBeNull] object context, [CanBeNull] ref object result)
        {
            if (Name == "*" || string.Equals((context as Item)?.ItemName, Name, StringComparison.OrdinalIgnoreCase))
            {
                Process(query, context, Predicate, NextStep, ref result);
            }
        }
    }
}
