﻿// © 2015 Sitecore Corporation A/S. All rights reserved.

using NUnit.Framework;
using Sitecore.Pathfinder.Compiling.FieldCompilers;
using Sitecore.Pathfinder.Diagnostics;
using Sitecore.Pathfinder.Projects.Templates;
using Sitecore.Pathfinder.Snapshots;
using System;
using System.ComponentModel.Composition;

namespace Sitecore.Pathfinder.Projects.Items
{
    [TestFixture]
    public class FieldTests
    {
        [Test]
        public void Compile_NullCompilers()
        {
            var context = CreateContext(new IFieldCompiler[0]);
            var field = new Field(Item.Empty) { Value = "Lorem Ipsum" };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.False);
        }

        [Test]
        public void Compile_EmptyCompilers()
        {
            var context = CreateContext(new IFieldCompiler[0]);
            var field = new Field(Item.Empty) { Value = "Lorem Ipsum" };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.False);
        }

        [Test]
        public void Compile_NoMatchingCompiler()
        {
            var compilers = new IFieldCompiler[] { new CheckboxFieldCompiler() };
            var project = new Project(null, null, null, null, null, null, null, null, new ProjectIndexer());
            var template = CreateTemplate(project);
            var context = CreateContext(compilers);

            var item = new Item(project, Guid.NewGuid(), "master", "item", "/sitecore/item", template.ItemIdOrPath);
            project.AddOrMerge(item);

            var field = new Field(item)
            {
                FieldName = "Text",
                Value = "Lorem Ipsum"
            };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.False);
        }

        [Test]
        public void Compile_MatchingCompiler()
        {
            var compilers = new IFieldCompiler[] { new CheckboxFieldCompiler() };
            var project = new Project(null, null, null, null, null, null, null, null, new ProjectIndexer());
            var template = CreateTemplate(project);
            var context = CreateContext(compilers);

            var item = new Item(project, Guid.NewGuid(), "master", "item", "/sitecore/item", template.ItemName);
            project.AddOrMerge(item);

            var field = new Field(item)
            {
                FieldName = "Checkbox",
                Value = "True"
            };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.True);
            NUnit.Framework.Assert.That(field.CompiledValue, Is.EqualTo("1"));
        }

        [Test]
        public void Compile_ExclusiveCompiler()
        {
            var compilers = new IFieldCompiler[] { new CheckboxFieldCompiler(), new ReplaceCompiler("alpha") };
            var project = new Project(null, null, null, null, null, null, null, null, new ProjectIndexer());
            var template = CreateTemplate(project);
            var context = CreateContext(compilers);

            var item = new Item(project, Guid.NewGuid(), "master", "item", "/sitecore/item", template.ItemName);
            project.AddOrMerge(item);

            var field = new Field(item)
            {
                FieldName = "Checkbox",
                Value = "True"
            };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.True);
            NUnit.Framework.Assert.That(field.CompiledValue, Is.EqualTo("1"));
        }

        [Test]
        public void Compile_NonExclusiveCompiler()
        {
            var compilers = new IFieldCompiler[] { new ReplaceCompiler("alpha"), new ReplaceCompiler("beta"), };
            var project = new Project(null, null, null, null, null, null, null, null, new ProjectIndexer());
            var template = CreateTemplate(project);
            var context = CreateContext(compilers);

            var item = new Item(project, Guid.NewGuid(), "master", "item", "/sitecore/item", template.ItemName);
            project.AddOrMerge(item);

            var field = new Field(item)
            {
                FieldName = "Text",
                Value = "True"
            };

            field.Compile(context);

            NUnit.Framework.Assert.That(field.IsCompiled, Is.True);
            NUnit.Framework.Assert.That(field.CompiledValue, Is.EqualTo("beta"));
        }

        [NotNull]
        private IFieldCompileContext CreateContext([CanBeNull,ItemNotNull] IFieldCompiler[] compilers)
        {
            var config = new Configuration.ConfigurationModel.Configuration();
            return new FieldCompileContext(config, null, null, null, compilers);
        }

        [NotNull]
        private Template CreateTemplate([NotNull] IProject project)
        {
            var template = new Template(project, Guid.NewGuid(), "master", "dummy template", Guid.NewGuid().ToString());
            var stringField = new TemplateField(template, Guid.NewGuid())
            {
                Type = "Single-Line Text",
                FieldName = "Text"
            };

            var checkboxField = new TemplateField(template, Guid.NewGuid())
            {
                Type = "Checkbox",
                FieldName = "Checkbox"
            };

            var section = new TemplateSection(template, Guid.NewGuid());
            section.Fields.Add(stringField);
            section.Fields.Add(checkboxField);

            template.Sections.Add(section);

            project.AddOrMerge(template);

            return template;
        }

        private class ReplaceCompiler : FieldCompilerBase
        {
            [NotNull]
            private string Value { get; set; }

            [ImportingConstructor]
            public ReplaceCompiler([NotNull] string value) : base(Constants.FieldCompilers.Low)
            {
                Value = value;
            }

            public override bool CanCompile(IFieldCompileContext context, Field field)
            {
                return true;
            }

            public override string Compile(IFieldCompileContext context, Field field)
            {
                return Value;
            }
        }
    }
}
