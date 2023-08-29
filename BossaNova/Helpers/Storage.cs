using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Tasks.Show.Models;

namespace Tasks.Show.Helpers
{
    public static class Storage
    {
        private const string c_fileName = "TodoShowTasks.xml";
        public static string TasksDataFolder
        {
            get
            {
                string applicationDataFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string tasksDataFolder = Path.Combine(applicationDataFolder, "Tasks.Show");

                // make sure folder exists
                if (!Directory.Exists(tasksDataFolder))
                    Directory.CreateDirectory(tasksDataFolder);

                return tasksDataFolder;
            }
        }

        public static TaskData Load()
        {
            string dataFile = getPath();

            if (File.Exists(dataFile))
            {
                try
                {
                    // Attempt a backup if enough time has passed.
                    if (File.Exists($"{dataFile}.backup"))
                    {
                        FileInfo fi = new FileInfo($"{dataFile}.backup");
                        if (MoreThanOneDay(fi.LastWriteTime, DateTime.Now))
                            File.Copy(dataFile, $"{dataFile}.backup", true);
                    }
                    else // No backup, so create one.
                    {
                        File.Copy(dataFile, $"{dataFile}.backup", true);
                    }

                    // Load and return TaskData.
                    using (FileStream fs = new FileStream(dataFile, FileMode.Open))
                    {
                        return XmlHelper.Load(XmlReader.Create(fs));
                    }
                }
                catch (XmlException e)
                {
                    Debug.WriteLine(e);
                    App.Logger.WriteLine(e.Message, LogLevel.Warning);
                    backupFile();
                }
                catch (IOException e)
                {
                    Debug.WriteLine(e);
                    App.Logger.WriteLine(e.Message, LogLevel.Warning);
                    backupFile();
                }
                catch (NullReferenceException e)
                {
                    Debug.WriteLine(e);
                    App.Logger.WriteLine(e.Message, LogLevel.Warning);
                    backupFile();
                }
            }

            return new TaskData();
        }

        public static void Save(TaskData data)
        {
            using (FileStream fs = new FileStream(getPath(), FileMode.Create))
            {
                var writer = XmlWriter.Create(fs, new XmlWriterSettings() { Indent = true });
                XmlHelper.Save(data, writer);
                writer.Flush();
            }
        }

        static void backupFile()
        {
            var newName = string.Format("{0}.{1}.backup", c_fileName, DateTime.Now.ToFileTime());
            newName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), newName);

            File.Move(getPath(), newName);
        }

        static string getPath()
        {
            return Path.Combine(TasksDataFolder, c_fileName);
        }

        /// <summary>
        /// Uses <see cref="Math.Abs"/> in the event that the dates are reversed.
        /// </summary>
        /// <param name="date1"><see cref="DateTime"/></param>
        /// <param name="date2"><see cref="DateTime"/></param>
        /// <returns>true if the diff is more than 24 hours, false otherwise</returns>
        static bool MoreThanOneDay(DateTime date1, DateTime date2)
        {
            TimeSpan difference = date1 - date2;
            return Math.Abs(difference.TotalDays) > 1.0;
        }
    }
}
