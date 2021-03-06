// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Sitecore.Pathfinder.Configuration;
using Sitecore.Pathfinder.Configuration.ConfigurationModel;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Extensibility.Pipelines;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pathfinder.IO;
using Sitecore.Pathfinder.Parsing.Pipelines.ReferenceParserPipelines;
using Sitecore.Pathfinder.Projects;
using Sitecore.Pathfinder.Projects.Items;
using Sitecore.Pathfinder.Projects.References;
using Sitecore.Pathfinder.Snapshots;
using Sitecore.Pathfinder.Text;

namespace Sitecore.Pathfinder.Parsing.References
{
    [Export(typeof(IReferenceParserService))]
    public class ReferenceParserService : IReferenceParserService
    {
        [CanBeNull, ItemNotNull]
        private List<Tuple<string, string>> _ignoreReferences;

        [ImportingConstructor]
        public ReferenceParserService([NotNull] IConfiguration configuration, [NotNull] IFactoryService factory, [NotNull] IPipelineService pipelines)
        {
            Configuration = configuration;
            Factory = factory;
            Pipelines = pipelines;
        }

        [NotNull]
        protected IConfiguration Configuration { get; }

        [NotNull]
        protected IFactoryService Factory { get; }

        [NotNull]
        protected IPipelineService Pipelines { get; }

        public virtual bool IsIgnoredReference(string referenceText)
        {
            if (_ignoreReferences == null)
            {
                _ignoreReferences = GetIgnoredReferences();
            }

            foreach (var pair in _ignoreReferences)
            {
                switch (pair.Item2)
                {
                    case "starts-with":
                        if (referenceText.StartsWith(pair.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        break;
                    case "ends-with":
                        if (referenceText.EndsWith(pair.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        break;
                    case "contains":
                        if (referenceText.IndexOf(pair.Item1, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }

                        break;

                    default:
                        if (string.Equals(referenceText, pair.Item1, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        public IEnumerable<IReference> ParseReferences(Field field)
        {
            var databaseName = string.Empty;
            var value = field.Value;

            // todo: templates may not be loaded at this point

            // look for database name
            if (field.TemplateField.Source.IndexOf("database", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var parameters = new UrlString(field.TemplateField.Source);
                databaseName = parameters["databasename"];
                if (string.IsNullOrEmpty(databaseName))
                {
                    databaseName = parameters["database"];
                }

                if (string.IsNullOrEmpty(databaseName))
                {
                    databaseName = string.Empty;
                }
            }

            // check for fields that contains paths
            var pathFields = Configuration.GetStringList(Constants.Configuration.CheckProject.PathFields);
            if (pathFields.Contains(field.FieldId.Format()))
            {
                var sourceProperty = field.ValueProperty;
                if (sourceProperty.SourceTextNode == TextNode.Empty)
                {
                    sourceProperty = field.FieldNameProperty;
                }

                var referenceText = PathHelper.NormalizeItemPath(value);
                if (!referenceText.StartsWith("~/"))
                {
                    referenceText = "~/" + referenceText.TrimStart('/');
                }

                yield return Factory.FileReference(field.Item, sourceProperty, referenceText);
                yield break;
            }

            var textNode = TraceHelper.GetTextNode(field.ValueProperty, field.FieldNameProperty, field);
            foreach (var reference in ParseReferences(field.Item, textNode, field.Value, databaseName))
            {
                yield return reference;
            }
        }

        public virtual IEnumerable<IReference> ParseReferences<T>(IProjectItem projectItem, SourceProperty<T> sourceProperty)
        {
            var sourceTextNode = sourceProperty.SourceTextNode;
            return ParseReferences(projectItem, sourceTextNode, sourceTextNode.Value, string.Empty);
        }

        public virtual IEnumerable<IReference> ParseReferences(IProjectItem projectItem, ITextNode textNode)
        {
            var referenceText = textNode.Value.Trim();

            return ParseReferences(projectItem, textNode, referenceText, string.Empty);
        }

        protected virtual int EndOfFilePath([NotNull] string text, int start)
        {
            var chars = text.ToCharArray();
            var invalidChars = Path.GetInvalidPathChars();

            for (var i = start; i < text.Length; i++)
            {
                var c = chars[i];
                if (invalidChars.Contains(c))
                {
                    return i;
                }
            }

            return text.Length;
        }

        protected virtual int EndOfItemPath([NotNull] string text, int start)
        {
            var chars = text.ToCharArray();

            for (var i = start; i < text.Length; i++)
            {
                var c = chars[i];
                if (!char.IsDigit(c) && !char.IsLetter(c) && c != '/' && c != ' ' && c != '-' && c != '.' && c != '_' && c != '~')
                {
                    return i;
                }
            }

            return text.Length;
        }

        [NotNull, ItemNotNull]
        protected virtual List<Tuple<string, string>> GetIgnoredReferences()
        {
            var ignoreReferences = new List<Tuple<string, string>>();

            foreach (var pair in Configuration.GetSubKeys(Constants.Configuration.CheckProject.IgnoredReferences))
            {
                var op = Configuration.Get(Constants.Configuration.CheckProject.IgnoredReferences + ":" + pair.Key);
                ignoreReferences.Add(new Tuple<string, string>(pair.Key, op));
            }

            return ignoreReferences;
        }

        [ItemNotNull, NotNull]
        protected virtual IEnumerable<IReference> ParseFilePaths([NotNull] IProjectItem projectItem, [NotNull] ITextNode textNode, [NotNull] string referenceText, [NotNull] string databaseName)
        {
            var s = 0;
            while (true)
            {
                var n = referenceText.IndexOf("~/", s, StringComparison.Ordinal);
                if (n < 0)
                {
                    break;
                }

                var e = EndOfFilePath(referenceText, n);
                var text = referenceText.Mid(n, e - n);

                var reference = ParseReference(projectItem, textNode, text, databaseName);
                if (reference != null)
                {
                    yield return reference;
                }

                s = e;
            }
        }

        [ItemNotNull, NotNull]
        protected virtual IEnumerable<IReference> ParseGuids([NotNull] IProjectItem projectItem, [NotNull] ITextNode textNode, [NotNull] string referenceText, [NotNull] string databaseName)
        {
            var s = 0;
            while (true)
            {
                var n = referenceText.IndexOf('{', s);
                if (n < 0)
                {
                    break;
                }

                // ignore uids
                if (n > 4 && referenceText.Mid(n - 5, 5) == "uid=\"")
                {
                    s = n + 1;
                    continue;
                }

                // ignore {{
                if (referenceText.Mid(n, 2) == "{{")
                {
                    s = n + 2;
                    continue;
                }

                // ignore \{
                if (referenceText.Mid(n, 2) == "\\{")
                {
                    s = n + 2;
                    continue;
                }

                var e = referenceText.IndexOf('}', n);
                if (e < 0)
                {
                    break;
                }

                e++;

                var text = referenceText.Mid(n, e - n);

                var reference = ParseReference(projectItem, textNode, text, databaseName);
                if (reference != null)
                {
                    yield return reference;
                }

                s = e;
            }
        }

        [ItemNotNull, NotNull]
        protected virtual IEnumerable<IReference> ParseItemPaths([NotNull] IProjectItem projectItem, [NotNull] ITextNode textNode, [NotNull] string referenceText, [NotNull] string databaseName)
        {
            var s = 0;
            while (true)
            {
                var n = referenceText.IndexOf("/sitecore", s, StringComparison.OrdinalIgnoreCase);
                if (n < 0)
                {
                    break;
                }

                var e = EndOfItemPath(referenceText, n);
                var text = referenceText.Mid(n, e - n);

                var reference = ParseReference(projectItem, textNode, text, databaseName);
                if (reference != null)
                {
                    yield return reference;
                }

                s = e;
            }
        }

        [CanBeNull]
        protected virtual IReference ParseReference([NotNull] IProjectItem projectItem, [NotNull] ITextNode sourceTextNode, [NotNull] string referenceText, [NotNull] string databaseName)
        {
            if (IsIgnoredReference(referenceText))
            {
                return null;
            }

            var pipeline = Pipelines.Resolve<ReferenceParserPipeline>().Execute(Factory, projectItem, sourceTextNode, referenceText, databaseName);
            return pipeline.Reference;
        }

        [ItemNotNull, NotNull]
        protected virtual IEnumerable<IReference> ParseReferences([NotNull] IProjectItem projectItem, [NotNull] ITextNode textNode, [NotNull] string referenceText, [NotNull] string databaseName)
        {
            // query string: ignore
            if (referenceText.StartsWith("query:"))
            {
                yield break;
            }

            // todo: process media links 
            if (referenceText.StartsWith("/~/media") || referenceText.StartsWith("~/media"))
            {
                yield break;
            }

            // todo: process icon links 
            if (referenceText.StartsWith("/~/icon") || referenceText.StartsWith("~/icon"))
            {
                yield break;
            }

            foreach (var reference in ParseItemPaths(projectItem, textNode, referenceText, databaseName))
            {
                yield return reference;
            }

            foreach (var reference in ParseGuids(projectItem, textNode, referenceText, databaseName))
            {
                yield return reference;
            }

            foreach (var reference in ParseFilePaths(projectItem, textNode, referenceText, databaseName))
            {
                yield return reference;
            }
        }
    }
}
