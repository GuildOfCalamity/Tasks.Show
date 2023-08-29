using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using PixelLab.Common;
using Tasks.Show.Models;
using Tasks.Show.Helpers;
using System.Windows;
using System.Windows.Media;

namespace Tasks.Show.ViewModels
{
    public class TaskListViewModel : INotifyPropertyChanged
    {
        #region Fields

        public event PropertyChangedEventHandler PropertyChanged;
        private EditTask m_newTask;
        private readonly CommandWrapper m_newTaskCommand;
        private readonly CommandWrapper m_cancelNewTaskCommand;
        private readonly CommandWrapper<Task> m_deleteTaskCommand;
        private readonly Func<Task, bool> m_filter;
        private readonly TaskData m_taskList;
        private readonly ObservableCollectionPlus<TaskViewModel> m_unfilteredTaskList;
        public readonly System.Windows.Threading.DispatcherTimer m_tmrCheckup = null;
        public static int m_totalBubbles = 0; // Checked at exit (these dialogs are not modal).
        #endregion Fields


        #region Constructors

        public TaskListViewModel(TaskData taskList, Func<Task, bool> filter)
        {
            Util.RequireNotNull(taskList, "taskList");
            m_taskList = taskList;
            ((INotifyCollectionChanged)m_taskList.Tasks).CollectionChanged += (sender, args) => RefreshFilter();

            Util.RequireNotNull(filter, "filter");
            m_filter = filter;

            m_unfilteredTaskList = new ObservableCollectionPlus<TaskViewModel>();
            m_taskList.Tasks.ForEach(t => m_unfilteredTaskList.Add(new TaskViewModel(t)));

            // Tasks
            m_newTaskCommand = new CommandWrapper(ShowNewTask, () => m_newTask == null);
            m_cancelNewTaskCommand = new CommandWrapper(() => CancelNewTask(), () => m_newTask != null);
            m_deleteTaskCommand = new CommandWrapper<Task>(task => DeleteTask(task), task => true);

            // Timer
            m_tmrCheckup = new System.Windows.Threading.DispatcherTimer();
            m_tmrCheckup.Interval = TimeSpan.FromSeconds(19);
            m_tmrCheckup.Tick += OnTimerTick;
            m_tmrCheckup.Start();
        }

        #endregion Constructors


        #region Properties

        public ReadOnlyObservableCollection<TaskViewModel> AllTasks
        {
            get { return m_unfilteredTaskList.ReadOnly; }
        }

        public ICommand CancelNewCommand { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[CancelNewCommand]");
                return m_cancelNewTaskCommand.Command; 
            } 
        }

        public ICommand DeleteTaskCommand { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteTaskCommand]");
                return m_deleteTaskCommand.Command; 
            } 
        }

        public EditTask NewTask
        {
            get { return m_newTask; }
            private set
            {
                if (m_newTask != value)
                {
                    if (m_newTask != null)
                    {
                        m_newTask.Committed -= NewTaskCommitted;
                    }
                    m_newTask = value;
                    if (m_newTask != null)
                    {
                        m_newTask.Committed += NewTaskCommitted;
                    }

                    m_newTaskCommand.UpdateCanExecute();
                    m_cancelNewTaskCommand.UpdateCanExecute();

                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(NewTask)));
                }
            }
        }

        public ICommand NewTaskCommand
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"[NewTaskCommand]");
                return m_newTaskCommand.Command;
            }
        }

        #endregion Properties


        #region Event Handlers

        private void NewTaskCommitted(object sender, EventArgs e)
        {
            Debug.Assert(m_newTask != null);

            var newTask = m_taskList.AddTask(m_newTask.Task, App.Root.FolderColorOptions);
            m_unfilteredTaskList.Add(new TaskViewModel(newTask));
            m_taskList.CurrentFolder = newTask.EffectiveFolder;

            CancelNewTask();
        }

        /// <summary>
        /// <see cref="System.Windows.Threading.DispatcherTimer"/> event.
        /// We're showing the user any upcomming tasks that are within a time range.
        /// </summary>
        void OnTimerTick(object sender, EventArgs e)
        {
            if (App.Root.WindowHost.IsClosing)
            {
                m_tmrCheckup.Stop();
                return;
            }

            try
            {   // After initial popups, notify every 2 hours.
                m_tmrCheckup.Interval = TimeSpan.FromMinutes(120);

                m_taskList.Tasks.ForEach(t =>
                {
                    if (!t.IsComplete && t.Due != null && t.Due.WithinOneDayOrPast(DateTime.Now))
                    {
                        System.Threading.Tasks.Task.Run(async () =>
                        {
                            // We'll need to run this on the UI thread...
                            await Application.Current.Dispatcher.InvokeAsync(() => {
                                ShowBubble($"{t.Description}", t, timer: 8, owner: App.Root.WindowHost);
                                System.Threading.Thread.Sleep(280);
                            }, System.Windows.Threading.DispatcherPriority.Background);
                            //await System.Threading.Tasks.Task.Delay(300);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine($"{ex.Message}", LogLevel.Warning);
            }
        }
        #endregion Event Handlers


        #region Private Methods
        /// <summary>
        /// Possibly turn this into a toast notification?
        /// </summary>
        void ShowBubble(string message, Task task, string title = "", int timer = 0, Window owner = null)
        {
            // Don't go crazy with the bubble dialogs.
            if (m_totalBubbles >= 3)
                return;
            else
                Debug.WriteLine($"Too many popups, {m_totalBubbles} are currenly active.");

            // Configure the dialog window...
            DialogBubble bubble = new DialogBubble(message, task, title, timer);
            ImageSource winIcon = GetBitmapFrame(@"pack://application:,,/Icons/popup.ico");
            if (winIcon != null) { bubble.Icon = winIcon; }
            if (owner != null) { bubble.Owner = owner; }
            bubble.Topmost = true;
            bubble.ShowActivated = true;
            if (owner == null) { bubble.WindowStartupLocation = WindowStartupLocation.CenterOwner; }
            else if (owner.WindowState != WindowState.Maximized && owner.WindowState != WindowState.Minimized)
            {
                // Bottom area of host window...
                bubble.Top = owner.Top + (owner.ActualHeight - 300);
                bubble.Left = owner.Left + ExtensionUtils.Rnd.Next(100, Math.Abs((int)owner.ActualWidth - 300));
            }
            else if (owner.WindowState == WindowState.Minimized)
            {
                Debug.WriteLine($"Window minimized, skipping X/Y adjustments.");
            }

            // Keep track of total bubbles...
            bubble.Loaded += (s, e) => { m_totalBubbles++; };
            bubble.Closing += (s, e) => { m_totalBubbles--; };

            // Did the user click on the title button?
            bubble.OnFolderRequest += (name) =>
            {
                // find requested folder
                BaseFolder theFolder = m_taskList.AllFolders.Where(folder => folder.Name.EasyEquals(name)).FirstOrDefault();
                if (theFolder != null)
                {
                    System.Threading.Thread.Sleep(200);
                    m_taskList.CurrentFolder = theFolder;
                }
                // Force close
                bubble.Close();

                if (owner != null)
                {
                    // Add option to bring main window to foreground.
                    owner.Activate();
                    // We should have a MainWindow by this point.
                    owner.Focus();
                }
            };

            // Render the window to screen.
            bubble.Show();
        }

        /// <summary>
        /// Icon image helper method.
        /// </summary>
        /// <param name="UriPath">a pack path, e.g. @"pack://application:,,/Icons/todo.ico"</param>
        /// <returns><see cref="System.Windows.Media.Imaging.BitmapFrame"/> from a URI path</returns>
        static System.Windows.Media.Imaging.BitmapFrame GetBitmapFrame(string UriPath)
        {
            try
            {
                System.Windows.Media.Imaging.IconBitmapDecoder ibd = new System.Windows.Media.Imaging.IconBitmapDecoder(
                        new Uri(UriPath, UriKind.RelativeOrAbsolute),
                        System.Windows.Media.Imaging.BitmapCreateOptions.None,
                        System.Windows.Media.Imaging.BitmapCacheOption.Default);
                return ibd.Frames[0];
            }
            catch (System.IO.FileFormatException fex)
            {
                App.Logger.WriteLine(fex.Message, LogLevel.Warning);
                return null;
            }
        }

        #endregion


        #region Public Methods

        public void CancelNewTask()
        {
            NewTask = null;
        }

        public void DeleteTask(Task task)
        {
            TaskViewModel tvm = m_unfilteredTaskList.First(t => t.Task == task);
            m_unfilteredTaskList.Remove(tvm);

            m_taskList.RemoveTask(task);
        }

        public void MoveTask(Task item, Task toItem)
        {
            var itemIndex = m_taskList.Tasks.IndexOf(item);
            var toIndex = m_taskList.Tasks.IndexOf(toItem);
            m_taskList.ReorderTasks(item, toItem);
            m_unfilteredTaskList.Move(itemIndex, toIndex);
        }

        public void RefreshFilter()
        {
            m_unfilteredTaskList.ForEach(tm => tm.IsVisible = m_filter(tm.Task));
        }

        public void ShowNewTask()
        {
            if (m_newTask == null)
            {
                NewTask = new EditTask();
            }
        }

        #endregion Public Methods


        #region Protected Methods

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion Protected Methods
    }
}
