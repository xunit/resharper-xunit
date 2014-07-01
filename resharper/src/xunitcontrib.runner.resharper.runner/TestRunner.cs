using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.Util;
using Xunit;
using Xunit.Abstractions;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    internal class TestRunner
    {
        private readonly RemoteTaskServer server;
        private readonly TaskExecutorConfiguration configuration;

        public TestRunner(RemoteTaskServer server)
        {
            this.server = server;
            configuration = server.Configuration;
        }

        public void Run(XunitTestAssemblyTask assemblyTask, TaskProvider taskProvider)
        {
            //MessageBox.ShowError("foo");

            var priorCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                // Use the assembly in the folder that the user has specified, or, if not, use the assembly location
                var assemblyFolder = GetAssemblyFolder(configuration, assemblyTask);
                var assemblyPath = Path.Combine(assemblyFolder, GetFileName(assemblyTask.AssemblyLocation));

                Environment.CurrentDirectory = assemblyFolder;

                // If we just pass null for the config file, the AppDomain will load {assembly}.config if it exists,
                // or use the config file of this app domain, which will usually be JetBrains.ReSharper.TaskRunner.*.exe.config.
                // If we specify the name directly, it will just use it, or have no configuration, with no fallback.
                // This is good because it stops the TaskRunner.exe config leaking into your tests. For example, that
                // file redirects all ReSharper assemblies to the current version. When the default AppDomain loads our
                // code, the assemblies are successfully redirected, and we use the latest version. If the new AppDomain
                // uses the same redirects, and the test assembly references resharper assemblies (e.g. xunitcontrib tests!)
                // the redirects are applied, but the new AppDomain can't find the newer assemblies, and throws
                var configFile = assemblyPath + ".config";

                //MessageBox.ShowError("Pants");

                // TODO: Source information provider?
                using (var controller = new XunitFrontController(assemblyPath, configFile, configuration.ShadowCopy))
                {
                    // TODO: How to get the temp folder now?
                    //SetTempFolderPath(executorWrapper);

                    var testCases = GetTestCases(controller, taskProvider);

                    var run = new XunitTestRun(server, controller, taskProvider);
                    run.RunTests(testCases);
                }
            }
            catch (Exception e)
            {
                server.TaskException(assemblyTask, new[] { new TaskException(e) });
            }
            finally
            {
                Environment.CurrentDirectory = priorCurrentDirectory;
            }
        }

        private IEnumerable<ITestCase> GetTestCases(ITestFrameworkDiscoverer discoverer, TaskProvider taskProvider)
        {
            var visitor = new TestDiscoveryVisitor();
            discoverer.Find(false, visitor, new XunitDiscoveryOptions());
            visitor.Finished.WaitOne();
            return visitor.TestCases.Where(c => taskProvider.GetClassTask(c.TestMethod.TestClass.Class.Name) != null);
        }

        // TODO: This was cheeky. Let's find a Better Way
        // Tell ReSharper the cache folder being used for shadow copy, so that if
        // someone kills this process (e.g. user aborts), ReSharper can delete it.
        // xunit doesn't expose this information, so we'll grab it via (sorry) reflection
        //private void SetTempFolderPath(ExecutorWrapper executorWrapper)
        //{
        //    if (!configuration.ShadowCopy)
        //        return;

        //    var fieldInfo = executorWrapper.GetType().GetField("appDomain", BindingFlags.NonPublic | BindingFlags.Instance);
        //    if (fieldInfo == null)
        //        throw new InvalidOperationException("Expected ExecutorWrapped to contain private field \"appDomain\"");

        //    var appDomain = fieldInfo.GetValue(executorWrapper) as AppDomain;
        //    if (appDomain != null)
        //    {
        //        var cachePath = appDomain.SetupInformation.CachePath;
        //        if (!string.IsNullOrEmpty(cachePath))
        //            server.SetTempFolderPath(cachePath);
        //    }
        //}

        private static string GetAssemblyFolder(TaskExecutorConfiguration config, XunitTestAssemblyTask assemblyTask)
        {
            return string.IsNullOrEmpty(config.AssemblyFolder)
                       ? GetDirectoryName(assemblyTask.AssemblyLocation)
                       : config.AssemblyFolder;
        }

        private static string GetDirectoryName(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }

        private static string GetFileName(string filepath)
        {
            return Path.GetFileName(filepath);
        }
    }
}
