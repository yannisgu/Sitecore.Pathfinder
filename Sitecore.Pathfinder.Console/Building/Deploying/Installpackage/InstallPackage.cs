// � 2015 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Web;
using Sitecore.Pathfinder.Extensions;

namespace Sitecore.Pathfinder.Building.Deploying.Installpackage
{
    [Export(typeof(ITask))]
    public class InstallPackage : RequestTaskBase
    {
        public InstallPackage() : base("install-package")
        {
        }

        public override void Run(IBuildContext context)
        {
            if (context.Project.HasErrors)
            {
                context.Trace.TraceInformation(Texts.Package_contains_errors_and_will_not_be_deployed);
                context.IsAborted = true;
                return;
            }

            context.Trace.TraceInformation(Texts.Installing___);

            var packageId = Path.GetFileNameWithoutExtension(context.Configuration.Get("nuget:filename"));
            if (string.IsNullOrEmpty(packageId))
            {
                return;
            }

            var hostName = context.Configuration.GetString(Constants.Configuration.HostName).TrimEnd('/');
            var installUrl = context.Configuration.GetString(Constants.Configuration.InstallUrl).TrimStart('/');
            var url = hostName + "/" + installUrl + HttpUtility.UrlEncode(packageId);

            if (!Request(context, url))
            {
                return;
            }

            foreach (var snapshot in context.Project.Items.SelectMany(i => i.Snapshots))
            {
                snapshot.SourceFile.IsModified = false;
            }
        }
    }
}