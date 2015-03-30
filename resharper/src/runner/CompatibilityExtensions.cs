using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using JetBrains.ReSharper.TaskRunnerFramework;
using Microsoft.WindowsAPICodePack.Dialogs;
using MessageBox = JetBrains.Util.MessageBox;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public static class CompatibilityExtensions
    {
        public static void ShowNotification(this IRemoteTaskServer server, string message, string description)
        {
            // Notify the user that something really bad has happened. It's not nice, but we'll show a message box
            if (!TaskDialog.IsPlatformSupported)
            {
                MessageBox.ShowExclamation(message + Environment.NewLine + Environment.NewLine + description,
                    "xUnit.net Test Runner");
                return;
            }

            using (new EnableThemingInScope(true))
            {
                var dialog = new TaskDialog
                {
                    Caption = "xUnit.net Test Runner",
                    InstructionText = "xUnit.net Test Runner",
                    Text = message,
                    Icon = TaskDialogStandardIcon.Information,
                    DetailsExpanded = false,
                    DetailsCollapsedLabel = "Show details",
                    DetailsExpandedLabel = "Hide details",
                    DetailsExpandedText = description
                };
                dialog.Show();
            }
        }

        // See https://support.microsoft.com/kb/830033?wa=wsignin1.0
        [SuppressUnmanagedCodeSecurity]
        private class EnableThemingInScope : IDisposable
        {
            private IntPtr cookie;
            private static ACTCTX enableThemingActivationContext;
            private static IntPtr hActCtx;
            private static bool contextCreationSucceeded;

            public EnableThemingInScope(bool enable)
            {
                cookie = IntPtr.Zero;
                if (enable && OSFeature.Feature.IsPresent(OSFeature.Themes))
                {
                    if (EnsureActivateContextCreated())
                    {
                        if (!ActivateActCtx(hActCtx, out cookie))
                        {
                            // Be sure cookie always zero if activation failed
                            cookie = IntPtr.Zero;
                        }
                    }
                }
            }

            ~EnableThemingInScope()
            {
                Dispose(false);
            }

            void IDisposable.Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (cookie != IntPtr.Zero)
                {
                    if (DeactivateActCtx(0, cookie))
                    {
                        // deactivation succeeded...
                        cookie = IntPtr.Zero;
                    }
                }
            }

            private bool EnsureActivateContextCreated()
            {
                lock (typeof(EnableThemingInScope))
                {
                    if (!contextCreationSucceeded)
                    {
                        // Pull manifest from the .NET Framework install
                        // directory

                        string assemblyLoc = null;

                        FileIOPermission fiop = new FileIOPermission(PermissionState.None);
                        fiop.AllFiles = FileIOPermissionAccess.PathDiscovery;
                        fiop.Assert();
                        try
                        {
                            assemblyLoc = typeof(Object).Assembly.Location;
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }

                        string manifestLoc = null;
                        string installDir = null;
                        if (assemblyLoc != null)
                        {
                            installDir = Path.GetDirectoryName(assemblyLoc);
                            const string manifestName = "XPThemes.manifest";
                            manifestLoc = Path.Combine(installDir, manifestName);
                        }

                        if (manifestLoc != null && installDir != null)
                        {
                            enableThemingActivationContext = new ACTCTX();
                            enableThemingActivationContext.cbSize = Marshal.SizeOf(typeof(ACTCTX));
                            enableThemingActivationContext.lpSource = manifestLoc;

                            // Set the lpAssemblyDirectory to the install
                            // directory to prevent Win32 Side by Side from
                            // looking for comctl32 in the application
                            // directory, which could cause a bogus dll to be
                            // placed there and open a security hole.
                            enableThemingActivationContext.lpAssemblyDirectory = installDir;
                            enableThemingActivationContext.dwFlags = ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID;

                            // Note this will fail gracefully if file specified
                            // by manifestLoc doesn't exist.
                            hActCtx = CreateActCtx(ref enableThemingActivationContext);
                            contextCreationSucceeded = (hActCtx != new IntPtr(-1));
                        }
                    }

                    // If we return false, we'll try again on the next call into
                    // EnsureActivateContextCreated(), which is fine.
                    return contextCreationSucceeded;
                }
            }

            // All the pinvoke goo...
            [DllImport("Kernel32.dll")]
            private static extern IntPtr CreateActCtx(ref ACTCTX actctx);

            [DllImport("Kernel32.dll")]
            private static extern bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);

            [DllImport("Kernel32.dll")]
            private static extern bool DeactivateActCtx(uint dwFlags, IntPtr lpCookie);

            private const int ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0x004;

            private struct ACTCTX
            {
                public int cbSize;
                public uint dwFlags;
                public string lpSource;
                public ushort wProcessorArchitecture;
                public ushort wLangId;
                public string lpAssemblyDirectory;
                public string lpResourceName;
                public string lpApplicationName;
            }
        }

    }
}