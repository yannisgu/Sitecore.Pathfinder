﻿// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Web.Optimization;
using System.Web.Optimization.React;
using React;
using Sitecore.Configuration;
using Sitecore.IO;
using Sitecore.Pathfinder.Extensions;
using Sitecore.Pipelines;

namespace Sitecore.Pathfinder.React.Pipelines.Loader
{
    public class AddJsxScripts
    {
        public void Process([NotNull] PipelineArgs args)
        {
            var bundleName = Settings.GetSetting("Pathfinder.React.BundleName");
            if (string.IsNullOrEmpty(bundleName))
            {
                return;
            }

            var directories = Settings.GetSetting("Pathfinder.React.JsxFolders");
            if (string.IsNullOrEmpty(directories))
            {
                return;
            }

            ReactSiteConfiguration.Configuration.SetReuseJavaScriptEngines(true);

            var jsxBundle = new BabelBundle(bundleName);

            foreach (var directory in directories.Split(Constants.Pipe, StringSplitOptions.RemoveEmptyEntries))
            {
                var absoluteDirectory = directory;
                if (absoluteDirectory.StartsWith("~/"))
                {
                    absoluteDirectory = absoluteDirectory.Mid(2);
                }

                absoluteDirectory = FileUtil.MapPath(absoluteDirectory);
                if (!Directory.Exists(absoluteDirectory))
                {
                    continue;
                }

                foreach (var fileName in Directory.GetFiles(absoluteDirectory, "*.jsx", SearchOption.AllDirectories))
                {
                    var jsx = "~" + FileUtil.UnmapPath(fileName, false);
                    ReactSiteConfiguration.Configuration.AddScript(jsx);
                    jsxBundle.Include(jsx);
                }
            }

            BundleTable.Bundles.Add(jsxBundle);
        }
    }
}