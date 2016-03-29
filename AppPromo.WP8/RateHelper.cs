#region License
/******************************************************************************
 * COPYRIGHT © MICROSOFT CORP. 
 * MICROSOFT LIMITED PERMISSIVE LICENSE (MS-LPL)
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 1. Definitions
 * The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same meaning here as under U.S. copyright law.
 * A “contribution” is the original software, or any additions or changes to the software.
 * A “contributor” is any person that distributes its contribution under this license.
 * “Licensed patents” are a contributor’s patent claims that read directly on its contribution.
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors’ name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
 * (F) Platform Limitation- The licenses granted in sections 2(A) & 2(B) extend only to the software or derivative works that you create that run on a Microsoft Windows operating system product.
 ******************************************************************************/
#endregion // License

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Windows;
#endif

#if WIN_RT
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.ApplicationModel.Store;
#endif

#if WINDOWS_UWP
using AppPromo.UWP.Controls;
using Windows.UI.Xaml.Controls;
#endif

#if WINDOWS_UWP
namespace AppPromo.UWP
#else
namespace AppPromo
#endif
{
    /// <summary>
    /// The result of prompting the user.
    /// </summary>
    internal enum PromptResult
    {
        /// <summary>
        /// The user confirmed the action.
        /// </summary>
        Confirm,

        /// <summary>
        /// The user declined the action.
        /// </summary>
        Decline,

        /// <summary>
        /// The user delayed the action.
        /// </summary>
        Delay
    };

    internal static class PlatformHelper
    {
#if WIN_RT
        static private ResourceLoader resourceLoader;
#endif
#pragma warning disable 1998 // The async Task keeps the signature identical between platforms.
        static public async Task<PromptResult> PromptAsync(string message, string title)
        {

#if WINDOWS_UWP

            // First we will try the advanced AlertDialog route. However, mixing Xaml and WinJS is not supported.
            // Therefore we must have a fallback using basic MessageDialog for WinJS (below).
            try
            {
                ActionDialog dlg = new ActionDialog()
                {
                    ConfirmText = ReadResourceString("Confirm"),
                    DeclineText = ReadResourceString("Decline"),
                    DelayText = ReadResourceString("Delay"),
                    DontRemindAgainText = ReadResourceString("DontRemindAgain"),
                    PromptText = ReadResourceString("RateAppPrompt"),
                    Title = title,
                };

                var result = ContentDialogResult.None;

                try
                {
                    result = await dlg.ShowAsync();
                }
                catch (Exception ex)
                {
                    //	this may happen if any other modal window is shown at the moment (ie, Windows query about running application background task)
                    await new MessageDialog($"Show Error: {ex.Message}").ShowAsync();
                }

                // Convert to a PromptResult based on the state of the ActionDialog
                if (result == ContentDialogResult.Primary)
                {
                    return PromptResult.Confirm;
                }
                else if (dlg.DontRemindAgain)
                {
                    return PromptResult.Decline;
                }
                else
                {
                    return PromptResult.Delay;
                }
            }

            // If we are here, ActionDialog is not supported. We must use basic MessageDialog instead.
            catch (Exception) // No specific exception because it's HRESULT of RPC_E_WRONG_THREAD
            {
                // Use plain message dialog as with Win8 but use new string resource names
                var dlg = new MessageDialog(message, title);
                dlg.Commands.Add(new UICommand(ReadResourceString("Confirm"), null, true));
                dlg.Commands.Add(new UICommand(ReadResourceString("Decline"), null, false));

                var result = false;

                try
                {
                    result = (bool)(await dlg.ShowAsync()).Id;
                }
                catch (Exception ex)
                {
                    //	this may happen if any other modal window is shown at the moment (ie, Windows query about running application background task)
                    await new MessageDialog($"Show Error: {ex.Message}").ShowAsync();
                }

                // Convert to PromptResult
                return (result ? PromptResult.Confirm : PromptResult.Decline);
            }




#elif WIN_RT

            // Use plain message dialog
            var dlg = new MessageDialog(message, title);
            dlg.Commands.Add(new UICommand(ReadResourceString("OK"), null, true));
            dlg.Commands.Add(new UICommand(ReadResourceString("Cancel"), null, false));
            
			var result = false;

			try
			{
				result = (bool)(await dlg.ShowAsync()).Id;
			}
			catch (Exception)
			{
				//	this may happen if any other modal window is shown at the moment (ie, Windows query about running application background task)
			}

			return (result ? PromptResult.Confirm : PromptResult.Decline);



#elif WINDOWS_PHONE

            // Use plain message box
            var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            return return (result == MessageBoxResult.OK ? PromptResult.Confirm : PromptResult.Decline);
#endif
        }
#pragma warning restore 1998

        static public bool HasSetting(string key)
        {
#if WINDOWS_PHONE
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
#endif
#if WIN_RT
            return ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
#endif
        }

        static public string ReadResourceString(string key)
        {
#if WINDOWS_PHONE
            return Resources.ResourceManager.GetString(key);
#elif WINDOWS_UWP
            if (resourceLoader == null) { resourceLoader = new ResourceLoader("AppPromo.UWP/Resources"); }
            return resourceLoader.GetString(key);
#elif WIN_RT
            if (resourceLoader == null) { resourceLoader = new ResourceLoader("AppPromo/Resources"); }
            return resourceLoader.GetString(key);
#endif
        }

        static public T ReadSetting<T>(string key)
        {
#if WINDOWS_PHONE
            return (T)IsolatedStorageSettings.ApplicationSettings[key];
#endif
#if WIN_RT
            return (T)ApplicationData.Current.RoamingSettings.Values[key];
#endif
        }

        static public T ReadSetting<T>(string key, T defaultValue)
        {
            if (HasSetting(key))
            {
                return ReadSetting<T>(key);
            }
            else
            {
                return defaultValue;
            }
        }

        static public bool RemoveSetting(string key)
        {
#if WINDOWS_PHONE
            var removed = IsolatedStorageSettings.ApplicationSettings.Remove(key);
            IsolatedStorageSettings.ApplicationSettings.Save();
#endif

#if WIN_RT
            var removed = ApplicationData.Current.RoamingSettings.Values.Remove(key);
#endif

            return removed;
        }

#pragma warning disable 1998 // The async Task keeps the signature identical between platforms.
        static public async Task ShowRatingUI()
        {
#if WINDOWS_PHONE
            var marketplaceReviewTask = new MarketplaceReviewTask(); 
            marketplaceReviewTask.Show(); 
#endif
#if WIN_RT
            try
            {
                await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            }
            catch (Exception)
            {
                await new MessageDialog("There was a problem showing the rating dialog. This app may not be published to the store.").ShowAsync();
            }
#endif
        }
#pragma warning restore 1998

        static public void WriteSetting<T>(string key, T value)
        {
#if WINDOWS_PHONE
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
#endif
#if WIN_RT
            ApplicationData.Current.RoamingSettings.Values[key] = value;
#endif
        }

        static public bool IsInDesignMode
        {
            get
            {
#if WINDOWS_PHONE
                return DesignerProperties.IsInDesignTool;
#endif
#if WIN_RT
                return DesignMode.DesignModeEnabled;
#endif
            }
        }
    }

    /// <summary>
    /// Provides results for a rating reminder.
    /// </summary>
    public sealed class RateReminderResult
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
        /// Gets a value that indicates if a reminder was shown on this attempt.
        /// </summary>
        public bool ReminderShown { get; private set; }

        /// <summary>
        /// Gets a value that indicates if the user accepted the reminder and the rating interface was shown on this attempt.
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

    /// <summary>
    /// A utility class to assist with rating reminders.
    /// </summary>
    /// <remarks>
    /// This class can be used in code behind on Windows Phone and from any WinRT language including JavaScript.
    /// </remarks>
    public sealed class RateHelper
    {
#region Constants
        private const string MESSAGE_KEY = "RateAppPrompt";
#if WINDOWS_PHONE_APP
        private const string FIRST_RUN_KEY = "WP.RateFirstRun";
        private const string RUNS_COUNT_KEY = "WP.RateRunsCount";
        private const string SHOWN_FOR_DAYS_KEY = "WP.RateShownForDays";
        private const string SHOWN_FOR_RUNS_KEY = "WP.RateShownForRuns";
#elif WINDOWS_APP
        private const string FIRST_RUN_KEY = "W8.RateFirstRun";
        private const string RUNS_COUNT_KEY = "W8.RateRunsCount";
        private const string SHOWN_FOR_DAYS_KEY = "W8.RateShownForDays";
        private const string SHOWN_FOR_RUNS_KEY = "W8.RateShownForRuns";
#else
        private const string FIRST_RUN_KEY = "RateFirstRun";
        private const string RUNS_COUNT_KEY = "RateRunsCount";
        private const string SHOWN_FOR_DAYS_KEY = "RateShownForDays";
        private const string SHOWN_FOR_RUNS_KEY = "RateShownForRuns";
#endif
#endregion // Constants

#region Member Variables
        private int runs;
#endregion // Member Variables

#region Constructors
        /// <summary>
        /// Initializes a new <see cref="RateReminder"/> instance.
        /// </summary>
        public RateHelper()
        {
            // Defaults
            RunsBeforeReminder = 7;
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
                    var firstRun = DateTime.Parse(PlatformHelper.ReadSetting<string>(FIRST_RUN_KEY));
                    days = Convert.ToInt32((DateTime.Now - firstRun).TotalDays);
                }
                else
                {
                    PlatformHelper.WriteSetting<string>(FIRST_RUN_KEY, DateTime.Now.ToString());
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
        /// Shows the reminder and indicates the result.
        /// </summary>
        /// <returns>
        /// A task that yields the result.
        /// </returns>
        private async Task<PromptResult> ShowReminderAsync()
        {
            // What message do we use?
            string msg = (!string.IsNullOrEmpty(CustomReminderText) ? CustomReminderText : PlatformHelper.ReadResourceString(MESSAGE_KEY));

            // Show the message
            var result = await PlatformHelper.PromptAsync(msg, "");

            // If it's OK to, show the rating window
            if (result == PromptResult.Confirm)
            {
                await PlatformHelper.ShowRatingUI();
            }

            return result;
        }
#endregion // Internal Methods

#region Overridables / Event Triggers
        private void OnTryReminderCompleted(RateReminderResult e)
        {
            if (TryReminderCompleted != null) { TryReminderCompleted(this, e); }
        }
#endregion // Overridables / Event Triggers

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

#if WINDOWS_PHONE
        /// <summary>
        /// Checks to see whether it's time to show a reminder and if so, shows it.
        /// </summary>
        /// <returns>
        /// A task that yields the result, a <see cref="RateReminderResult"/>. 
        /// </returns>
        public async Task<RateReminderResult> TryReminderAsync()
#endif
#if WIN_RT
        internal async Task<RateReminderResult> InnerTryReminderAsync()
#endif
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
                    var runsResult = await ShowReminderAsync();
                    ratingShown = (runsResult == PromptResult.Confirm);

                    // Reminder has been shown
                    reminderShown = true;

                    // If not delayed until next time, mark as done
                    if (runsResult != PromptResult.Delay)
                    {
                        PlatformHelper.WriteSetting<bool>(SHOWN_FOR_RUNS_KEY, true);
                    }
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
                    var daysResult = await ShowReminderAsync();
                    ratingShown = (daysResult == PromptResult.Confirm);

                    // Reminder has been shown
                    reminderShown = true;

                    // If not delayed until next time, mark as done
                    if (daysResult != PromptResult.Delay)
                    {
                        PlatformHelper.WriteSetting<bool>(SHOWN_FOR_DAYS_KEY, true);
                    }
                }
            }

            // Create result
            var result = new RateReminderResult(days, runs, reminderShown, ratingShown);

            // Notify
            OnTryReminderCompleted(result);

            // Return result
            return result;
        }

#if WIN_RT
        /// <summary>
        /// Checks to see whether it's time to show a reminder and if so, shows it.
        /// </summary>
        /// <returns>
        /// An asychronous operation that yields the result, a <see cref="RateReminderResult"/>. 
        /// </returns>
        public IAsyncOperation<RateReminderResult> TryReminderAsync()
        {
            return InnerTryReminderAsync().AsAsyncOperation();
        }
#endif
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
#endregion // Public Properties

#region Public Events
        /// <summary>
        /// Occurs when the <see cref="TryReminder"/> method has completed.
        /// </summary>
        public event EventHandler<RateReminderResult> TryReminderCompleted;
#endregion // Public Events
    }
}
