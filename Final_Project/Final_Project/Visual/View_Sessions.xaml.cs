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
using System.Threading;

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class View_Sessions : ContentPage
	{
		List<Session> sessions; //List of sessions.
		Database_Controller database; // Database
		ObservableCollection<Session> list = new ObservableCollection<Session>(); //Observable collection of sessions, used to list session.
		Session live;// Live Session
		Interrupts interrupt; //Live Session
		bool getlocation; // View session live for more details.
		bool gettingLocation;// View session live for more details.
		bool movement = false;// View session live for more details.
		bool accelometeractive = false;// View session live for more details.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");// View session live for more details.
		bool accelworking = false;// View session live for more details.
		bool onlyOne = false;// View session live for more details.

		/*
		 * View_Sessions - Class constructor.
		 * @param sessionsSelected - List of session to display.
		 * @param databaseAccess - database.
		 */
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


		/*
		 * OnAppearing - Get live session.
		 */
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
					accelometeractive = true;
					GlobalUtilities.accelerometer.ReadingAvailable += Accelerometer_ReadingAvailable;
				}
			}
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
		 * OnSelection - Goes to selected session details.
		 */
		async void OnSelection(object sender, ItemTappedEventArgs e)
		{
			if (e.Item == null)
			{
				return;
			}
			Session temp_session = (Session)e.Item;
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new Session_Detail(temp_session, database));
			}
			/*
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
			*/
		}

		/*
		 * OnCancelButtonClicked - Not used.
		 */
		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Session_List.IsVisible = true;
		}

		/*
		 * askLocation - View session live for more details.
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
		 * GetLocation - View session live for more details.
		 */
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

		/*
		 * OnBackButtonPressed - View session live for more details.
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