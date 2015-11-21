﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Pathfinder.Diagnostics;

namespace Sitecore.Pathfinder.Projects.Items
{
    public class FieldCollection : List<Field>
    {
        [NotNull]
        private readonly Item _item;

        public FieldCollection([NotNull] Item item)
        {
            _item = item;
        }

        [CanBeNull]
        public Field this[[NotNull] string fieldName]
        {
            get { return this.FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.OrdinalIgnoreCase)); }
        }
    }
}