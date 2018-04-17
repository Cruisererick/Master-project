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

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class New_Project : ContentPage
	{
		ObservableCollection<Project_Task> list = new ObservableCollection<Project_Task>();
		Database_Controller database;
		User currentUser;
		Session live;
		Interrupts interrupt;
		bool getlocation;
		bool gettingLocation;
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
				}
			}
		}

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

		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Tasks_List.IsVisible = true;
			New_Tasks.IsVisible = true;
			Create_Project.IsVisible = true;
		}

		private async void askLocation()
		{
			await Task.Delay(10).ContinueWith(async (arg) => {
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