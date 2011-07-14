using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Compete.Core;
using Compete.Core.Infrastructure;
using Compete.Persistence;
using Compete.Site.Infrastructure;
using Compete.Site.Refereeing;
using Compete.Site.Startup;
using Compete.TeamManagement;
using Machine.Container;
using Machine.Container.Plugins;
using Machine.Container.Services;
using Machine.Core;
using Machine.MsMvc;
using Microsoft.Practices.ServiceLocation;

/*
 * 4/1/2011
 *      Stop database on Application_End
 *      Switch to HWC
 *      Increase login timeout to 1440 (one day)
 *      Code to recover on restart of role
 *      
 */

namespace Compete.Site
{
    public class MvcApplication : System.Web.HttpApplication
    {
        IMachineContainer _container;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
                "GameLog",
                "GameLog/{teamName}.vs.{otherTeamName}",
                new { controller = "GameLog", action = "Index" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = "" }
            );

            routes.MapRoute(
                "None",
                 "",
                new { controller = "Home", action = "Index", id = "" }
            );
        }

        private static void SetupStorage(String installPath)
        {
            // get path to target local storage directory
            String targetPath = RockPaperAzure.AzureHelper.GetLocalFolder();

            // create a staging folder
            string stagingPath = Path.Combine(targetPath, "staging");
            if (!System.IO.Directory.Exists(stagingPath))
            {
                System.IO.Directory.CreateDirectory(stagingPath);
            }

            // create a bots folder
            string botsPath = Path.Combine(targetPath, "bots");
            if (!System.IO.Directory.Exists(botsPath))
            {
                System.IO.Directory.CreateDirectory(botsPath);
            }

            // copy 'house' bot files to local storage directory.  File.Copy not used because it retains
            // the stream zone information, which will later prevent DLL from being dynamically loaded.
            String[] botFiles = System.IO.Directory.GetFiles(Path.Combine(installPath, "bots"));
            foreach (String f in botFiles)
            {
                FileStream srcStream = null;
                FileStream destStream = null;
                String destPath = null;
                try
                {
                    destPath = Path.Combine(botsPath, Path.GetFileName(f));
                    if (!File.Exists(destPath))
                    {
                        srcStream = File.OpenRead(f);
                        destStream = File.Create(destPath);
                        srcStream.CopyTo(destStream);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error copying bot file {0}: {1}", ex.Message);
                }
                finally
                {
                    if (srcStream != null) srcStream.Close();
                    if (destStream != null) destStream.Close();
                }
            }

            // create a src folder (for dynamic language support)
            //string srcPath = Path.Combine(targetPath, "src");
            //if (!System.IO.Directory.Exists(srcPath))
            //{
            //    System.IO.Directory.CreateDirectory(srcPath);
            //}

            //String[] srcFiles = System.IO.Directory.GetFiles(Path.Combine(installPath, "src"));
            //foreach (String f in srcFiles)
            //{
            //    File.Copy(f, Path.Combine(srcPath, Path.GetFileName(f)));
            //}
        }

        protected void Application_Start()
        {
            SetupStorage(Server.MapPath("~/install"));

            var path = Path.GetDirectoryName(Server.MapPath("~/Web.config"));
            AppDomainHelper.Start(path);
            Database.Start(path);

            _container = CreateContainer();
            _container.Resolve.Object<WebServerStartup>().Start();

            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_End()
        {
            Database.Stop();
        }

        protected static IMachineContainer CreateContainer()
        {
            var container = new MachineContainer();
            ContainerRegistrationHelper helper = new ContainerRegistrationHelper(container);
            container.Initialize();
            container.PrepareForServices();
            helper.AddServiceCollection(new CoreServices());
            helper.AddServiceCollection(new SiteServices());
            helper.AddServiceCollection(new PersistenceServices());
            helper.AddServiceCollection(new MsMvcServices());
            helper.AddServiceCollection(new TeamManagementServices());
            container.Start();

            IoC.Container = container;
            var adapter = new CommonServiceLocatorAdapter(container);
            ServiceLocator.SetLocatorProvider(() => adapter);
            return container;
        }

        public class SiteServices : IServiceCollection
        {
            public void RegisterServices(ContainerRegisterer register)
            {
                register.Type<WebServerStartup>();
                register.Type<RefereeThread>();
                register.Type<ScoreKeeper>();
                register.Type<IFormsAuthentication>().ImplementedBy<FormsAuthenticationService>();
                register.Type<ISignin>().ImplementedBy<SigninService>();
                register.Type<IInitialSetup>().ImplementedBy<InitialSetupService>();
                register.Type<MatchStarter>();

                GetType().Assembly.GetExportedTypes().Where(x => typeof(Controller).IsAssignableFrom(x)).Each(
                  x => register.Type(x).AsTransient()
                  );
            }
        }
    }
}