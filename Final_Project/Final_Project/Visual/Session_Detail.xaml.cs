using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Final_Project.Model;
using Final_Project.Control;
using System.Collections.ObjectModel;
using Plugin.Geolocator;
using Xamarin.Forms.Maps;
using XLabs.Platform.Device;
using System.Threading;
using XLabs.Ioc;

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Session_Detail : ContentPage
	{
		Database_Controller database;// Database.
		Session selectedSession;// Selected session
		ObservableCollection<Interrupts> list = new ObservableCollection<Interrupts>(); //Observable collection of tasks, used to list interrupts.
		ObservableCollection<Location> locationList = new ObservableCollection<Location>(); //Observable collection of tasks, used to list locations.
		bool LiveSession = false;//live session boolean.
		Session live;// Live session
		bool getlocation;// View session live for more details.
		bool gettingLocation;// View session live for more details.
		Interrupts interrupt;// Live interrupt.
		bool movement = false;// View session live for more details.
		bool accelometeractive = false;// View session live for more details.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");// View session live for more details.
		bool accelworking = false;// View session live for more details.
		bool onlyOne = false;// View session live for more details.

		/*
		 * Project_View_Page - Class constructor.
		 * @param session - Selected session.
		 * @param databaseAccess - database.
		 */
		public Session_Detail(Session session, Database_Controller databaseAccess)
		{
			selectedSession = session;
			database = databaseAccess;
			InitializeComponent();
			live = database.GetliveSession();
			if (live != null)
			{
				Resume_Session.IsVisible = true;
				LiveSession = true;
			}
			selectedSession.interrupts = database.GetInterruptsOfSession(selectedSession.Id);
			foreach (var interruptsTemp in selectedSession.interrupts)
			{
				list.Add(interruptsTemp);
			}
			var objectsList = new ListView() { HeightRequest = 200 };
			Interrupt_List.ItemTemplate = new DataTemplate(typeof(ObjectsInterrupt));
			Interrupt_List.ItemsSource = list;
			Interrupt_List.ItemTapped += OnSelection;

		}

		/*
		 * OnAppearing - Get live session, calculates the engage time for the session.
		 */
		protected override void OnAppearing()
		{
			overlayLocation.IsVisible = false;
			Interrupt_List.IsVisible = true;
			live = database.GetliveSession();

			if (live != null)
			{
				Resume_Session.IsVisible = true;
				LiveSession = true;
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
				LiveSession = false;
			}
			Start_Time.Text = selectedSession.start.ToString();
			End_Time.Text = selectedSession.end.ToString();
			Task_Name.Text = "Task: " + database.GetOneTask(selectedSession.taskId).name;
			Time_Spend.Text = "Time Spend: ";
			List<Interrupts> interruptsList = database.GetInterruptsOfSession(selectedSession.Id);
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
				if (selectedSession.end is null)
				{

				}
				else
				{
					double aux = (double)(((TimeSpan)(selectedSession.end - selectedSession.start)).TotalMinutes);
					Time_Spend.Text += TimeSpan.FromMinutes((aux - lessTime)).ToString();
				}
			}
			catch (Exception ex)
			{
				Time_Spend.Text += TimeSpan.FromMinutes(0);
			}
		}

		/*
		 * OnSelection - display information for the select interrupt.
		 */
		void OnSelection(object sender, ItemTappedEventArgs e)
		{
			OverlayInterrupts.IsVisible = true;
			Interrupt_List.IsVisible = false;
			Interrupts temp_interrupt = (Interrupts)e.Item;
			TimeStartI.Text = "Start Time: " + temp_interrupt.start.ToString();
			TimeEndI.Text = "End Time: " + temp_interrupt.start.ToString();
			ReasonI.Text = "Reason: " + temp_interrupt.reason;
		}

		/*
		 * OnCancelButtonClicked - Closes location overlay.
		 */
		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlayLocation.IsVisible = false;
			Give_Location.IsVisible = true;
			Interrupt_List.IsVisible = true;
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
		 * OnCancelButtonClicked2 - Close the interrupt overlay.
		 */
		void OnCancelButtonClicked2(object sender, EventArgs args)
		{
			OverlayInterrupts.IsVisible = false;
			Interrupt_List.IsVisible = true;
		}

		/*
		 * Location_Cliked - Displays the list of locations.
		 */
		void Location_Cliked(object sender, EventArgs args)
		{
			overlayLocation.IsVisible = true;
			Give_Location.IsVisible = false;
			Interrupt_List.IsVisible = false;
			selectedSession.locations = database.GetLocationOfSession(selectedSession.Id);
			foreach (var loc in selectedSession.locations)
			{
				locationList.Add(loc);
			}
			var objectsListLocation = new ListView() { HeightRequest = 200 };
			Location_List.ItemTemplate = new DataTemplate(typeof(ObjectsLocation));
			Location_List.ItemsSource = locationList;
			overlayLocation.Focus();
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

	class ObjectsLocation : ViewCell
	{
		public ObjectsLocation()
		{
			Label tempLabel = new Label
			{
				Text = "Objects Placeholder", // this will be overwritten at runtime with the data in the List (see binding below)!!
				TextColor = Color.Black,
				FontAttributes = FontAttributes.Italic,
				FontSize = 20,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};
			//
			tempLabel.SetBinding(Label.TextProperty, "address"); // Define the binding to cField1 in the List !!
															  //
			var sLayout = new StackLayout();
			sLayout.Children.Add(tempLabel);
			this.View = sLayout;
		}
	}

	class ObjectsInterrupt : ViewCell
	{
		public ObjectsInterrupt()
		{
			Label tempLabel = new Label
			{
				Text = "Objects Placeholder", // this will be overwritten at runtime with the data in the List (see binding below)!!
				TextColor = Color.Black,
				FontAttributes = FontAttributes.Italic,
				FontSize = 25,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};
			//
			tempLabel.SetBinding(Label.TextProperty, "reason"); // Define the binding to cField1 in the List !!
																 //
			var sLayout = new StackLayout();
			sLayout.Children.Add(tempLabel);
			this.View = sLayout;
		}
	}
}