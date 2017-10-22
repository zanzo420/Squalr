﻿namespace SqualrClient.Source.Main
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using SqualrClient.Properties;
    using SqualrCore.Source.Analytics;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Engine.AddressResolver;
    using SqualrCore.Source.Engine.AddressResolver.DotNet;
    using SqualrCore.Source.Output;
    using System;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Xceed.Wpf.AvalonDock;

    /// <summary>
    /// Main view model.
    /// Note: There are several MVVM responsability violations in this class, but these are isolated and acceptable.
    /// </summary>
    internal class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Default layout file for browsing cheats.
        /// </summary>
        private const String DefaultLayoutResource = "SqualrClient.Content.DefaultLayout.xml";

        /// <summary>
        /// The save file for the docking layout.
        /// </summary>
        private const String LayoutSaveFile = "layout.xml";

        /// <summary>
        /// The developer tools executable.
        /// </summary>
        private const String DeveloperToolsExecutable = "Squalr.exe";

        /// <summary>
        /// Singleton instance of the <see cref="MainViewModel" /> class
        /// </summary>
        private static Lazy<MainViewModel> mainViewModelInstance = new Lazy<MainViewModel>(
                () => { return new MainViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="MainViewModel" /> class from being created.
        /// </summary>
        private MainViewModel()
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Squalr started");

            // Note: These cannot be async, as the logic to update the layout or window cannot be on a new thread
            this.DisplayChangeLogCommand = new RelayCommand(() => this.DisplayChangeLog(), () => true);
            this.CloseCommand = new RelayCommand<Window>((window) => this.Close(window), (window) => true);
            this.MaximizeRestoreCommand = new RelayCommand<Window>((window) => this.MaximizeRestore(window), (window) => true);
            this.MinimizeCommand = new RelayCommand<Window>((window) => this.Minimize(window), (window) => true);

            this.ResetLayoutCommand = new RelayCommand<DockingManager>((dockingManager)
                => DockingViewModel.GetInstance().LoadLayoutFromResource(dockingManager, MainViewModel.DefaultLayoutResource), (dockingManager) => true);
            this.LoadLayoutCommand = new RelayCommand<DockingManager>((dockingManager)
                => DockingViewModel.GetInstance().LoadLayoutFromFile(dockingManager, MainViewModel.LayoutSaveFile, MainViewModel.DefaultLayoutResource), (dockingManager) => true);
            this.SaveLayoutCommand = new RelayCommand<DockingManager>((dockingManager)
                => DockingViewModel.GetInstance().SaveLayout(dockingManager, MainViewModel.LayoutSaveFile), (dockingManager) => true);

            this.LaunchDeveloperToolsCommand = new RelayCommand(() => this.LaunchDeveloperTools(asAdmin: false), () => true);
            this.LaunchDeveloperToolsAsAdminCommand = new RelayCommand(() => this.LaunchDeveloperTools(asAdmin: true), () => true);

            Task.Run(() => this.StartBackgroundServices());
        }

        /// <summary>
        /// Gets a command to launch the developer tools.
        /// </summary>
        public ICommand LaunchDeveloperToolsCommand { get; private set; }

        /// <summary>
        /// Gets a command to launch the developer tools as admin.
        /// </summary>
        public ICommand LaunchDeveloperToolsAsAdminCommand { get; private set; }

        /// <summary>
        /// Gets the command to close the main window.
        /// </summary>
        public ICommand CloseCommand { get; private set; }

        /// <summary>
        /// Gets the command to maximize the main window.
        /// </summary>
        public ICommand MaximizeRestoreCommand { get; private set; }

        /// <summary>
        /// Gets the command to minimize the main window.
        /// </summary>
        public ICommand MinimizeCommand { get; private set; }

        /// <summary>
        /// Gets the command to reset the current docking layout to the default.
        /// </summary>
        public ICommand ResetLayoutCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the change log.
        /// </summary>
        public ICommand DisplayChangeLogCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the current docking layout.
        /// </summary>
        public ICommand LoadLayoutCommand { get; private set; }

        /// <summary>
        /// Gets the command to save the current docking layout.
        /// </summary>
        public ICommand SaveLayoutCommand { get; private set; }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="MainViewModel" /> class.</returns>
        public static MainViewModel GetInstance()
        {
            return mainViewModelInstance.Value;
        }

        /// <summary>
        /// Starts useful services that run in the background to assist in various operations.
        /// </summary>
        private void StartBackgroundServices()
        {
            DotNetObjectCollector.GetInstance().Schedule();
            AddressResolver.GetInstance().Schedule();
            AnalyticsService.GetInstance().Start();

            AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, "Start");
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Background services started");
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        /// <param name="window">The window to close.</param>
        private void Close(Window window)
        {
            SettingsViewModel.GetInstance().Save();

            window.Close();
        }

        /// <summary>
        /// Maximizes or Restores the main window.
        /// </summary>
        /// <param name="window">The window to maximize or restore.</param>
        private void MaximizeRestore(Window window)
        {
            if (window == null)
            {
                return;
            }

            if (window.WindowState != WindowState.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// Minimizes the main window.
        /// </summary>
        /// <param name="window">The window to minimize.</param>
        private void Minimize(Window window)
        {
            window.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Displays the change log to the user if there has been a recent update.
        /// </summary>
        private void DisplayChangeLog()
        {
            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed || !ApplicationDeployment.CurrentDeployment.IsFirstRun)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Error displaying change log", ex);
                return;
            }

            // View.ChangeLog changeLog = new View.ChangeLog();
            // changeLog.Owner = Application.Current.MainWindow;
            // changeLog.ShowDialog();
        }

        /// <summary>
        /// Launches the developer tools.
        /// </summary>
        /// <param name="asAdmin">A value indicating whether the tools should be launched as admin.</param>
        private void LaunchDeveloperTools(Boolean asAdmin = false)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), MainViewModel.DeveloperToolsExecutable));
                processInfo.UseShellExecute = true;

                if (asAdmin)
                {
                    processInfo.Verb = "runas";
                }

                Process.Start(processInfo);

                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Launching developer tools...");
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error staring developer tools", ex);
            }
        }
    }
    //// End class
}
//// End namesapce