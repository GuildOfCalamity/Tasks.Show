#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows;
using Con = System.Diagnostics.Debug;
using Path = System.IO.Path;

namespace Tasks.Show.Helpers
{
    public static class ExtensionUtils
    {
        #region [OS Version Helpers]
        /* [!!! IMPORTANT NOTE: !!!]
           Older versions of dotNET (below dotNET 5) may not provide the correct major version.
           To make sure you get the right version using Environment.OSVersion you should add an
           "app.manifest" using Visual Studio and then un-comment the following supportedOS tags:

        <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
          <application>
            <!-- Windows Vista -->
            <!--<supportedOS Id="{e2011457-1546-43c5-a5fe-008deee3d3f0}" />-->
            <!-- Windows 7 -->
            <!--<supportedOS Id="{35138b9a-5d96-4fbd-8e2d-a2440225f93a}" />-->
            <!-- Windows 8 -->
            <!--<supportedOS Id="{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}" />-->
            <!-- Windows 8.1 -->
            <!--<supportedOS Id="{1f676c76-80e1-4239-95bb-83d0f6d0da78}" />-->
            <!-- Windows 10 -->
            <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
          </application>
        </compatibility>

        [Sample Code]
        var version = Environment.OSVersion;
        Console.WriteLine(version);
        Before: "Microsoft Windows NT 6.2.9200.0"
        After: "Microsoft Windows NT 10.0.19044.0"

          If you don't want to use an app.manifest, then starting with .NET 4.7.1+ you can parse the OSDescription property:
          string description = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
          // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.runtimeinformation.isosplatform?view=netframework-4.7.1

          You can also read from the registry key "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion"
           - ProductName (String)
           - CurrentBuild (String)
           - CurrentMajorVersionNumber (DWORD)
           - CurrentMinorVersionNumber (DWORD)
          This registry key has contained the OS ProductName since Windows XP.
        */

        internal static bool IsWindowsNT { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        /// Minimum supported client: Windows 2000 Professional
        /// </summary>
        /// <returns>true if OS is Windows XP or greater</returns>
        public static bool IsWindowsXPOrLater()
        {
            OperatingSystem osVersion = Environment.OSVersion;
            ExtensionUtils.OSVERSIONINFOEX osvi = new ExtensionUtils.OSVERSIONINFOEX();
            osvi.dwOSVersionInfoSize = Marshal.SizeOf(typeof(ExtensionUtils.OSVERSIONINFOEX));
            if (ExtensionUtils.GetVersionEx(ref osvi))
            {
                int majorVersion = osVersion.Version.Major;
                int minorVersion = osVersion.Version.Minor;
                int platformId = osvi.dwPlatformId;
                byte productType = osvi.wProductType;
                short suiteMask = osvi.wSuiteMask;
                string servicePack = osvi.szCSDVersion;
            }
            return ((osvi.dwMajorVersion > 5) || ((osvi.dwMajorVersion == 5) && (osvi.dwMinorVersion >= 1)));
        }

        /// <summary>
        /// Minimum supported client: Windows 2000 Professional
        /// </summary>
        public static bool IsWindows8OrLater()
        {
            Con.WriteLine($"Major={Environment.OSVersion.Version.Major} Minor={Environment.OSVersion.Version.Minor} Revision={Environment.OSVersion.Version.Revision} Build={Environment.OSVersion.Version.Build}");
            return IsWindowsNT && Environment.OSVersion.Version >= new Version(6, 2);
        }

        /// <summary>
        /// Minimum supported client: Windows 2000 Professional
        /// </summary>
        public static bool IsWindows10OrLater()
        {
            Con.WriteLine($"Major={Environment.OSVersion.Version.Major} Minor={Environment.OSVersion.Version.Minor} Revision={Environment.OSVersion.Version.Revision} Build={Environment.OSVersion.Version.Build}");
            return IsWindowsNT && Environment.OSVersion.Version >= new Version(10, 0);
        }

        /// <summary>
        /// Minimum supported client: Windows 2000 Professional
        /// </summary>
        public static bool IsWindows11OrLater()
        {
            Con.WriteLine($"Major={Environment.OSVersion.Version.Major} Minor={Environment.OSVersion.Version.Minor} Revision={Environment.OSVersion.Version.Revision} Build={Environment.OSVersion.Version.Build}");
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-getproductinfo
        /// This method uses KERNEL32's GetProductInfo() to correctly determine if the OS is Windows 10,
        /// regardless of what settings are in the app.manifest for the project. A note on the manifest:
        /// If your executable assembly manifest doesn't explicitly state that your exe assembly is compatible
        /// with Windows 8.1 and/or Windows 10.0, System.Environment.OSVersion will return Windows 8 version, 
        /// which is 6.2, instead of 6.3 and 10.0! Source: https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-osversioninfoexa?redirectedfrom=MSDN
        /// </summary>
        public static bool IsWin10Product()
        {
            System.Diagnostics.Debug.WriteLine($"Major={Environment.OSVersion.Version.Major} Minor={Environment.OSVersion.Version.Minor} Revision={Environment.OSVersion.Version.Revision} Build={Environment.OSVersion.Version.Build}");

            GetProductInfo(Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, 0, 0, out var productNum);

            switch (productNum)
            {
                case 0x000000B2: //Windows 10 S
                case 0x000000B3: //Windows 10 S N
                case 0x00000031: //Windows 10 Pro N
                case 0x000000A1: //Windows 10 Pro for Workstations
                case 0x000000A2: //Windows 10 Pro for Workstations N
                case 0x00000030: //Windows 10 Pro
                case 0x00000068: //Windows 10 Mobile
                case 0x00000085: //Windows 10 Mobile Enterprise
                case 0x0000007B: //Windows 10 IoT Core
                case 0x00000083: //Windows 10 IoT Core Commercial
                case 0x00000065: //Windows 10 Home
                case 0x00000063: //Windows 10 Home China
                case 0x00000062: //Windows 10 Home N
                case 0x00000064: //Windows 10 Home Single Language
                case 0x00000079: //Windows 10 Education
                case 0x0000007A: //Windows 10 Education N
                case 0x00000004: //Windows 10 Enterprise
                case 0x00000046: //Windows 10 Enterprise E
                case 0x00000048: //Windows 10 Enterprise Evaluation
                case 0x0000001B: //Windows 10 Enterprise N
                case 0x00000054: //Windows 10 Enterprise N Evaluation
                case 0x0000007D: //Windows 10 Enterprise 2015 LTSB
                case 0x00000081: //Windows 10 Enterprise 2015 LTSB Evaluation
                case 0x0000007E: //Windows 10 Enterprise 2015 LTSB N
                case 0x00000082: //Windows 10 Enterprise 2015 LTSB N Evaluation
                    return true;
                default:
                    return false;
            }

            #region PRODUCTS
            /* --- List updated: January 2023 ---
            const int PRODUCT_UNDEFINED = 0x00000000; //An unknown product
            const int PRODUCT_BUSINESS = 0x00000006; //Business
            const int PRODUCT_BUSINESS_N = 0x00000010; //Business N
            const int PRODUCT_CLOUD = 0x000000B2; //Windows 10 S
            const int PRODUCT_CLOUDN = 0x000000B3; //Windows 10 S N
            const int PRODUCT_CLUSTER_SERVER = 0x00000012; //HPC Edition
            const int PRODUCT_CLUSTER_SERVER_V = 0x00000040; //Server Hyper Core V
            const int PRODUCT_CORE = 0x00000065; //Windows 10 Home
            const int PRODUCT_CORE_COUNTRYSPECIFIC = 0x00000063; //Windows 10 Home China
            const int PRODUCT_CORE_N = 0x00000062; //Windows 10 Home N
            const int PRODUCT_CORE_SINGLELANGUAGE = 0x00000064; //Windows 10 Home Single Language
            const int PRODUCT_DATACENTER_EVALUATION_SERVER = 0x00000050;//Server Datacenter (evaluation installation)
            const int PRODUCT_DATACENTER_A_SERVER_CORE = 0x00000091; //Server Datacenter, Semi-Annual Channel (core installation)
            const int PRODUCT_STANDARD_A_SERVER_CORE = 0x00000092; //Server Standard, Semi-Annual Channel (core installation)
            const int PRODUCT_DATACENTER_SERVER = 0x00000008; //Server Datacenter (full installation. For Server Core installations of Windows Server 2012 and later, use the method, Determining whether Server Core is running.)
            const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C; //Server Datacenter (core installation, Windows Server 2008 R2 and earlier)
            const int PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027; //Server Datacenter without Hyper-V (core installation)
            const int PRODUCT_DATACENTER_SERVER_V = 0x00000025; //Server Datacenter without Hyper-V (full installation)
            const int PRODUCT_EDUCATION = 0x00000079; //Windows 10 Education
            const int PRODUCT_EDUCATION_N = 0x0000007A; //Windows 10 Education N
            const int PRODUCT_ENTERPRISE = 0x00000004; //Windows 10 Enterprise
            const int PRODUCT_ENTERPRISE_E = 0x00000046; //Windows 10 Enterprise E
            const int PRODUCT_ENTERPRISE_EVALUATION = 0x00000048; //Windows 10 Enterprise Evaluation
            const int PRODUCT_ENTERPRISE_N = 0x0000001B; //Windows 10 Enterprise N
            const int PRODUCT_ENTERPRISE_N_EVALUATION = 0x00000054; //Windows 10 Enterprise N Evaluation
            const int PRODUCT_ENTERPRISE_S = 0x0000007D; //Windows 10 Enterprise 2015 LTSB
            const int PRODUCT_ENTERPRISE_S_EVALUATION = 0x00000081; //Windows 10 Enterprise 2015 LTSB Evaluation
            const int PRODUCT_ENTERPRISE_S_N = 0x0000007E; //Windows 10 Enterprise 2015 LTSB N
            const int PRODUCT_ENTERPRISE_S_N_EVALUATION = 0x00000082; //Windows 10 Enterprise 2015 LTSB N Evaluation
            const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A; //Server Enterprise (full installation)
            const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E; //Server Enterprise (core installation)
            const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029; //Server Enterprise without Hyper-V (core installation)
            const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F; //Server Enterprise for Itanium-based Systems
            const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026; //Server Enterprise without Hyper-V (full installation)
            const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C; //Windows Essential Server Solution Additional
            const int PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E; //Windows Essential Server Solution Additional SVC
            const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B; //Windows Essential Server Solution Management
            const int PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D; //Windows Essential Server Solution Management SVC
            const int PRODUCT_HOME_BASIC = 0x00000002; //Home Basic
            const int PRODUCT_HOME_BASIC_E = 0x00000043; //Not supported
            const int PRODUCT_HOME_BASIC_N = 0x00000005; //Home Basic N
            const int PRODUCT_HOME_PREMIUM = 0x00000003; //Home Premium
            const int PRODUCT_HOME_PREMIUM_E = 0x00000044; //Not supported
            const int PRODUCT_HOME_PREMIUM_N = 0x0000001A; //Home Premium N
            const int PRODUCT_HOME_PREMIUM_SERVER = 0x00000022; //Windows Home Server 2011
            const int PRODUCT_HOME_SERVER = 0x00000013; //Windows Storage Server 2008 R2 Essentials
            const int PRODUCT_HYPERV = 0x0000002A; //Microsoft Hyper-V Server
            const int PRODUCT_IOTENTERPRISE = 0x000000BC; //Windows IoT Enterprise
            const int PRODUCT_IOTENTERPRISE_S = 0x000000BF; //Windows IoT Enterprise LTSC
            const int PRODUCT_IOTUAP = 0x0000007B; //Windows 10 IoT Core
            const int PRODUCT_IOTUAPCOMMERCIAL = 0x00000083; //Windows 10 IoT Core Commercial
            const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E; //Windows Essential Business Server Management Server
            const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020; //Windows Essential Business Server Messaging Server
            const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F; //Windows Essential Business Server Security Server
            const int PRODUCT_MOBILE_CORE = 0x00000068; //Windows 10 Mobile
            const int PRODUCT_MOBILE_ENTERPRISE = 0x00000085; //Windows 10 Mobile Enterprise
            const int PRODUCT_MULTIPOINT_PREMIUM_SERVER = 0x0000004D; //Windows MultiPoint Server Premium (full installation)
            const int PRODUCT_MULTIPOINT_STANDARD_SERVER = 0x0000004C; //Windows MultiPoint Server Standard (full installation)
            const int PRODUCT_PRO_WORKSTATION = 0x000000A1; //Windows 10 Pro for Workstations
            const int PRODUCT_PRO_WORKSTATION_N = 0x000000A2; //Windows 10 Pro for Workstations N
            const int PRODUCT_PROFESSIONAL = 0x00000030; //Windows 10 Pro
            const int PRODUCT_PROFESSIONAL_E = 0x00000045; //Not supported
            const int PRODUCT_PROFESSIONAL_N = 0x00000031; //Windows 10 Pro N
            const int PRODUCT_PROFESSIONAL_WMC = 0x00000067; //Professional with Media Center
            const int PRODUCT_SB_SOLUTION_SERVER = 0x00000032; //Windows Small Business Server 2011 Essentials
            const int PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036; //Server For SB Solutions EM
            const int PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033; //Server For SB Solutions
            const int PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037; //Server For SB Solutions EM
            const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018; //Windows Server 2008 for Windows Essential Server Solutions
            const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023; //Windows Server 2008 without Hyper-V for Windows Essential Server Solutions
            const int PRODUCT_SERVER_FOUNDATION = 0x00000021; //Server Foundation
            const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009; //Windows Small Business Server
            const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019; //Small Business Server Premium
            const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F; //Small Business Server Premium (core installation)
            const int PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038; //Windows MultiPoint Server
            const int PRODUCT_STANDARD_EVALUATION_SERVER = 0x0000004F; //Server Standard (evaluation installation)
            const int PRODUCT_STANDARD_SERVER = 0x00000007; //Server Standard (full installation. For Server Core installations of Windows Server 2012 and later, use the method, Determining whether Server Core is running.)
            const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D; //Server Standard (core installation, Windows Server 2008 R2 and earlier)
            const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028; //Server Standard without Hyper-V (core installation)
            const int PRODUCT_STANDARD_SERVER_V = 0x00000024; //Server Standard without Hyper-V
            const int PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034; //Server Solutions Premium
            const int PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035; //Server Solutions Premium (core installation)
            const int PRODUCT_STARTER = 0x0000000B; //Starter
            const int PRODUCT_STARTER_E = 0x00000042; //Not supported
            const int PRODUCT_STARTER_N = 0x0000002F; //Starter N
            const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017; //Storage Server Enterprise
            const int PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E; //Storage Server Enterprise (core installation)
            const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014; //Storage Server Express
            const int PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B; //Storage Server Express (core installation)
            const int PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER = 0x00000060; //Storage Server Standard (evaluation installation)
            const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015; //Storage Server Standard
            const int PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C; //Storage Server Standard (core installation)
            const int PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER = 0x0000005F; //Storage Server Workgroup (evaluation installation)
            const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016; //Storage Server Workgroup
            const int PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D; //Storage Server Workgroup (core installation)
            const int PRODUCT_ULTIMATE = 0x00000001; //Ultimate
            const int PRODUCT_ULTIMATE_E = 0x00000047; //Not supported
            const int PRODUCT_ULTIMATE_N = 0x0000001C; //Ultimate N
            const int PRODUCT_WEB_SERVER = 0x00000011; //Web Server (full installation)
            const int PRODUCT_WEB_SERVER_CORE = 0x0000001D; //Web Server (core installation)
            */
            #endregion PRODUCTS

            #region VERSIONS (check using the OperatingSystem.Platform property)
            /*
            const int VER_NT_WORKSTATION = 1;                //The operating system is Windows 8, Windows 7, Windows Vista, Windows XP Professional, Windows XP Home Edition, or Windows 2000 Professional. 
            const int VER_NT_DOMAIN_CONTROLLER = 2;          //The system is a domain controller and the operating system is Windows Server 2012 , Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server. 
            const int VER_NT_SERVER = 3;                     //The operating system is Windows Server 2012, Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server. Note that a server that is also a domain controller is reported as VER_NT_DOMAIN_CONTROLLER, not VER_NT_SERVER.
            const int VER_SUITE_SMALLBUSINESS = 1;           //Microsoft Small Business Server was once installed on the system, but may have been upgraded to another version of Windows. Refer to the Remarks section for more information about this bit flag. 
            const int VER_SUITE_ENTERPRISE = 2;              //Windows Server 2008 Enterprise, Windows Server 2003, Enterprise Edition, or Windows 2000 Advanced Server is installed. Refer to the Remarks section for more information about this bit flag. 
            const int VER_SUITE_TERMINAL = 16;               //Terminal Services is installed. This value is always set. If VER_SUITE_TERMINAL is set but VER_SUITE_SINGLEUSERTS is not set, the system is running in application server mode.
            const int VER_SUITE_DATACENTER = 128;            //Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed. 
            const int VER_SUITE_SINGLEUSERTS = 256;          //Remote Desktop is supported, but only one interactive session is supported. This value is set unless the system is running in application server mode. 
            const int VER_SUITE_PERSONAL = 512;              //Windows Vista Home Premium, Windows Vista Home Basic, or Windows XP Home Edition is installed. 
            const int VER_SUITE_BLADE = 1024;                //Windows Server 2003, Web Edition is installed. 
            const int VER_SUITE_BACKOFFICE = 0x00000004;     //Microsoft BackOffice components are installed. 
            const int VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020; //Microsoft Small Business Server is installed with the restrictive client license in force. Refer to the Remarks section for more information about this bit flag. 
            const int VER_SUITE_EMBEDDEDNT = 0x00000040;     //Windows XP Embedded is installed. 
            const int VER_SUITE_STORAGE_SERVER = 0x00002000; //Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed. 
            const int VER_SUITE_COMPUTE_SERVER = 0x00004000; //Windows Server 2003, Compute Cluster Edition is installed. 
            const int VER_SUITE_WH_SERVER = 0x00008000;      //Windows Home Server is installed. 
            const int VER_SUITE_MULTIUSERTS = 0x00020000;    //AppServer mode is enabled. 
            */
            #endregion VERSIONS
        }
        [DllImport("kernel32.dll", SetLastError = false)]
        private static extern bool GetProductInfo(int dwOSMajorVersion, int dwOSMinorVersion, int dwSpMajorVersion, int dwSpMinorVersion, out int pdwReturnedProductType);

        [DllImport("kernel32.dll")]
        public static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }
        #endregion

        #region [Task Helpers]
        /// <summary>
        /// Task extension to add a timeout.
        /// </summary>
        /// <returns>The task with timeout.</returns>
        /// <param name="task">Task.</param>
        /// <param name="timeoutInMilliseconds">Timeout duration in Milliseconds.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async static Task<T> WithTimeout<T>(this Task<T> task, int timeoutInMilliseconds)
        {
            var retTask = await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds))
                .ConfigureAwait(false);

#pragma warning disable CS8603 // Possible null reference return.
            return retTask is Task<T> ? task.Result : default;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Task extension to add a timeout.
        /// </summary>
        /// <returns>The task with timeout.</returns>
        /// <param name="task">Task.</param>
        /// <param name="timeout">Timeout Duration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout) => WithTimeout(task, (int)timeout.TotalMilliseconds);

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        /// <summary>
        /// Attempts to await on the task and catches exception
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="onException">What to do when method has an exception</param>
        /// <param name="continueOnCapturedContext">If the context should be captured.</param>
        public static async void SafeFireAndForget(this Task task, Action<Exception>? onException = null, bool continueOnCapturedContext = false)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException.Invoke(ex);
            }
        }

        /// <summary>
        /// Chainable task helper.
        /// var result = await SomeLongAsyncFunction().WithCancellation(cts.Token);
        /// </summary>
        /// <typeparam name="TResult">the type of task result</typeparam>
        /// <returns><see cref="Task"/>TResult</returns>
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancelToken)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            CancellationTokenRegistration reg = cancelToken.Register(() => tcs.TrySetCanceled());
            task.ContinueWith(ant =>
            {
                reg.Dispose(); // NOTE: it's important to dispose of CancellationTokenRegistrations or they will hand around in memory until the application closes
                if (ant.IsCanceled)
                    tcs.TrySetCanceled();
                else if (ant.IsFaulted)
                    tcs.TrySetException(ant.Exception.InnerException);
                else
                    tcs.TrySetResult(ant.Result);
            });
            return tcs.Task;  // Return the TaskCompletionSource result
        }

        public static Task<T> WithAllExceptions<T>(this Task<T> task)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            task.ContinueWith(ignored =>
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        Con.WriteLine($"[TaskStatus.Canceled]");
                        tcs.SetCanceled();
                        break;
                    case TaskStatus.RanToCompletion:
                        tcs.SetResult(task.Result);
                        //Con.WriteLine($"[TaskStatus.RanToCompletion({task.Result})]");
                        break;
                    case TaskStatus.Faulted:
                        // SetException will automatically wrap the original AggregateException
                        // in another one. The new wrapper will be removed in TaskAwaiter, leaving
                        // the original intact.
                        Con.WriteLine($"[TaskStatus.Faulted]: {task.Exception.Message}");
                        tcs.SetException(task.Exception);
                        break;
                    default:
                        Con.WriteLine($"[TaskStatus: Continuation called illegally.]");
                        tcs.SetException(new InvalidOperationException("Continuation called illegally."));
                        break;
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// Task.Factory.StartNew (() => { throw null; }).IgnoreExceptions();
        /// </summary>
        public static void IgnoreExceptions(this Task task, bool logEx = false)
        {
            task.ContinueWith(t =>
            {
                AggregateException ignore = t.Exception;

                ignore?.Flatten().Handle(ex =>
                {
                    if (logEx)
                        Con.WriteLine("Exception type: {0}\r\nException Message: {1}", ex.GetType(), ex.Message);
                    return true; // don't re-throw
                });

            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion

        #region [Memory-Friendly Random]
        private static readonly WeakReference s_random = new WeakReference(null);
        /// <summary>
        /// NOTE: In later versions of .NET a "Random.Shared" property was introduced to alleviate the need for this.
        /// </summary>
        public static Random Rnd
        {
            get
            {
                var r = (Random)s_random.Target;
                if (r == null) { s_random.Target = r = new Random(); }
                return r;
            }
        }
        #endregion

        #region [Image Helpers]
        public static void RotateImage(this System.Windows.Controls.Image imgControl, double angle = 90.0)
        {
            if (imgControl.Source == null)
                return;

            var img = (BitmapSource)(imgControl.Source);
            var cache = new CachedBitmap(img, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            imgControl.Source = new TransformedBitmap(cache, new RotateTransform(angle));
        }

        public static void TurnBlackAndWhite(this System.Windows.Controls.Image imgControl, double alphaThresh = 1.0)
        {
            if (imgControl.Source == null)
                return;

            var img = (BitmapSource)(imgControl.Source);
            imgControl.Source = new FormatConvertedBitmap(img, PixelFormats.Gray8, BitmapPalettes.Gray256, alphaThresh);
        }
        #endregion

        #region [Enumerable Helpers]
        public static IEnumerable<T> JoinLists<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var joined = new[] { list1, list2 }.Where(x => x != null).SelectMany(x => x);
            return joined ?? Enumerable.Empty<T>();
        }
        public static IEnumerable<T> JoinLists<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEnumerable<T> list3)
        {
            var joined = new[] { list1, list2, list3 }.Where(x => x != null).SelectMany(x => x);
            return joined ?? Enumerable.Empty<T>();
        }
        public static IEnumerable<T> JoinMany<T>(params IEnumerable<T>[] array)
        {
            var final = array.Where(x => x != null).SelectMany(x => x);
            return final ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Compare method.
        /// </summary>
        public static bool SequenceEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstIter = first.GetEnumerator();
            var secondIter = second.GetEnumerator();

            if (firstIter is null || secondIter is null)
                return false;

            while (firstIter.MoveNext() && secondIter.MoveNext())
            {
                if (firstIter.Current!.Equals(secondIter.Current!))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region [File System Helpers]
        /// <summary>
        /// Check if a file can be created in the directory.
        /// </summary>
        /// <param name="directoryPath">the directory path to evaluate</param>
        /// <returns>true if the directory is writeable, false otherwise</returns>
        public static bool CanWriteToDirectory(string directoryPath)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(directoryPath, "test.txt"), 1, FileOptions.DeleteOnClose)) { /* no-op */ }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IEnumerable<string> ReadFileLines(string path)
        {
            string line = string.Empty;

            if (!File.Exists(path))
                yield return line;
            else
            {
                using (TextReader reader = File.OpenText(path))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        public static void CopyFiles(string source, string destination)
        {
            try
            {
                if (!source.EndsWith("\\")) { source += "\\"; }

                foreach (var p in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(Path.Combine(destination, p.Substring(source.Length)));
                foreach (var f in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                    File.Copy(f, Path.Combine(destination, f.Substring(source.Length)), true);
            }
            catch (Exception ex)
            {
                Con.WriteLine($"CopyFiles: {ex.Message}");
            }
        }

        public static List<FileInfo> AddRangeOfFiles(string targetDirectory, string fileExt)
        {
            List<FileInfo> files = new List<FileInfo>();
            var targetDir = new DirectoryInfo(targetDirectory);
            files.AddRange(targetDir.GetFiles($"*.{fileExt}", SearchOption.AllDirectories));

            return files;
        }

        public static string GetFileMD5(this string fileName)
        {
            if (File.Exists(fileName))
            {
                string checksum = string.Empty;
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        checksum = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }
                return checksum;
            }
            else
                return string.Empty;
        }

        public static System.Text.Encoding DetermineFileEncoding(this string path)
        {
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
                System.IO.StreamReader sr = new System.IO.StreamReader(fs);
                System.Text.Encoding coding = sr.CurrentEncoding;
                fs.Close(); fs.Dispose();
                sr.Close(); sr.Dispose();
                return coding;
            }
            catch (Exception ex)
            {
                Con.WriteLine($"DetermineFileEncoding: {ex.Message}");
                return System.Text.Encoding.Default;
            }
        }

        /// <summary>
        /// Helper method.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/></param>
        /// <returns>true if file is in use, false otherwise</returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream? stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // - still being written to
                // - or being processed by another thread 
                // - or does not exist
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            //file is not locked   
            return false;
        }
        #endregion

        #region [Misc Helpers]
        public static string NameOf(this object obj)
        {
            return $"{obj.GetType().Name} => {obj.GetType().BaseType?.Name}";
        }

        public static bool IsDefaultValue<T>(T value)
        {
            return object.Equals(default(T), value);
        }

        public static string ReplaceAll(this string input, string[] terms, string replace)
        {
            if (terms.Length == 0)
                throw new ArgumentException($"{nameof(terms)} list contains no usable elements.");

            for (int i = 0; i < terms.Length; i++)
                input = Regex.Replace(input, terms[i], replace);

            return input;
        }

        public static void Swap<T>(ref T x, ref T y)
        {
            T tmp = x;
            x = y;
            y = tmp;
        }

        public static T? ParseEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : (val.CompareTo(max) > 0 ? max : val);
        }

        /// <summary>
        /// Find the maximum in a set.
        /// </summary>
        public static T Max<T>(params T[] values) where T : IComparable
        {
            T result = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (result.CompareTo(values[i]) < 0)
                {
                    result = values[i];
                }

            }
            return result;
        }

        /// <summary>
        /// Find the minimum in a set.
        /// </summary>
        public static T Min<T>(params T[] values) where T : IComparable
        {
            T result = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (result.CompareTo(values[i]) > 0)
                {
                    result = values[i];
                }

            }
            return result;
        }

        public static string LocalApplicationDataFolder(string moduleName = "Settings")
        {
            var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{((App.Attribs != null) ? App.Attribs.AssemblyTitle : System.Reflection.Assembly.GetExecutingAssembly().GetName().Name)}\\{moduleName}");
            return result;
        }

        public static bool? IsLocalhost(string host)
        {
            if (string.IsNullOrEmpty(host)) { return null; }
            try
            {
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                return hostIPs.Any(ip => IPAddress.IsLoopback(ip) || localIPs.Contains(ip));
            }
            catch (Exception) { return null; }
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    System.ComponentModel.DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute)) as System.ComponentModel.DescriptionAttribute;
                    if (attr != null)
                        return attr.Description;
                }
            }
            return value.ToString();
        }
        #endregion
    }
}
