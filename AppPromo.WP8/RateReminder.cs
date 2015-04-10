#region License
/******************************************************************************
 * COPYRIGHT © MICROSOFT CORP. 
 * MICROSOFT LIMITED PERMISSIVE LICENSE (MS-LPL)
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
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
using System.Windows.Controls;
#endif

#if WIN_RT || WINDOWS_UAP
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.ApplicationModel;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace AppPromo
{
    /// <summary>
    /// A rating reminder control that can be placed on any Xaml page.
    /// </summary>
    public sealed class RateReminder : Control
    {
        #region Member Variables
        private RateHelper rateHelper;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="RateReminder"/> instance.
        /// </summary>
        public RateReminder()
        {
            // Defaults
            TryReminderOnLoad = true;

            // Create helper
            rateHelper = new RateHelper();

            // Subscribe to events
            rateHelper.TryReminderCompleted += rateHelper_TryReminderCompleted;                
            this.Loaded += Control_Loaded;
        }
        #endregion // Constructors

        #region Overrides / Event Handlers
        private void rateHelper_TryReminderCompleted(object sender, RateReminderResult e)
        {
            if (TryReminderCompleted != null) { TryReminderCompleted(this, e); }
        }

        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            // Only attempt reminder if runtime, not design time
            if (!PlatformHelper.IsInDesignMode)
            {
                // Try to show reminder?
                if (TryReminderOnLoad)
                {
                    #if WINDOWS_PHONE
                        var t = await rateHelper.TryReminderAsync();
                    #endif

                    #if WIN_RT || WINDOWS_UAP
                        var t = await rateHelper.InnerTryReminderAsync();
                    #endif
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
            rateHelper.ResetCounters();
        }

        #if WINDOWS_PHONE
        /// <summary>
        /// Checks to see whether it's time to show a reminder and if so, shows it.
        /// </summary>
        /// <returns>
        /// A task that yields the result, a <see cref="RateReminderResult"/>. 
        /// </returns>
        public Task<RateReminderResult> TryReminderAsync()
        #endif
        #if WIN_RT || WINDOWS_UAP
        /// <summary>
        /// Checks to see whether it's time to show a reminder and if so, shows it.
        /// </summary>
        /// <returns>
        /// An asynchronous operation that yields the result, a <see cref="RateReminderResult"/>. 
        /// </returns>
        public IAsyncOperation<RateReminderResult> TryReminderAsync()
        #endif
        {
            // Use helper
            return rateHelper.TryReminderAsync();
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
        public string CustomReminderText
        {
            get
            {
                return rateHelper.CustomReminderText;
            }
            set
            {
                rateHelper.CustomReminderText = value;
            }
        }

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
        public int DaysBeforeReminder
        {
            get
            {
                return rateHelper.DaysBeforeReminder;
            }
            set
            {
                rateHelper.DaysBeforeReminder = value;
            }
        }

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
        public int RunsBeforeReminder
        {
            get
            {
                return rateHelper.RunsBeforeReminder;
            }
            set
            {
                rateHelper.RunsBeforeReminder = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the control should try to show the reminer as soon as it's loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if the control should try to show the reminder as soon as it's loaded; otherwise <c>false</c>. The default is <c>true</c>.
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
