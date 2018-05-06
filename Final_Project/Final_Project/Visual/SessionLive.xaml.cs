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
	/* Author: Erick Lares
	 * This class is the responsible of handling everything that 
	 * happens when a new session is started as well as giving the 
	 * user the information they need about the live session.
	 */
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SessionLive : ContentPage
	{
		Project selected_project;
		Project_Task selected_task; 
		Database_Controller database;
		Session current;
		Interrupts interrupt;
		bool gettingLocation = false; //Bool that indicates if the GPS is in used.
		bool getlocation = false; //Bool that allow the use of the GPS.
		bool movement = false; //Bool that indicate if move was detected.
		bool accelometeractive = false; //Bool that indicate if the accelerometer is active.
		bool accelworking = false; //Bool that indicates that a posible notifications could come from the accelerometer.
		bool onlyOne = false; //Bool to allow only one reading of the accelerometer.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");  //semaphore to allow only one reading of the accelerometer.

		/*
		 * SessionLive - Initialize the Content view and its components.
		 * @param project - The project that own the task and the session.
		 * @param task - The task that own the session.
		 * @param databaseAccess - The database.
		 */
		public SessionLive (Project project, Project_Task task, Database_Controller databaseAccess)
		{
			InitializeComponent ();
			this.Task_Name.Text = task.name;
			this.Project_Name.Text = project.name;
			selected_project = project;
			selected_task = task;
			database = databaseAccess;
			//Timer_Counter.Text = "Session is not live";
		}

		/* RefreshTime - Method that refresh in real time the timer of the session,
		 * the method takes the initial time of the session and subtrack the current time of
		 * the system, taking in account all the interrupts of the session.
		 */
		public async void RefreshTime()
		{
			await Task.Delay(100).ContinueWith(async (arg) =>
			{
				while (true)
				{
					List<Interrupts> interrruptL = database.GetInterruptsOfSession(current.Id);
					TimeSpan difference = (TimeSpan)(DateTime.Now - current.start);
					double extra = 0;
					foreach (var temp in interrruptL)
					{
						if (temp.end != null)
						{
							extra += ((TimeSpan)(temp.end - temp.start)).TotalMinutes;
						}
						else
						{
							extra += ((TimeSpan)(DateTime.Now - temp.start)).TotalMinutes;
						}
					}
					double total = difference.TotalMinutes;
					total = total - extra;
					Device.BeginInvokeOnMainThread(async () =>
					{
						Timer_Counter.Text = TimeSpan.FromMinutes(total).ToString();
					});
				}
			});
		}

		/* Accelerometer_ReadingAvailable - Method that pools data from the accelerometer, this method creates a new system task, so the user interface remains responsive. 
		 * If the GPS is being used the accelerometer must wait until this is done so the user doesn’t get two notifications at the same time.
		 * Only one reading is allowed at one time, a semaphore and preventing more reading by taking out the event are used to enforce this.
		 * If it’s the first reading the method does nothing.
		 * The method compares the previous reading to the new one to detect movement.
		 * If change is detected the movement buffer increases.
		 * If no movement is detected the still buffer increases.
		 * If the movement buffer gets to it limit an alert will be fire indicating that movement was detected, the user need to handle this alert.
		 * If the still buffer gets to it limit the movement buffer will reset.
		 * Still buffer and movement Buffer are used to discard and control accelerometer inaccuracy’s.
		 * If a notification is fire an interrupt will be created if the user stops the session the interrupt will be place in the database else, it will be ignored.
		 * If the application is in background and movement is detected a notification will be requested to android OS.
		 * While a reading is being done the user cannot leave this screen.
		 * A thread sleep is used to prevent the application to overflow the method with readings.
		 */
		private async void Accelerometer_ReadingAvailable(object sender, XLabs.EventArgs<XLabs.Vector3> e)
		{
			await Task.Delay(100).ContinueWith(async (arg) =>
				{
					if (!gettingLocation)
					{
						if (onlyOne == false)
						{
							onlyOne = true;
							semaphoreObject.WaitOne();
							GlobalUtilities.accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;

							XLabs.Vector3 Currentreading = e.Value;
							if (GlobalUtilities.LastMovement is null)
							{
								GlobalUtilities.LastMovement = Currentreading;
							}
							else
							{
								if (Math.Round(Currentreading.X, 1) != Math.Round(GlobalUtilities.LastMovement.X, 1) || Math.Round(Currentreading.Y, 1) != Math.Round(GlobalUtilities.LastMovement.Y, 1) || Math.Round(Currentreading.Z, 1) != Math.Round(GlobalUtilities.LastMovement.Z, 1))
								{
									GlobalUtilities.moving += 1;
									if (GlobalUtilities.moving > GlobalUtilities.MovementTicks)
									{
										accelworking = true;
										getlocation = false;
										GlobalUtilities.moving = 0;
										GlobalUtilities.still = 0;
										movement = true;
										interrupt = new Interrupts();
										interrupt.reason = "Movement detected.";
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
											realAnwser = await DisplayAlert("Movement detected", "Are you still working?", "Yes.", "No, pause.");
											if (!realAnwser)
											{
												interrupt.sessionId = current.Id;
												interrupt.Id = database.SaveInterrupt(interrupt);
												getlocation = false;
												//Timer_Counter.Text = "Session is paused";
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
										accelworking = true;
									}
								}
								else
								{
									GlobalUtilities.still += 1;
									if (GlobalUtilities.still > GlobalUtilities.StillTicks)
									{
										GlobalUtilities.moving = 0;
										GlobalUtilities.still = 0;
									}
								}
							}
							while (movement) ;
							GlobalUtilities.LastMovement = Currentreading;
							Thread.Sleep(GlobalUtilities.aceelTime);
							if (accelometeractive)
								GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
							semaphoreObject.Release();
							onlyOne = false;
							accelworking = false;
						}
					}
				});
		}

		/*
		 * SessionLive - Initialize the Content view when a session is being resume.
		 * @param databaseAccess - The database.
		 */
		public SessionLive(Database_Controller databaseAccess)
		{
			InitializeComponent();
			RefreshTime();
			database = databaseAccess;
			current = database.GetliveSession();
			selected_project = database.GetOneProject(current.project);
			selected_task = database.GetOneTask(current.taskId);
			this.Task_Name.Text = selected_task.name;
			this.Project_Name.Text = selected_project.name;
			interrupt = database.GetliveInterrupt();
			if (!Resolver.IsSet)
			{
				var container = new SimpleContainer();
				container.Register<IAccelerometer, Accelerometer>();
				Resolver.SetResolver(container.GetResolver());
			}
			GlobalUtilities.accelerometer = Resolver.Resolve<IAccelerometer>();
			GlobalUtilities.accelerometer.Interval = AccelerometerInterval.Normal;
			if (interrupt is null)
			{
				Start.Text = "Pause";
				getlocation = true;
				askLocation();
				accelometeractive = true;
				GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
			}
			else
			{
				Start.Text = "Resume";
				getlocation = false;
				askLocation();
			}
		}

		/*
		 * askLocation - Method that handles the location change,
		 * an infinite looping thread is created, a Boolean is used to 
		 * allow the reading of the GPS, once inside the method 
		 * a second Boolean signal every other method that GPS is 
		 * taking a reading, while the reading is been done the accelerometer  
		 * is blocked, also the user cannot leave this screen.
		 * 
		 * Once the location is obtained the method check is a change occurred, 
		 * if this was the case an alert will pop up stating that location 
		 * change was detected, and the user need to resolve this, an interrupt 
		 * will be created but it will only be stored in the database if the user
		 * agrees that the change in location was an interrupt to their work else, 
		 * it will be discarded.
		 * If the application is in background a request to the OS will be made 
		 * to push a notification outside the application.
		*/
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
							interrupt.reason = "Change Location";
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
									//Timer_Counter.Text = "Session is paused";
									Start.Text = "Resume";
									accelometeractive = false;
									GlobalUtilities.accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
									GlobalUtilities.moving = 0;
									GlobalUtilities.still = 0;
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
						Task.Delay(GlobalUtilities.locationTime).Wait();
					}
				}
			});	
		}

		/*
		 * GetLocation - Method that uses Geolocator and xamarin maps to 
		 * get the current geological information of the device.
		 */
		public async Task<bool> GetLocation()
		{
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 20;

			var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(100), null, true);
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

		/*
		 * StartButtonClicked - Method that start, pauses and resumes a session.
		 */
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
				//Timer_Counter.Text = "Session is live";
				accelometeractive = true;
				GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
				RefreshTime();
			}
			else if (Start.Text.CompareTo("Pause") == 0)
			{
				accelometeractive = false;
				GlobalUtilities.accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
				getlocation = false;
				Start.Text = "Resume";
				if (database.GetliveInterrupt() is null)
				{
					interrupt = new Interrupts();
					interrupt.reason = "User reason";
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
				GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
			}

		}

		/*
		 * PauseButtonClicked - Stops a session.
		 */
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

		/*
		 * OnBackButtonPressed - Overwrite on the back button press,
		 * the overwrite is done so the back button has to check if its allow to leave the screen.
		 */
		protected override bool OnBackButtonPressed()
		{
			if (preventMovement())
			{
				Navigation.PopModalAsync();
				return true;
			}
			return true;
		}

		/*
		 * preventMovement - Method that prevents the user from leaving the screen, this is 
		 * done so sensitives method finish so important information is not lost.
		 */
		public bool preventMovement()
		{
			if (!gettingLocation && !accelworking)
			{
				getlocation = false;
				accelometeractive = false;
				GlobalUtilities.accelerometer.ReadingAvailable -= Accelerometer_ReadingAvailable;
				return true;
			}
			else
			{
				DisplayAlert("Posible Question incoming!", "You cath us getting your current location or movement give us a second to finish that up", "Okey");
				return false;
			}
		}
	}
}