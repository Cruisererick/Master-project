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
	public partial class Project_View_Page : ContentPage
	{
		Project selectedProject;
		Project_Task tapped;// Tapped task.
		Database_Controller database; //Database
		ObservableCollection<Project_Task> list = new ObservableCollection<Project_Task>(); //Observable collection of tasks, used to list tasks.
		bool LiveSession = false; //live session boolean.
		Session live;// Live session
		bool getlocation;// View session live for more details.
		bool gettingLocation;// View session live for more details.
		Interrupts interrupt;// View session live for more details.
		bool movement = false;// View session live for more details.
		bool accelometeractive = false;// View session live for more details.
		Semaphore semaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1, name: "accel");// View session live for more details.
		bool accelworking = false;// View session live for more details.
		bool onlyOne = false;// View session live for more details.


		/*
		 * Project_View_Page - Class constructor.
		 * @param project - Selected project.
		 * @param databaseAccess - database.
		 */
		public Project_View_Page (Project project, Database_Controller databaseAccess)
		{
			selectedProject = project;
			database = databaseAccess;
			InitializeComponent();
			live = database.GetliveSession();	
			if (live != null)
			{
				Resume_Session.IsVisible = true;
				LiveSession = true;
			}
			
			Project_Name.Text += selectedProject.name;
			Project_Description.Text += selectedProject.description;

			foreach (var tasks in selectedProject.tasks)
			{
				list.Add(tasks);
			}
			var objectsList = new ListView() { HeightRequest = 200 };
			Tasks_List.ItemTemplate = new DataTemplate(typeof(Objects));
			Tasks_List.ItemsSource = list;
			Tasks_List.ItemTapped += OnSelection;
		}


		/*
		 * OnAppearing - Get live session, calculates the engage time and estimation time for the project.
		 */
		protected override void OnAppearing()
		{
			overlay.IsVisible = false;
			Tasks_List.IsVisible = true;
			View_Sessions.IsVisible = true;
			Delete_Project.IsVisible = true;
			tapped = null;
			live = database.GetliveSession();
			Estimation_Time.Text = "Estimation: ";
			Engage_Time.Text = "Time spend: ";
			double auxEstimation = GetEstimation(selectedProject);
			double auxSpend = GetEngageTime(selectedProject);
			Estimation_Time.Text += TimeSpan.FromMinutes(Math.Round(auxEstimation, 3)).ToString();
			Engage_Time.Text += TimeSpan.FromMinutes(Math.Round(auxSpend, 3)).ToString();
			if (auxEstimation < auxSpend)
			{
				Engage_Time.BackgroundColor = Color.Red;
			}
			else
			{
				Engage_Time.BackgroundColor = Color.Green;
			}
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
		}

		/*
		 * OnSelection - Method that display the information of the selected task.
		 */
		void OnSelection(object sender, ItemTappedEventArgs e)
		{
			if (e.Item == null)
			{
				return; 
			}
			Project_Task temp_task = (Project_Task)e.Item;
			Name.Text = "Name: ";
			Decription.Text = "Description: ";
			Estimation.Text = "Estimation: ";
			temp_task.sessions = database.GetSessionsOfTask(temp_task.Id);
			double timeSP;
			if (temp_task.sessions != null)
			{
				timeSP = GetTaskEngageTime(temp_task.sessions);
				TimeSpendTask.Text = "Time Spend: " + TimeSpan.FromMinutes(Math.Round(timeSP, 3)).ToString();
			}
			else
			{
				timeSP = 0;
			}

			Name.Text += temp_task.name;
			Decription.Text += temp_task.description;
			Estimation.Text += TimeSpan.FromMinutes(temp_task.estimation).ToString();
			tapped = temp_task;
			if (temp_task.estimation < timeSP)
			{
				TimeSpendTask.BackgroundColor = Color.Red;
			}
			else
			{
				TimeSpendTask.BackgroundColor = Color.Green;
			}
			overlay.IsVisible = true;
			Tasks_List.IsVisible = false;
			View_Sessions.IsVisible = false;
			Delete_Project.IsVisible = false;
			Name.Focus();
		}

		/*
		 * NewSessionButtonClicked - Method that handles a new session for a task.
		 */
		private async void NewSessionButtonClicked(object sender, EventArgs args)
		{
			if (LiveSession == false)
			{
				if (preventMovement())
				{
					await Navigation.PushModalAsync(new SessionLive(selectedProject, tapped, database));
				}
			}
			else
			{
				await DisplayAlert("Session Live", "There is a Session active, please terminate that session firts", "OK");
			}
		}

		/*
		 * OnCancelButtonClicked - Method that cancels the information displayed for a task.
		 */
		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Tasks_List.IsVisible = true;
			View_Sessions.IsVisible = true;
			Delete_Project.IsVisible = true;
			tapped = null;
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
		 * ViewSessionsClick - view all session that belong to the project.
		 */
		private async void ViewSessionsClick(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				List<Session> projectSessions = database.GetSessionsOfProjects(selectedProject.Id);
				await Navigation.PushModalAsync(new View_Sessions(projectSessions, database));
			}
		}

		/*
		 * ViewTaskSessionsClick - view all session that belong to a task.
		 */
		private async void ViewTaskSessionsClick(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				List<Session> taskSessions = database.GetSessionsOfTask(tapped.Id);
				await Navigation.PushModalAsync(new View_Sessions(taskSessions, database));
			}
		}

		/*
		 * GetEstimation - Method that calculate the estimation time for the entire project,
		 * based on the tasks.
		 * @param project - The project
		 */
		double GetEstimation(Project project)
		{
			double minutes = 0;
			foreach (var projectTask in project.tasks)
			{
				minutes += projectTask.estimation;
			}
			return minutes;
		}

		/*
		 * GetEngageTime - Method that calculate the spend time for the entire project,
		 * based on the sessions.
		 * @param project - The project
		 */
		double GetEngageTime(Project project)
		{
			double minutes = 0;
			TimeSpan difference;
			List<Session> projectSessions = database.GetSessionsOfProjects(selectedProject.Id);
			foreach (var session in projectSessions)
			{
				try
				{
					double lessTime = 0;
					List<Interrupts> interruptsList = database.GetInterruptsOfSession(session.Id);
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
						catch
						{
							lessTime += 0;
						}
					}
					if (session.end is null)
					{
						minutes += 0;
					}
					else
					{
						difference = (TimeSpan)(session.end - session.start);
						minutes += ((double)difference.TotalMinutes) - lessTime;
					}
				}
				catch
				{
					minutes += 0;
				}
			}
			return minutes;
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
		* Stats_Click - View stats for that particular project.
		*/
		private async void Stats_Click(object sender, EventArgs e)
		{
			overlayStats.IsVisible = true;
			TimeSpan ATT = TimeSpan.Zero;
			Double TPR = 0;
			TimeSpan auxPPR = TimeSpan.Zero;
			TimeSpan auxATP = TimeSpan.Zero;
			TimeSpan auxATT = TimeSpan.Zero;
			TimeSpan auxTPR = TimeSpan.Zero;

			auxATT = Project.AverageOfTasks(selectedProject, database);
			ATT += auxATT;
			auxTPR = Project.GetEstimation(selectedProject, database);

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

			ATT = TimeSpan.FromMinutes(ATT.TotalMinutes);
			AverageTimeTask.Text = "Task Average Time: " + ATT.ToString();
			TaskPrecictionRate.Text = "Task Estimation correctness: " + TPR.ToString() + "%";
		}

		/*
		* OnCancelButtonClicked2 - Close stats overlay.
		*/
		void OnCancelButtonClicked2(object sender, EventArgs args)
		{
			overlayStats.IsVisible = false;
		}

		/*
		* GetTaskEngageTime - Calculate time spend on a particular task based on it sessions.
		*/
		double GetTaskEngageTime(List<Session> sessionsL)
		{
			double minutes = 0;
			TimeSpan difference;
			foreach (var tempS in sessionsL)
			{
				try
				{
					double lessTime = 0;
					List<Interrupts> interruptsList = database.GetInterruptsOfSession(tempS.Id);
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
						catch
						{
							lessTime += 0;
						}
					}
					if (tempS.end is null)
					{
						minutes += 0;
					}
					else
					{
						difference = (TimeSpan)(tempS.end - tempS.start);
						minutes += ((double)difference.TotalMinutes) - lessTime;
					}
				}
				catch
				{
					minutes += 0;
				}
			}
			return minutes;
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
}