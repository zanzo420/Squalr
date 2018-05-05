﻿namespace Squalr.Source.Tasks
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine;
    using Squalr.Engine.Snapshots;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Class to schedule tasks that are executed.
    /// </summary>
    public class TaskTrackerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="TaskTrackerViewModel" /> class.
        /// </summary>
        private static Lazy<TaskTrackerViewModel> actionSchedulerViewModelInstance = new Lazy<TaskTrackerViewModel>(
            () => { return new TaskTrackerViewModel(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        private FullyObservableCollection<TrackableTask<Snapshot>> trackedTasks;

        /// <summary>
        /// Prevents a default instance of the <see cref="TaskTrackerViewModel" /> class from being created.
        /// </summary>
        private TaskTrackerViewModel() : base("Task Tracker")
        {
            this.trackedTasks = new FullyObservableCollection<TrackableTask<Snapshot>>();

            this.CancelTaskCommand = new RelayCommand<TrackableTask<Snapshot>>(task => task.Cancel(), (task) => true);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TaskTrackerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static TaskTrackerViewModel GetInstance()
        {
            return TaskTrackerViewModel.actionSchedulerViewModelInstance.Value;
        }

        /// <summary>
        /// Gets a command to cancel a running task.
        /// </summary>
        public ICommand CancelTaskCommand { get; private set; }

        /// <summary>
        /// Gets the tasks that are actively running.
        /// </summary>
        public FullyObservableCollection<TrackableTask<Snapshot>> TrackedTasks
        {
            get
            {
                return this.trackedTasks;
            }
        }

        /// <summary>
        /// Tracks a given task until it is canceled or completed.
        /// </summary>
        /// <param name="task">The task to track.</param>
        public void TrackTask(TrackableTask<Snapshot> task)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                task.OnCanceledEvent += this.RemoveTask;
                task.OnCompletedEvent += this.RemoveTask;
                this.TrackedTasks.Add(task);
            }));
        }

        /// <summary>
        /// Removes a tracked task from the list of tracked tasks.
        /// </summary>
        /// <param name="task">The task to remove.</param>
        private void RemoveTask(TrackableTask<Snapshot> task)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (this.TrackedTasks.Contains(task))
                {
                    this.TrackedTasks.Remove(task);
                }
            }));
        }
    }
    //// End class
}
//// End namespace