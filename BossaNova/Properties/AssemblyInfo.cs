using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Reflection;
/*
 * [slc] I've made various improvements to the original code from 2010:
 *  - Took framework version from 3.5 to 4.6.2
 *  - Added an ILogger to the application.
 *  - Added better time period selections.
 *  - Added timer to auto-save tasks in the event of a crash.
 *  - Added proper data backup methods.
 *  - Added handler for the Dispatcher's UnhandledException (rare dictionary exceptions were causing data loss).
 *  - Added error catching to potential failure points.
 *  - Added custom popup dialog for alerting user to upcomming tasks.
 *  - Added ability for popups to route to desired task folder if title is clicked.
 *  - Added memory effecient stopwatch for timing routines.
 *  - Added more folder colors and adjusted pallete to be more "nouveau".
 *  - Added missing verbs to DateParser.
 *  - Added animation helpers and blur effects.
 *  - Added OS version checking helper.
 *  - Added middle mouse button check for routing web urls in text blocks.
 *  - Added dark title bar.
 *  - Made the timeline control default to the recent month range.
 *  - Moved task data to local application folder so they are portable.
 *  - Removed the useless "Inbox" special folder to reclaim UI real estate.
 *  - Updated the visual styles (tuned contrast to fit a more dark-theme-esque).
 *  - Code cleanup/modernize and additional comments added.
 */
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.SourceAssembly, ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyTitleAttribute("Tasks.Show")]
[assembly: AssemblyCompanyAttribute("Chamware")]
[assembly: AssemblyProductAttribute("Tasks.Show")]
[assembly: AssemblyCopyrightAttribute("Copyright © Chamware 2022-2023")]
[assembly: AssemblyVersionAttribute("1.0.1.5")]
[assembly: AssemblyFileVersionAttribute("1.0.1.5")]
