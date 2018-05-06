using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Content;
using System.Threading;
using SQLite;
using Final_Project.Model;
using Final_Project.Control;
using Android.Media;

namespace Final_Project.Droid
{
	[Activity(Label = "Final_Project", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		private static readonly int ButtonClickNotificationId = 1000;
		ThreadStart myThreadDelegate; 
		Thread myThread;//Thread that consult database to verify if notification is needed.
		Database_Controller database;// Database
		RunningInfo info;// Running info.

		/*
		 * OnCreate - Constructor for the activity.
		 */
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			Forms.Init(this, bundle);
			LoadApplication(new App());
			Xamarin.FormsMaps.Init(this, bundle);
			myThreadDelegate = new ThreadStart(CheckNotifications);
			myThread = new Thread(myThreadDelegate);
			database = new Database_Controller();
			info = new RunningInfo();
			info.Id = 1;
			info.background = false;
			info.notificationNeeded = false;
			database.SaveRunningInfo(info);
			myThread.Start();
		}

		/*
		 * OnResume - Override for onResume to store in the database when the applications resumes.
		 */
		protected override void OnResume()
		{
			base.OnResume();
			info = database.getRunningInfo(1);
			info.background = false;
			database.UpdateRunningInfo(info);
		}

		/*
		 * OnPause - Override for onPause to store in the database when the applications sleeps.
		 */
		protected override void OnPause()
		{
			base.OnPause();
			info = database.getRunningInfo(1);
			info.background = true;
			database.UpdateRunningInfo(info);
			/*
			var contentIntent = PendingIntent.GetActivity(this, 0, new Intent(this, this.GetType()), 0);
			NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
			.SetAutoCancel(true)
			.SetContentIntent(contentIntent) 
			.SetContentTitle("Button Clicked")     
			.SetNumber(5)                     
			.SetSmallIcon(Resource.Drawable.notti_icon)  
			.SetContentText(String.Format(
				"The button has been clicked {0} times.", 5));

			NotificationManager notificationManager =
		(NotificationManager)GetSystemService(Context.NotificationService);
			notificationManager.Notify(ButtonClickNotificationId, builder.Build());*/
		}

		/*
		 * CheckNotifications - Check if a notification is needed, push the notification if needed.
		 */
		protected void CheckNotifications()
		{
			
			while (true)
			{
				info = database.getRunningInfo(1);
				if (info.notificationNeeded == true && info.background == true)
				{
					var contentIntent = PendingIntent.GetActivity(this, 0, new Intent(this, this.GetType()), 0);
					NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
					.SetAutoCancel(true)
					.SetContentIntent(contentIntent)
					.SetContentTitle("Hey there")
					.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
					 .SetVibrate(new long[] { 1000, 1000 })
					.SetSmallIcon(Resource.Drawable.notti_icon)
					.SetContentText(String.Format("We need your attention over here"));

					NotificationManager notificationManager =
					(NotificationManager)GetSystemService(Context.NotificationService);
					notificationManager.Notify(ButtonClickNotificationId, builder.Build());
					info.notificationNeeded = false;
					database.UpdateRunningInfo(info);
				}
				Thread.Sleep(10000);
			}
		}
	}
}

