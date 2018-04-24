using Final_Project.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading;
using Final_Project.Control;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;
using XLabs.Forms;
using XLabs.Ioc;
using XLabs.Platform.Device;
using Plugin.LocalNotifications;

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SessionLive : ContentPage
	{
		Project selected_project;
		Project_Task selected_task;
		Database_Controller database;
		Session current;
		Interrupts interrupt;
		bool gettingLocation = false;
		bool getlocation = false;
		IAccelerometer accelerometer;
		XLabs.Vector3 LastMovement;
		bool movement = false;
		int still = 0;
		int moving = 0;
		bool accelometeractive = false;
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");

		public SessionLive (Project project, Project_Task task, Database_Controller databaseAccess)
		{
			InitializeComponent ();
			this.Task_Name.Text = task.name;
			this.Project_Name.Text = project.name;
			selected_project = project;
			selected_task = task;
			database = databaseAccess;
			Timer_Counter.Text = "Session is not live";
			if (!Resolver.IsSet)
			{
				var container = new SimpleContainer();
				container.Register<IAccelerometer, Accelerometer>();
				Resolver.SetResolver(container.GetResolver());
			}
			accelerometer = Resolver.Resolve<IAccelerometer>();
			accelerometer.Interval = AccelerometerInterval.Normal;
		}

			private async void Accelerometer_ReadingAvailable(object sender, XLabs.EventArgs<XLabs.Vector3> e)
		{
				await Task.Delay(10).ContinueWith(async (arg) =>
				{
					if (!gettingLocation)
					{
						semaphoreObject.WaitOne();
						accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
						XLabs.Vector3 Currentreading = e.Value;
						if (LastMovement is null)
						{
							LastMovement = Currentreading;
						}
						else
						{
							if (Math.Round(Currentreading.X,1) != Math.Round(LastMovement.X,1) || Math.Round(Currentreading.Y,1) != Math.Round(LastMovement.Y,1) || Math.Round(Currentreading.Z,1) != Math.Round(LastMovement.Z,1))
							{
								moving += 1;
								if (moving > 5)
								{
									getlocation = false;
									moving = 0;
									still = 0;
									movement = true;
									interrupt = new Interrupts();
									DateTime toBeClonedDateTime = DateTime.Now;
									interrupt.start = toBeClonedDateTime;
									bool realAnwser = false;

									Device.BeginInvokeOnMainThread(
									async () =>
									{
										realAnwser = await DisplayAlert("Movement detected", "Are you still working?", "Yes.", "No, pause.");
										if (!realAnwser)
										{
											interrupt.sessionId = current.Id;
											interrupt.Id = database.SaveInterrupt(interrupt);
											getlocation = false;
											Timer_Counter.Text = "Session is paused";
											Start.Text = "Resume";
											accelometeractive = false;
										}
										else
										{
											getlocation = true;
											accelometeractive = true;
										}
										movement = false;
									});
								}
							}
							else
							{
								still += 1;
								if (still > 5)
								{
									moving = 0;
									still = 0;
								}
							}
						}
						while (movement) ;
						LastMovement = Currentreading;
						Thread.Sleep(1333);
						if(accelometeractive)
							accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
						semaphoreObject.Release();
					}
				});
		}
		public SessionLive(Database_Controller databaseAccess)
		{
			InitializeComponent();
			database = databaseAccess;
			current = database.GetliveSession();
			selected_project = database.GetOneProject(current.project);
			selected_task = database.GetOneTask(current.taskId);
			this.Task_Name.Text = selected_task.name;
			this.Project_Name.Text = selected_project.name;
			interrupt = database.GetliveInterrupt();
			if (interrupt is null)
			{
				Start.Text = "Pause";
				Timer_Counter.Text = "Session is live";
				getlocation = true;
				askLocation();
			}
			else
			{
				Start.Text = "Resume";
				Timer_Counter.Text = "Session is not live";
				getlocation = false;
				askLocation();
			}
		}

		private async void askLocation()
		{
			await Task.Delay(10).ContinueWith(async (arg) => {
				while (true)
				{
					while (getlocation)
					{
						gettingLocation = true;
						var change = GetLocation();
						change.Wait();
						bool realChange = change.Result;
						if (!realChange)
						{
							getlocation = false;
							interrupt = new Interrupts();
							DateTime toBeClonedDateTime = DateTime.Now;
							interrupt.start = toBeClonedDateTime;
							bool realAnwser = false;
							RunningInfo info = database.getRunningInfo(1);
							if (info.background)
							{
								info.notificationNeeded = true;
								database.UpdateRunningInfo(info);
							}
							Device.BeginInvokeOnMainThread(
							async () =>
							{
								realAnwser = await DisplayAlert("Location Change", "Hey there your location change, are you still working?", "Yes.", "No, pause.");
								if (!realAnwser)
								{
									interrupt.sessionId = current.Id;
									interrupt.Id = database.SaveInterrupt(interrupt);
									getlocation = false;
									Timer_Counter.Text = "Session is paused";
									Start.Text = "Resume";
									accelometeractive = false;
									accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
									moving = 0;
									still = 0;
								}
								else
								{
									getlocation = true;
								}
								gettingLocation = false;
							});
						}
						else
						{
							gettingLocation = false;
						}
						while (gettingLocation) ;
						Task.Delay(10000).Wait();
					}
				}
			});	
		}

		

		public async Task<bool> GetLocation()
		{
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 20;

			var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(1000), null, true);
			Geocoder geoCoder;
			geoCoder = new Geocoder();

			var temp = new Position(position.Latitude, position.Longitude);
			var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(temp);
			List<string> listPossibleAddresses = new List<string>();
			foreach (var address in possibleAddresses)
			{
				listPossibleAddresses.Add(address);
			}
			bool change = Verfiy_Location.locationChange(current, position.Latitude, position.Longitude, listPossibleAddresses, database);
			return change;
		}

		void StartButtonClicked(object sender, EventArgs args)
		{
			if (Start.Text.CompareTo("Start") == 0)
			{
				
				Session new_session = new Session();
				DateTime toBeClonedDateTime = DateTime.Now;
				DateTime initial = toBeClonedDateTime;
				new_session.start = initial;
				new_session.end = null;
				new_session.project = selected_project.Id;
				new_session.taskId = selected_task.Id;
				int id = database.SaveSession(new_session);
				new_session.Id = id;
				current = new_session;
				Start.Text = "Pause";
				getlocation = true;
				askLocation();
				Timer_Counter.Text = "Session is live";
				accelometeractive = true;
				accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
			}
			else if (Start.Text.CompareTo("Pause") == 0)
			{
				accelometeractive = false;
				accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
				getlocation = false;
				Start.Text = "Resume";
				if (database.GetliveInterrupt() is null)
				{
					interrupt = new Interrupts();
					DateTime toBeClonedDateTime = DateTime.Now;
					interrupt.start = toBeClonedDateTime;
					interrupt.sessionId = current.Id;
					interrupt.Id = database.SaveInterrupt(interrupt);
				}
				Timer_Counter.Text = "Session is paused";
				

			}
			else if (Start.Text.CompareTo("Resume") == 0)
			{
				Start.Text = "Pause";
				interrupt = database.GetliveInterrupt();
				DateTime toBeClonedDateTime = DateTime.Now;
				interrupt.end = toBeClonedDateTime;
				database.UpdateInterrupt(interrupt);
				getlocation = true;
				Timer_Counter.Text = "Session is live";
				accelometeractive = true;
				accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
			}

		}

		void PauseButtonClicked(object sender, EventArgs args)
		{
			if (preventMovement())
			{
				current.end = DateTime.Now;
				database.UpdateSession(current);
				if (Start.Text.CompareTo("Resume") == 0)
				{
					interrupt = database.GetliveInterrupt();
					DateTime toBeClonedDateTime = DateTime.Now;
					interrupt.end = toBeClonedDateTime;
					database.UpdateInterrupt(interrupt);
				}
				getlocation = false;
				Navigation.PopModalAsync();
			}
		}

		protected override bool OnBackButtonPressed()
		{
			if (preventMovement())
			{
				Navigation.PopModalAsync();
				return true;
			}
			return true;
		}

		public bool preventMovement()
		{
			if (!gettingLocation)
			{
				getlocation = false;
				return true;
			}
			else
			{
				DisplayAlert("Getting location", "You cath us getting your current location give us a second to finish that up", "Okey");
				return false;
			}
		}
	}
}