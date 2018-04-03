using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Final_Project.Control;
using Final_Project.Model;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class View_Sessions : ContentPage
	{
		List<Session> sessions;
		Database_Controller database;
		ObservableCollection<Session> list = new ObservableCollection<Session>();
		Session live;
		Interrupts interrupt;
		bool getlocation;
		bool gettingLocation;
		public View_Sessions (List<Session> sessionsSelected, Database_Controller databaseAccess)
		{
			sessions = sessionsSelected;
			InitializeComponent ();
			database = databaseAccess;
			foreach (var session in sessions)
			{
				list.Add(session);
			}

			var objectsList = new ListView() { HeightRequest = 200 };
			Session_List.ItemTemplate = new DataTemplate(typeof(ObjectsSession));
			Session_List.ItemsSource = list;
			Session_List.ItemTapped += OnSelection;
			live = database.GetliveSession();

			
		}

		protected override void OnAppearing()
		{
			Resume_Session.IsVisible = false;
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
		}

		private async void Resume_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new SessionLive(database));
			}
		}

		void OnSelection(object sender, ItemTappedEventArgs e)
		{
			if (e.Item == null)
			{
				return;
			}
			Session temp_session = (Session)e.Item;
			List<Interrupts> interruptsList = database.GetInterruptsOfSession(temp_session.Id);
			start.Text = temp_session.start.ToString();
			end.Text = temp_session.end.ToString();
			taskName.Text = "Task: ";
			Spend.Text = "Time Spend: ";
			taskName.Text += database.GetOneTask(temp_session.taskId).name;
			double lessTime = 0;
			foreach (var interrupt in interruptsList)
			{
				try
				{
					if (interrupt.end is null)
					{
						lessTime += 0;
					}
					else
					{
						lessTime += ((double)((TimeSpan)(interrupt.end - interrupt.start)).TotalMinutes);
					}
				}
				catch (Exception ex)
				{
					lessTime += 0;
				}
			}
			try
			{
				if (temp_session.end is null)
				{
					
				}
				else
				{
					double aux = (double)(((TimeSpan)(temp_session.end - temp_session.start)).TotalMinutes);
					Spend.Text += (aux - lessTime).ToString();
				}
			}
			catch (Exception ex)
			{
				Spend.Text += 0;
			}
			overlay.IsVisible = true;
			Session_List.IsVisible = false;
			start.Focus();
		}

		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Session_List.IsVisible = true;
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

	class ObjectsSession : ViewCell
	{
		public ObjectsSession()
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
			tempLabel.SetBinding(Label.TextProperty, "start"); // Define the binding to cField1 in the List !!
															  //
			var sLayout = new StackLayout();
			sLayout.Children.Add(tempLabel);
			this.View = sLayout;
		}
	}
}