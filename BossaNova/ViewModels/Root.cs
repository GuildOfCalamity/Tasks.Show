using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;
using Tasks.Show.Helpers;
using Tasks.Show.Models;
using PixelLab.Common;
using System.Collections.ObjectModel;

namespace Tasks.Show.ViewModels
{
    /// <summary>
    /// This is our main/core view model with which each subsequent view model can access.
    /// </summary>
    public class Root : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        private readonly TaskData m_taskData;
        private readonly ReadOnlyCollection<Color> m_folderColorOptions;

        #endregion Fields

        #region Constructors

        public Root(TaskData taskData, IEnumerable<Color> folderColorOptions)
        {
            Util.RequireNotNull(taskData, "taskData");
            m_taskData = taskData;

            Util.RequireNotNull(folderColorOptions, "folderColorOptions");
            m_folderColorOptions = folderColorOptions.ToReadOnlyCollection();

            Tasks = new TaskListViewModel(taskData, filter);
            Timeline = new TimelineViewModel(Tasks.AllTasks);

            Filters = new Filters(taskData);
            Folders = new Folders(taskData);

            taskData.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "CurrentFolder" || args.PropertyName == "Filter")
                {
                    Tasks.RefreshFilter();
                }
            };

            DispatcherTimer dateChanger = new DispatcherTimer();
            dateChanger.Interval = TimeSpan.FromMinutes(2);
            dateChanger.Tick += new EventHandler(dateChanger_Tick);
            dateChanger.Start();
            Tasks.RefreshFilter();
        }

        #endregion Constructors

        #region Properties

        public TaskData TaskData
        { 
            get 
            {
                if (m_taskData != null)
                    return m_taskData;
                else
                    return new TaskData();
            } 
        }

        public IList<Color> FolderColorOptions { get => m_folderColorOptions; }

        public Filters Filters { get; private set; }

        public Folders Folders { get; private set; }

        public TaskListViewModel Tasks { get; private set; }

        public TimelineViewModel Timeline { get; private set; }

        public DateTime Now { get => DateTime.Now; }

        public DateTime Today { get => DateTime.Today; }

        public MainWindow WindowHost { get; set; }

        #region [only for testing visibility triggers]
        private bool m_popupsEnabled = true;
        public bool PopupsEnabled
        {
            get => m_popupsEnabled;
            set
            {
                if (m_popupsEnabled != value)
                {
                    m_popupsEnabled = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(PopupsEnabled)));
                }
            }
        }
        #endregion

        #region [only for drop down user update]
        private string m_popupStatus = "Disable Popup Reminders"; // default is enabled
        public string PopupStatus
        { 
            get => m_popupStatus;
            set
            {
                if (m_popupStatus != value)
                {
                    m_popupStatus = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(PopupStatus)));
                }
            }
        }
        #endregion

        #endregion Properties

        #region Event Handlers

        void dateChanger_Tick(object sender, EventArgs e)
        {
            var handler = PropertyChanged;
            
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(nameof(Today)));
                handler(this, new PropertyChangedEventArgs(nameof(Now)));
            }
        }

        #endregion Event Handlers

        #region Private Methods

        private bool filter(Task task)
        {
            return Filters.InCurrent(task) && TaskData.CurrentFolder.ContainsTask(task);
        }

        #endregion Private Methods

        #region Protected Methods

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, e); }
        }

        #endregion Protected Methods
    }
}
