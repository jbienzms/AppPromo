using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
#endif

namespace AppPromo
{
    internal static class PlatformHelper
    {
        static public async Task<bool> AskOkCancel(string message, string title)
        {
            #if WINDOWS_PHONE
            var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
            #endif
        }

        static public bool HasSetting(string key)
        {
            #if WINDOWS_PHONE
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
            #endif
        }

        static public string ReadResourceString(string key)
        {
            #if WINDOWS_PHONE
            return Resources.ResourceManager.GetString(key);
            #endif
        }

        static public T ReadSetting<T>(string key)
        {
            #if WINDOWS_PHONE
            return (T)IsolatedStorageSettings.ApplicationSettings[key];
            #endif
        }

        static public T ReadSetting<T>(string key, T defaultValue)
        {
            #if WINDOWS_PHONE
            if (HasSetting(key))
            {
                return (T)IsolatedStorageSettings.ApplicationSettings[key];
            }
            else
            {
                return defaultValue;
            }
            #endif
        }

        static public bool RemoveSetting(string key)
        {
            #if WINDOWS_PHONE
            var removed = IsolatedStorageSettings.ApplicationSettings.Remove(key);
            IsolatedStorageSettings.ApplicationSettings.Save();
            return removed;
            #endif
        }

        static public async Task ShowRatingUI()
        {
            #if WINDOWS_PHONE
            var marketplaceReviewTask = new MarketplaceReviewTask(); 
            marketplaceReviewTask.Show(); 
            #endif
        }

        static public void WriteSetting<T>(string key, T value)
        {
            #if WINDOWS_PHONE
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
            #endif
        }

        static public bool IsInDesignMode
        {
            get
            {
                #if WINDOWS_PHONE
                return DesignerProperties.IsInDesignTool;
                #endif
            }
        }
    }

    /// <summary>
    /// Provides results for a rating reminder.
    /// </summary>
    public class RateReminderResult
    {
        /// <summary>
        /// Initializes a new <see cref="RateReminderResult"/>.
        /// </summary>
        /// <param name="days">
        /// The number of days that have passed since the app was installed.
        /// </param>
        /// <param name="runs">
        /// The number of times the application has been run since it was installed.
        /// </param>
        /// <param name="reminderShown">
        /// A value that indicates if a reminder was shown.
        /// </param>
        /// <param name="ratingShown">
        /// A value that indicates if the rating interface was shown.
        /// </param>
        public RateReminderResult(int days, int runs, bool reminderShown, bool ratingShown)
        {
            Days = days;
            ReminderShown = reminderShown;
            RatingShown = ratingShown;
            Runs = runs;
        }

        /// <summary>
        /// Gets the number of days that have passed since the app was installed.
        /// </summary>
        /// <value>
        /// The number of days that have passed since the app was installed.
        /// </value>
        /// <remarks>
        /// This count is only calculated if the days reminder is enabled and hasn't already been shown. The count can be reset by calling 
        /// the <see cref="ResetCounters">RateReminder.ResetCounters</see> method of the <see cref="RateReminder"/> control.
        /// </remarks>
        public int Days { get; private set; }

        /// <summary>
        /// Gets a value that indicates if a reminder was shown.
        /// </summary>
        public bool ReminderShown { get; private set; }

        /// <summary>
        /// Gets a value that indicates if the rating interface was shown.
        /// </summary>
        public bool RatingShown { get; private set; }

        /// <summary>
        /// Gets the number of times the application has been run since it was installed.
        /// </summary>
        /// <value>
        /// The number of times the application has been run since it was installed.
        /// </value>
        /// <remarks>
        /// This count is only calculated if the runs reminder is enabled and hasn't already been shown. The count can be reset by calling 
        /// the <see cref="ResetCounters">RateReminder.ResetCounters</see> method of the <see cref="RateReminder"/> control.
        /// </remarks>
        public int Runs { get; private set; }
    }

    public class RateReminder : Control
    {
        #region Constants
        private const string FIRST_RUN_KEY = "RateFirstRun";
        private const string MESSAGE_KEY = "RateAppPrompt";
        private const string RUNS_COUNT_KEY = "RateRunsCount";
        private const string SHOWN_FOR_DAYS_KEY = "RateShownForDays";
        private const string SHOWN_FOR_RUNS_KEY = "RateShownForRuns";
        #endregion // Constants

        #region Member Variables
        private int runs;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="RateReminder"/> instance.
        /// </summary>
        public RateReminder()
        {
            // Defaults
            RunsBeforeReminder = 7;
            TryReminderOnLoad = true;

            // Handle loaded
            this.Loaded += Control_Loaded;
        }
        #endregion // Constructors

        #region Internal Methods
        /// <summary>
        /// Gets the number of days since the application was first run.
        /// </summary>
        /// <returns>
        /// The number of days since the application was first run.
        /// </returns>
        private int GetDays()
        {
            int days = 0;
            try
            {
                if (PlatformHelper.HasSetting(FIRST_RUN_KEY))
                {
                    var firstRun = PlatformHelper.ReadSetting<DateTime>(FIRST_RUN_KEY);
                    days = Convert.ToInt32((DateTime.Now - firstRun).TotalDays);
                }
                else
                {
                    PlatformHelper.WriteSetting<DateTime>(FIRST_RUN_KEY, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Unable to get rating days count. {0}", ex.Message));
            }
            return days;
        }

        /// <summary>
        /// Gets the number of application runs, incrementing the count if this is the first request in this run.
        /// </summary>
        /// <returns>
        /// The number of application runs.
        /// </returns>
        private int GetRuns()
        {
            // If we've already read this value, just return it instead of incrementing again.
            if (runs > 0) { return runs; }

            try
            {
                runs = PlatformHelper.ReadSetting<int>(RUNS_COUNT_KEY, 0);
                runs++;
                PlatformHelper.WriteSetting<int>(RUNS_COUNT_KEY, runs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Unable to get or increment rating run count. {0}", ex.Message));
            }
            return runs;
        }

        /// <summary>
        /// Shows the reminder and indicates if the rating interface was displayed.
        /// </summary>
        /// <returns>
        /// A task that yeilds the resut, <c>true</c> if the rating interface was shown; otherwise <c>false</c>.
        /// </returns>
        private async Task<bool> ShowReminder()
        {
            // What message do we use?
            string msg = (!string.IsNullOrEmpty(CustomReminderText) ? CustomReminderText : PlatformHelper.ReadResourceString(MESSAGE_KEY));

            // Show the message
            var ok = await PlatformHelper.AskOkCancel(msg, "");

            // If it's OK to, show the rating window
            if (ok)
            {
                await PlatformHelper.ShowRatingUI();
            }

            return ok;
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
        protected virtual void OnTryReminderCompleted(RateReminderResult e)
        {
            if (TryReminderCompleted != null) { TryReminderCompleted(this, e); }
        }
        #endregion // Overridables / Event Triggers

        #region Overrides / Event Handlers
        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            // Only attempt reminder if runtime, not design time
            if (!PlatformHelper.IsInDesignMode)
            {
                // Try to show reminder?
                if (TryReminderOnLoad)
                {
                    var t = await TryReminder();
                }
            }
        }
        #endregion // Overrides / Event Handlers

        #region Public Methods
        /// <summary>
        /// Resets the reminder counters and whether or not reminders have been shown.
        /// </summary>
        public void ResetCounters()
        {
            PlatformHelper.RemoveSetting(FIRST_RUN_KEY);
            PlatformHelper.RemoveSetting(RUNS_COUNT_KEY);
            PlatformHelper.RemoveSetting(SHOWN_FOR_RUNS_KEY);
            PlatformHelper.RemoveSetting(SHOWN_FOR_DAYS_KEY);
        }

        /// <summary>
        /// Checks to see whether it's time to show a reminder and if so, shows it.
        /// </summary>
        /// <returns>
        /// A task that yields the result, a <see cref="RateReminderResult"/>. 
        /// </returns>
        public async Task<RateReminderResult> TryReminder()
        {
            int runs = 0;
            int days = 0;
            bool reminderShown = false;
            bool ratingShown = false;

            // If the runs reminder is enabled and has not been shown, see if we need to show it
            if ((RunsBeforeReminder > 0) && (!PlatformHelper.ReadSetting<bool>(SHOWN_FOR_RUNS_KEY, false)))
            {
                // How many runs so far?
                runs = GetRuns();

                // Have we met the threshold?
                if (runs >= RunsBeforeReminder)
                {
                    // Show the reminder
                    ratingShown = await ShowReminder();

                    // Mark that it's been shown
                    reminderShown = true;
                    PlatformHelper.WriteSetting<bool>(SHOWN_FOR_RUNS_KEY, true);
                }
            }

            // If no reminder has been shown, and if the days reminder is enabled but hasn't been shown, see if we need to show it
            if ((!reminderShown) && (DaysBeforeReminder > 0) && (!PlatformHelper.ReadSetting<bool>(SHOWN_FOR_DAYS_KEY, false)))
            {
                // How many days so far?
                days = GetDays();

                // Have we met the threshold?
                if (days >= DaysBeforeReminder)
                {
                    // Show the reminder
                    ratingShown = await ShowReminder();

                    // Mark that it's been shown
                    reminderShown = true;
                    PlatformHelper.WriteSetting<bool>(SHOWN_FOR_DAYS_KEY, true);
                }
            }

            // Create result
            var result = new RateReminderResult(days, runs, reminderShown, ratingShown);

            // Notify
            OnTryReminderCompleted(result);

            // Return result
            return result;
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the customized reminder message to display to the user.
        /// </summary>
        /// <value>
        /// The customized reminder message to display to the user. The default is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Setting this property to <c>null</c> (the default) will cause the 
        /// included localized message to be displayed.
        /// </remarks>
        [DefaultValue(null)]
        public string CustomReminderText { get; set; }

        /// <summary>
        /// Gets or sets the number of days before the reminder will be displayed.
        /// </summary>
        /// <value>
        /// The number of days before the reminder will be displayed. The default is zero.
        /// </value>
        /// <remarks>
        /// Setting this property to zero (the default) will disable a reminder by days.
        /// </remarks>
        [DefaultValue(0)]
        public int DaysBeforeReminder { get; set; }

        /// <summary>
        /// Gets or sets the number of application runs before the reminder will be displayed.
        /// </summary>
        /// <value>
        /// The number of application runs before the reminder will be displayed. The default is 7.
        /// </value>
        /// <remarks>
        /// Setting this property to zero will disable a reminder by days.
        /// </remarks>
        [DefaultValue(7)]
        public int RunsBeforeReminder { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the control should try to show the reminer as soon as it's loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if the control should try to show the reminer as soon as it's loaded; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        /// <remarks>
        /// If this member is set to false you must call <see cref="TryReminder"/> to show the reminder.
        /// </remarks>
        [DefaultValue(true)]
        public bool TryReminderOnLoad { get; set; }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Occurs when the <see cref="TryReminder"/> method has completed.
        /// </summary>
        public event EventHandler<RateReminderResult> TryReminderCompleted;
        #endregion // Public Events
    }
}
