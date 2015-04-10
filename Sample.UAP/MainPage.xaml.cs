using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sample.UAP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string ShownText = "Shown";
        private const string NotShownText = "Not Shown";

        public MainPage()
        {
            this.InitializeComponent();
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
