using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Shell;
using Tasks.Show.Helpers;
using Tasks.Show.ViewModels;

namespace Tasks.Show 
{
    /// <summary>
    /// Entry point.
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        public static AssemblyAttributes? Attribs;
        private Root m_root;
        private static ILogger _logger = null;
        public static ILogger Logger
        {
            get
            {
                if (_logger is null)
                    _logger = GetLogger(LogLevel.Debug);

                return _logger;
            }
        }

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance("Tasks.Show"))
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);

                var application = new App();

                application.Init();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public static Root Root { get { return ((App)App.Current).m_root; } }
    
        public void Init()
        {
            this.InitializeComponent();

            try
            {
                m_root = new Root(Storage.Load(), FindResource("FolderColors") as IEnumerable<Color>);
                
                Attribs = new AssemblyAttributes();

                #region [Log Dependencies]
                Logger.WriteLine($"Listing assemblies...", LogLevel.Debug);
                var assems = Attribs.GetAllAssemblies();
                foreach (KeyValuePair<string, Version> assem in assems)
                    Logger.WriteLine($"{assem.Key}: {assem.Value}", LogLevel.Debug);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", "App.Init", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments from external execution
            return ((MainWindow)MainWindow).ProcessCommandLineArgs(args, false);
        }

        #endregion

        /// <summary>
        /// Handle application object exceptions. (main UI thread only)
        /// </summary>
        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled exception from Dispatcher {e.Dispatcher.Thread.ManagedThreadId}: {e.Exception.Message}");
                System.Diagnostics.Debug.WriteLine(ToLogString(e.Exception, "Full Error Dump"));
                MessageBox.Show($"{e.Exception}", "UnhandledException", MessageBoxButton.OK, MessageBoxImage.Warning);
                Logger.WriteLine($"{e.Exception.Message}", LogLevel.Critical);
                //System.Diagnostics.EventLog.WriteEntry(SystemTitle, $"Unhandled exception thrown from Dispatcher {e.Dispatcher.Thread.ManagedThreadId}: {e.Exception.ToString()}");
                e.Handled = true;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Handle exceptions thrown from custom threads.
        /// </summary>
        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception? ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Thread exception: {ex}");
                    MessageBox.Show($"{ex.Message}", "ThreadException", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Logger.WriteLine($"Thread exception: {ex}", LogLevel.Critical);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Logging Service Provider (leaving it here for convenience).
        /// </summary>
        /// <returns><see cref="ILogger"/></returns>
        public static ILogger GetLogger(LogLevel level = LogLevel.Debug)
        {
            //var logPaths = System.IO.DriveInfo.GetDrives().Where(di => (di.DriveType == DriveType.Fixed) && (di.IsReady) && (di.AvailableFreeSpace > 1000000)).Select(di => di.RootDirectory).OrderByDescending(di => di.FullName);
            //string root = logPaths.FirstOrDefault().FullName; // "D:\"
            return new FileLogger(System.IO.Path.Combine(FileLogger.GetRoot(), "Logs"), level);
        }

        /// <summary>
        /// Creates a loggable string from an <see cref="Exception"/>.
        /// The result includes the stacktrace, inner exception, et al.
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, may be empty or null.</param>
        /// <returns>formatted string</returns>
        public static string ToLogString(Exception ex, string additionalMessage = "")
        {
            System.Text.StringBuilder msg = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                msg.Append($"-----[{additionalMessage}]-----");
                msg.Append(Environment.NewLine);
            }
            else
            {
                msg.Append($"-----[{DateTime.Now.ToString("hh:mm:ss.fff tt")}]-----");
                msg.Append(Environment.NewLine);
            }

            if (ex != null)
            {
                try
                {
                    Exception orgEx = ex;
                    msg.Append("[Exception]: ");
                    while (orgEx != null)
                    {
                        msg.Append(orgEx.Message);
                        msg.Append(Environment.NewLine);
                        orgEx = orgEx.InnerException;
                    }

                    if (ex.Source != null)
                    {
                        msg.Append("[Source]: ");
                        msg.Append(ex.Source);
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.Data != null)
                    {
                        foreach (object i in ex.Data)
                        {
                            msg.Append("[Data]: ");
                            msg.Append(i.ToString());
                            msg.Append(Environment.NewLine);
                        }
                    }

                    if (ex.StackTrace != null)
                    {
                        msg.Append("[StackTrace]: ");
                        msg.Append(ex.StackTrace.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.TargetSite != null)
                    {
                        msg.Append("[TargetSite]: ");
                        msg.Append(ex.TargetSite.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    Exception baseException = ex.GetBaseException();
                    if (baseException != null)
                    {
                        msg.Append("[BaseException]: ");
                        msg.Append(ex.GetBaseException());
                    }
                }
                catch (Exception iex)
                {
                    System.Diagnostics.Debug.WriteLine($"ToLogString: {iex.Message}");
                }
            }
            return msg.ToString();
        }
    }
}
