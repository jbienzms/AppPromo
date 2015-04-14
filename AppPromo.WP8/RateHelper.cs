﻿#region License
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

#if WIN_RT || WINDOWS_UAP
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.ApplicationModel;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using AppPromo.Controls;
#endif

namespace AppPromo
{
    /// <summary>
    /// Potential responses to requesting an action.
    /// </summary>
    internal enum ActionResponse
    {
        /// <summary>
        /// The action should be performed.
        /// </summary>
        Confirm,
        /// <summary>
        /// The action should not be performed and should never be requested again.
        /// </summary>
        Decline,
        /// <summary>
        /// The action should not be performed but may be requested again later.
        /// </summary>
        Delay
    };

    internal enum DeviceType
    {
        Unknown,
        Mobile,
        Desktop
    };

    internal static class PlatformHelper
    {
        #if WIN_RT || WINDOWS_UAP
        static private ResourceLoader resourceLoader;
        #endif

        static private DeviceType GetDeviceType()
        {
            #if WINDOWS_PHONE
                return DeviceType.Mobile;
            #elif WINDOWS_UAP
                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
                if (qualifiers.ContainsKey("DeviceFamily"))
                {
                    switch (qualifiers["DeviceFamily"])
                    {
                        case "Mobile":
                            return DeviceType.Mobile;
                        case "Desktop":
                            return DeviceType.Desktop;
                        default:
                            return DeviceType.Unknown;
                    }
                }
                else
                {
                    return DeviceType.Unknown;
                }

            #else
                return DeviceType.Unknown; 
            #endif
        }

        #pragma warning disable 1998 // The async Task keeps the signature identical between platforms.
        static public async Task<ActionResponse> AskAction(string message, string title)
        {
            #if WINDOWS_PHONE
            var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            if  (result == MessageBoxResult.OK)
            {
                return ActionResponse.Confirm;
            }
            else
            {
                return ActionResponse.Decline;
            }
            #endif

            #if WIN_RT || WINDOWS_UAP
            // Get device type
            var deviceType = GetDeviceType();

            // If it's a mobile, use the ActionDialog
            if (deviceType == DeviceType.Mobile)
            {
                // Create the action dialog
                var ad = new ActionDialog();

                // Setup text fields
                ad.PromptText = message;
                ad.ConfirmText = ReadResourceString("Confirm");
                ad.DeclineText = ReadResourceString("Decline");
                ad.DelayText = ReadResourceString("Delay");
                ad.DontRemindAgainText = ReadResourceString("DontRemindAgain");

                // Show and get result
                var cdResult = await ad.ShowAsync();

                // Convert CD result to ActionResult
                if (cdResult == ContentDialogResult.Primary)
                {
                    return ActionResponse.Confirm;
                }
                else
                {
                    if (ad.DontRemindAgain)
                    {
                        return ActionResponse.Decline;
                    }
                    else
                    {
                        return ActionResponse.Delay;
                    }
                }
            }
            else // Not a mobile device
            {
                // Use MessageDialog
                var dlg = new MessageDialog(message, title);

                // Always show confirm
                dlg.Commands.Add(new UICommand(ReadResourceString("Confirm"), null, ActionResponse.Confirm));

                // If it's desktop we can add a delay (third) option
                if (deviceType == DeviceType.Desktop)
                {
                    dlg.Commands.Add(new UICommand(ReadResourceString("Delay"), null, ActionResponse.Delay));
                }

                // Always show decline
                dlg.Commands.Add(new UICommand(ReadResourceString("DontRemindAgain"), null, ActionResponse.Decline)); // "Decline"

                // Enter will always trigger Accept
                dlg.DefaultCommandIndex = 0;

                // Escape will trigger Delay or Cancel (depending on whether delay is available)
                dlg.CancelCommandIndex = 1;

                // Placeholder response
                var result = ActionResponse.Delay;

                // Attempt to show the dialog
                try
                {
                    result = (ActionResponse)(await dlg.ShowAsync()).Id;
                }
                catch (Exception)
                {
                    //	this may happen if any other modal window is shown at the moment (ie, Windows query about running application background task)
                }

                // Done
                return result;
            }
            #endif
        }
        #pragma warning restore 1998

        static public bool HasSetting(string key)
        {
            #if WINDOWS_PHONE
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
            #endif

            #if WIN_RT || WINDOWS_UAP
            return ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
            #endif
        }

        static public string ReadResourceString(string key)
        {
            #if WINDOWS_PHONE
            return Resources.ResourceManager.GetString(key);
            #endif

            #if WIN_RT
            if (resourceLoader == null) { resourceLoader = new ResourceLoader("AppPromo/Resources"); }
            #endif

            #if WINDOWS_UAP
            if (resourceLoader == null) { resourceLoader = ResourceLoader.GetForCurrentView("AppPromo/Resources"); }
            #endif

            #if WIN_RT || WINDOWS_UAP
            return resourceLoader.GetString(key);
            #endif
        }

        static public T ReadSetting<T>(string key)
        {
            #if WINDOWS_PHONE
            return (T)IsolatedStorageSettings.ApplicationSettings[key];
            #endif

            #if WIN_RT || WINDOWS_UAP
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

            #if WIN_RT || WINDOWS_UAP
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

            #if WIN_RT || WINDOWS_UAP
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            #endif
        }
        #pragma warning restore 1998

        static public void WriteSetting<T>(string key, T value)
        {
            #if WINDOWS_PHONE
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
            #endif

            #if WIN_RT || WINDOWS_UAP
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

                #if WIN_RT || WINDOWS_UAP
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
        /// <param name="delayed">
        /// A value that indicates if the user chose to delay their raiting until another time.
        /// </param>
        public RateReminderResult(int days, int runs, bool reminderShown, bool ratingShown, bool delayed)
        {
            Days = days;
            ReminderShown = reminderShown;
            RatingShown = ratingShown;
            Runs = runs;
            Delayed = delayed;
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
        /// Gets a value that indicates if the user chose to delay their raiting until another time.
        /// </summary>
        public bool Delayed { get; private set; }

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
        /// Shows the reminder and indicates if the rating interface was displayed.
        /// </summary>
        /// <returns>
        /// A task that yeilds the resut, <c>true</c> if the rating interface was shown; otherwise <c>false</c>.
        /// </returns>
        private async Task<ActionResponse> ShowReminderAsync()
        {
            // What message do we use?
            string msg = (!string.IsNullOrEmpty(CustomReminderText) ? CustomReminderText : PlatformHelper.ReadResourceString(MESSAGE_KEY));

            // Show the message
            var result = await PlatformHelper.AskAction(msg, "");

            // If it's OK to, show the rating window
            if (result == ActionResponse.Confirm)
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
        #if WIN_RT || WINDOWS_UAP
        internal async Task<RateReminderResult> InnerTryReminderAsync()
        #endif
        {
            int runs = 0;
            int days = 0;
            bool reminderShown = false;
            ActionResponse ratingResponse = ActionResponse.Delay;

            // If the runs reminder is enabled and has not been shown, see if we need to show it
            if ((RunsBeforeReminder > 0) && (!PlatformHelper.ReadSetting<bool>(SHOWN_FOR_RUNS_KEY, false)))
            {
                // How many runs so far?
                runs = GetRuns();

                // Have we met the threshold?
                if (runs >= RunsBeforeReminder)
                {
                    // Show the reminder
                    ratingResponse = await ShowReminderAsync();

                    // Flag that it's been shown this run
                    reminderShown = true;

                    // If the user didn't delay, mark it completed
                    if (ratingResponse != ActionResponse.Delay)
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
                    ratingResponse = await ShowReminderAsync();

                    // Flag that it's been shown this run
                    reminderShown = true;

                    // If the user didn't delay, mark it completed
                    if (ratingResponse != ActionResponse.Delay)
                    {
                        PlatformHelper.WriteSetting<bool>(SHOWN_FOR_DAYS_KEY, true);
                    }
                }
            }

            // Create result
            var result = new RateReminderResult(days, runs, reminderShown, (ratingResponse == ActionResponse.Confirm), (ratingResponse == ActionResponse.Delay));

            // Notify
            OnTryReminderCompleted(result);

            // Return result
            return result;
        }

        #if WIN_RT || WINDOWS_UAP
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
