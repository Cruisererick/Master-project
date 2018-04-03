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

namespace Final_Project
{
	public partial class MainPage : ContentPage
	{
		ObservableCollection<Project> list = new ObservableCollection<Project>();
		Database_Controller database = new Database_Controller();
		User user;
		Session live;
		bool getlocation;
		bool gettingLocation;
		Interrupts interrupt;
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
		}

		protected override void OnAppearing()
		{
			user.projects = database.GetProjectUser(user.Id);
			list.Clear();
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
				}
			}
			else
			{
				Resume_Session.IsVisible = false;
			}

		}

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

		private async void Button_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new New_Project(user, database));
			}
		}

		private async void Resume_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new SessionLive(database));
			}
		}

		private void askLocation()
		{
			Task.Run(() =>
			{
				Task.Delay(10000).Wait();
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
			bool change = Verfiy_Location.locationChange(live, position.Latitude, position.Longitude, listPossibleAddresses, database);
			return change;
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
