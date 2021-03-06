// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Snapshots;

namespace Sitecore.Pathfinder.Projects.Templates
{
    public class TemplateSection : TextNodeSourcePropertyBag
    {
        public TemplateSection([NotNull] Template template, Guid guid)
        {
            Template = template;

            IconProperty = NewSourceProperty("Icon", string.Empty);
            SectionNameProperty = NewSourceProperty("Name", string.Empty);
            SortorderProperty = NewSourceProperty("Sortorder", 0);
            Fields = new LockableList<TemplateField>(this);

            Uri = new ProjectItemUri(template.DatabaseName, guid);
        }

        [NotNull]
        public SourceProperty<int> SortorderProperty { get;  }

        [NotNull, ItemNotNull]
        public ICollection<TemplateField> Fields { get; }

        [NotNull]
        public string Icon
        {
            get { return IconProperty.GetValue(); }
            set { IconProperty.SetValue(value); }
        }
        public int Sortorder
{
            get { return SortorderProperty.GetValue(); }
            set { SortorderProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> IconProperty { get; }

        public override Locking Locking => Template.Locking;

        [NotNull]
        public string SectionName
        {
            get { return SectionNameProperty.GetValue(); }
            set { SectionNameProperty.SetValue(value); }
        }

        [NotNull]
        public SourceProperty<string> SectionNameProperty { get; }

        [NotNull]
        public Template Template { get; }

        [NotNull]
        public IProjectItemUri Uri { get; private set; }

        public void Merge([NotNull] TemplateSection newSection, bool overwrite)
        {
            if (!string.IsNullOrEmpty(newSection.Icon))
            {
                IconProperty.SetValue(newSection.IconProperty);
            }

            if (newSection.Sortorder != 0)
            {
                SortorderProperty.SetValue(newSection.SortorderProperty);
            }

            foreach (var newField in newSection.Fields)
            {
                var field = Fields.FirstOrDefault(f => string.Equals(f.FieldName, newField.FieldName, StringComparison.OrdinalIgnoreCase));
                if (field == null)
                {
                    Fields.Add(newField);
                    continue;
                }

                field.Merge(newField, overwrite);
            }
        }

        [NotNull]
        public TemplateSection With([NotNull] ITextNode sourceTextNode)
        {
            WithSourceTextNode(sourceTextNode);
            return this;
        }
    }
}
