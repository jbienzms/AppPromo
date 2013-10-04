using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Sample.Win8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Sample.Win8.Common.LayoutAwarePage
    {
        private const string ShownText = "Shown";
        private const string NotShownText = "Not Shown";

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void RateReminder_TryReminderCompleted(object sender, AppPromo.RateReminderResult e)
        {
            var t = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DaysLabel.Text = e.Days.ToString();
                    RunsLabel.Text = e.Runs.ToString();
                    ReminderLabel.Text = (e.ReminderShown ? ShownText : NotShownText);
                    RatingLabel.Text = (e.RatingShown ? ShownText : NotShownText);
                });
        }

        private void ResetCountersButton_Click(object sender, RoutedEventArgs e)
        {
            RateReminder.ResetCounters();
            DaysLabel.Text = "0";
            RunsLabel.Text = "0";
            ReminderLabel.Text = NotShownText;
            RatingLabel.Text = NotShownText;
            ResetBlock.Text = "Counters Reset";
        }
    }
}
