// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkId=232509
(function () {
	"use strict";

	var app = WinJS.Application;
	var activation = Windows.ApplicationModel.Activation;
	var rate = null;

	app.onactivated = function (args) {
		if (args.detail.kind === activation.ActivationKind.launch) {
			if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
				// TODO: This application has been newly launched. Initialize your application here.
			} else {
				// TODO: This application was suspended and then terminated.
				// To create a smooth user experience, restore application state here so that it looks like the app never stopped running.
			}
			args.setPromise(WinJS.UI.processAll().then(function ()
			{
			    // Subscribe to the reset button
			    resetButton.addEventListener("click", resetClick, false);

	            // Create the reminder helper object
	            rate = new AppPromo.UWP.RateHelper();

	            // Say we only want to wait 3 runs instead of the default 7
	            rate.runsBeforeReminder = 3;

	            // Try to show the reminder
	            var result = null;
	            rate.tryReminderAsync().then(function (result) {
	                // Show the results on the page
	                numDays.innerText = result.days.toString();
	                numRuns.innerText = result.runs.toString();
	                reminderShown.innerText = result.reminderShown.toString();
	                ratingShown.innerText = result.ratingShown.toString();
	            });
			}));
		}
	};

	function resetClick(mouseEvent) {
	    rate.resetCounters();

	    numDays.innerText = "0";
	    numRuns.innerText = "0";
	    reminderShown.innerText = "";
	    ratingShown.innerText = "";
	}

	app.oncheckpoint = function (args) {
		// TODO: This application is about to be suspended. Save any state that needs to persist across suspensions here.
		// You might use the WinJS.Application.sessionState object, which is automatically saved and restored across suspension.
		// If you need to complete an asynchronous operation before your application is suspended, call args.setPromise().
	};

	app.start();
})();
