using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup;
using Final_Project.Model;
using Final_Project.Control;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;
using System.Threading;

namespace Final_Project.Visual
{
	/* Author: Erick Lares
	 * This class is the screen that handles the creating of a new project, 
	 * requiring the user to fill every field needed.
	 */
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class New_Project : ContentPage
	{
		ObservableCollection<Project_Task> list = new ObservableCollection<Project_Task>();//Observable collection of tasks, used to list tasks and allow the user to tap them and remove them form the list.
		Database_Controller database;// The database.
		User currentUser;// Current live user.
		Session live;// Current live session.
		Interrupts interrupt;// Current live interrupt.
		bool getlocation; // View session live for more details.
		bool gettingLocation; // View session live for more details.
		bool movement = false; // View session live for more details.
		bool accelometeractive = false; // View session live for more details.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel"); // View session live for more details.
		bool accelworking = false; // View session live for more details.
		bool onlyOne = false; // View session live for more details.

		/*
		 * New_Project - Class constructor.
		 * @param user - Live user.
		 * @param databaseAccess - database.
		 */
		public New_Project (User user, Database_Controller databaseAccess)
		{
			database = databaseAccess;
			currentUser = user;
			InitializeComponent ();
			var objectsList = new ListView() { HeightRequest = 200 };
			Tasks_List.ItemTemplate = new DataTemplate(typeof(Objects)); 
			Tasks_List.ItemsSource = list;
			Tasks_List.ItemTapped += OnSelection;
		}

		/*
		 * OnAppearing - Get live session.
		 */
		protected override void OnAppearing()
		{
			live = database.GetliveSession();
			if (live != null)
			{
				interrupt = database.GetliveInterrupt();
				if (interrupt is null)
				{
					getlocation = true;
					askLocation();
					accelometeractive = true;
					GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
				}
			}
		}

		/*
		 * Button_Click - Handles add a new Task.
		 */
		private void Button_Click(object sender, EventArgs e)
		{
			Name.Text = string.Empty;
			Decription.Text = string.Empty;
			Estimation.Text = string.Empty;
			overlay.IsVisible = true;
			Tasks_List.IsVisible = false;
			New_Tasks.IsVisible = false;
			Create_Project.IsVisible = false;
			Name.Focus();
			//list.Add("New Item");
		}

		/*
		 * Done_Click - Save the project to the database, makes sure no field is empty.
		 */
		void Done_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				if (Project_Name.Text == "")
				{
					DisplayAlert("No name?", "Your project deserve a name!", "Ok");
					return;
				}

				if (Project_Description.Text == "")
				{
					DisplayAlert("What's about?", "Give a little description to the project", "Ok");
					return;
				}

				if (list.Count == 0)
				{
					DisplayAlert("No tasks?", "Divide your project in small tasks that you do one by one!", "Ok");
					return;
				}

				List<Session> sessions = new List<Session>();
				Project project = new Project();
				List<Project_Task> list_task = new List<Project_Task>();

				project.name = Project_Name.Text;
				project.description = Project_Description.Text;
				project.Sessions = sessions;
				project.userId = currentUser.Id;
				int ProjectId = database.SaveProject(project);
				foreach (var tasks in list)
				{
					list_task.Add(tasks);
					tasks.projectId = ProjectId;
					database.SaveTask(tasks);
				}
			
				Navigation.PopModalAsync();
			}
		}

		/*
		 * OnSelection - Deleted the tapped task out of the project.
		 */
		void OnSelection(object sender, ItemTappedEventArgs e)
		{
			if (e.Item == null)
			{
				return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
			}
			Project_Task temp_task = (Project_Task)e.Item;
			DisplayAlert("Item Selected", temp_task.name, "Ok");
			list.Remove(temp_task);
		}

		/*
		 * OnOKButtonClicked - Add the task to the list.
		 */
		void OnOKButtonClicked(object sender, EventArgs args)
		{	
			overlay.IsVisible = false;
			List<Session> sessions = new List<Session>();
			Project_Task new_task = new Project_Task(Name.Text, Decription.Text, Double.Parse(Estimation.Text), sessions);
			list.Add(new_task);
			Tasks_List.IsVisible = true;
			New_Tasks.IsVisible = true;
			Create_Project.IsVisible = true;
		}

		/*
		 * OnCancelButtonClicked - Close the overlay to add tasks.
		 */
		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Tasks_List.IsVisible = true;
			New_Tasks.IsVisible = true;
			Create_Project.IsVisible = true;
		}

		/*
		 * preventMovement - View session live for more details.
		 */
		private async void askLocation()
		{
			await Task.Delay(100).ContinueWith(async (arg) => {
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
						interrupt.reason = "Change location";
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
					Task.Delay(GlobalUtilities.locationTime).Wait();
				}
			});
		}

		/*
		 * preventMovement - View session live for more details.
		 */
		public async Task<bool> GetLocation()
		{
			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 20;

			var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(1), null, true);
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

		/*
		 * preventMovement - View session live for more details.
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
	}

	class Objects : ViewCell
	{
		public Objects()
		{
			Label tempLabel = new Label
			{
				Text = "Objects Placeholder",
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