using Final_Project.Control;
using Final_Project.Model;
using Final_Project.Visual;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;
using XLabs.Platform.Device;
using System.Threading;
using XLabs.Ioc;

namespace Final_Project
{
	/* Author: Erick Lares
	 * This class is the first screen the user will see when they start the application.
	 */
	public partial class MainPage : ContentPage
	{
		ObservableCollection<Project> list = new ObservableCollection<Project>(); //Observable collection of projects, used to list ptoject and allow the user to tap them.
		Database_Controller database = new Database_Controller(); //Database creation.
		User user; //active User
		Session live; //Live session
		bool getlocation; // View session live for more details.
		bool gettingLocation;// View session live for more details.
		Interrupts interrupt;// Live interrupt.
		bool movement = false;// View session live for more details.
		bool accelometeractive = false;// View session live for more details.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");// View session live for more details.
		bool accelworking = false;// View session live for more details.
		bool onlyOne = false;// View session live for more details.

		/*
		 * MainPage - Constructor, initialize the componets, make sure there is no live session in the database
		 * Fill projetcs list.
		 */
		public MainPage()
		{
			List<Project> ProjectsL;
			if (database.GetUser() == null)
			{
				Settings temp_settings = new Settings();
				ProjectsL = new List<Project>();
				user = new User("Erick", temp_settings, ProjectsL);
				database.SaveUser(user);
			}
			else
			{
				user = database.GetUser();
				user.projects = database.GetProjectUser(user.Id);
			}
			InitializeComponent();
			live = database.GetliveSession();
			if (live != null)
			{
				DisplayAlert("Session left Live", "The appplication was closed and a session was alive the end time was set to now, this can be change on the edit option for the session.", "OK");
				live.end = DateTime.Now;
				database.UpdateSession(live);
			}
			Proyects.ItemTapped += OnSelection;
			GlobalUtilities.setAccel();
		}

		/*
		 * OnAppearing - Overrride to the screen appearing, refresh the project list,
		 * gets the live session.
		 */
		protected override void OnAppearing()
		{
			user.projects = database.GetProjectUser(user.Id);
			list.Clear();
			if (!Resolver.IsSet)
			{
				var container = new SimpleContainer();
				container.Register<IAccelerometer, Accelerometer>();
				Resolver.SetResolver(container.GetResolver());
			}
			GlobalUtilities.accelerometer = Resolver.Resolve<IAccelerometer>();
			GlobalUtilities.accelerometer.Interval = AccelerometerInterval.Normal;
			if (user.projects != null)
				foreach (var ProjectTemp in user.projects)
				{
					ProjectTemp.tasks = database.GetTaskOfProjects(ProjectTemp.Id);
					list.Add(ProjectTemp);
				}
			Proyects.ItemTemplate = new DataTemplate(typeof(Objects));
			Proyects.ItemsSource = list;
			live = database.GetliveSession();
			if (live != null)
			{
				Resume_Session.IsVisible = true;
				interrupt = database.GetliveInterrupt();
				if (interrupt is null)
				{
					getlocation = true;
					askLocation();
					accelometeractive = true;
					GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
				}
			}
			else
			{
				Resume_Session.IsVisible = false;
			}

		}

		/*
		 * Accelerometer_ReadingAvailable - View session live for more details.
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
											interrupt.sessionId = live.Id;
											interrupt.Id = database.SaveInterrupt(interrupt);
											getlocation = false;
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
		 * OnSelection - Method to handle the user selecting a project from the list.
		 */
		private async void OnSelection(object sender, ItemTappedEventArgs e)
		{
			if (preventMovement())
			{
				if (e.Item == null)
				{
					return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
				}
				Project temp_project = (Project)e.Item;
				await Navigation.PushModalAsync(new Project_View_Page(temp_project, database));
			}
		}

		/*
		 * Button_Click - Method to handle the user selecting to create a new project.
		 */
		private async void Button_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new New_Project(user, database));
			}
		}

		/*
		* Stats_Click - Method to handle the user selecting to view stats.
		*/
		private async void Stats_Click(object sender, EventArgs e)
		{
			overlay.IsVisible = true;
			TimeSpan ATP = TimeSpan.Zero;
			Double PPR = 0;
			TimeSpan ATT = TimeSpan.Zero;
			Double TPR = 0;
			TimeSpan auxPPR = TimeSpan.Zero;
			TimeSpan auxATP = TimeSpan.Zero;
			TimeSpan auxATT = TimeSpan.Zero;
			TimeSpan auxTPR = TimeSpan.Zero;

			foreach (var tempP in user.projects)
			{
				auxATP = Project.GetEngageTime(tempP, database);
				ATP += auxATP;
				auxPPR = Project.GetEstimation(tempP, database);

				if (auxATP.TotalMinutes != 0 && auxPPR.TotalMinutes != 0)
				{
					if (auxATP < auxPPR)
					{
						PPR += Math.Round((auxATP.TotalMinutes / auxPPR.TotalMinutes) * 100, 1);
					}
					else
					{
						PPR += Math.Round((auxPPR.TotalMinutes / auxATP.TotalMinutes) * 100, 1);
					}
				}

				auxATT = Project.AverageOfTasks(tempP, database);
				ATT += auxATT;
				auxTPR = Project.GetEstimation(tempP, database);

				if (auxATT.TotalMinutes != 0 && auxTPR.TotalMinutes != 0)
				{
					if (auxATT < auxTPR)
					{
						TPR += Math.Round((auxATT.TotalMinutes / auxTPR.TotalMinutes) * 100, 1);
					}
					else
					{
						TPR += Math.Round((auxTPR.TotalMinutes / auxATT.TotalMinutes) * 100, 1);
					}
				}


			}

			ATP = TimeSpan.FromMinutes(ATP.TotalMinutes / user.projects.Count);
			ATT = TimeSpan.FromMinutes(ATT.TotalMinutes / user.projects.Count);
			PPR = PPR / user.projects.Count;
			TPR = TPR / user.projects.Count;
			AverageTimeProject.Text = "Projects Average Time: " + ATP.ToString();
			ProjectPrecictionRate.Text = "Projects Estimation correctness: " + PPR.ToString() + "%";
			AverageTimeTask.Text = "Task Average Time: " + ATT.ToString();
			TaskPrecictionRate.Text = "Task Estimation correctness: " + TPR.ToString() + "%";


		}

		/*
		* Stats_Click - Method to close the stats overlay.
		*/
		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
		}

		/*
		 * Resume_Click - Goes to session live to resume the live session.
		 */
		private async void Resume_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new SessionLive(database));
			}
		}

		/*
		 * askLocation - View session live for more details.
		 */
		private async void askLocation()
		{
			await Task.Delay(10).ContinueWith(async (arg) => {
				Task.Delay(GlobalUtilities.locationTime).Wait();
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
						var answer = Task.FromResult(false);
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
								interrupt.Id = database.SaveInterrupt(interrupt);
								getlocation = false;
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
			});
		}

		/*
		 * GetLocation - View session live for more details.
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
			bool change = Verfiy_Location.locationChange(live, position.Latitude, position.Longitude, listPossibleAddresses, database);
			return change;
		}

		/*
		 * preventMovement - View session live for more details.
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

	class Objects : ViewCell
	{
		public Objects()
		{
			Label tempLabel = new Label
			{
				Text = "Objects Placeholder", // this will be overwritten at runtime with the data in the List (see binding below)!!
				TextColor = Color.Black,
				FontAttributes = FontAttributes.Italic,
				FontSize = 35,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};
			//
			tempLabel.SetBinding(Label.TextProperty, "name"); // Define the binding to cField1 in the List !!
															  //
			var sLayout = new StackLayout();
			sLayout.Children.Add(tempLabel);
			this.View = sLayout;
		}
	}
}
