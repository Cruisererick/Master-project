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

namespace Final_Project.Visual
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Project_View_Page : ContentPage
	{
		Project selectedProject;
		Project_Task tapped;
		Database_Controller database;
		ObservableCollection<Project_Task> list = new ObservableCollection<Project_Task>();
		bool LiveSession = false;
		Session live;
		bool getlocation;
		bool gettingLocation;
		Interrupts interrupt;
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
			Estimation_Time.Text += Math.Round(auxEstimation, 3).ToString();
			Engage_Time.Text += Math.Round(auxSpend, 3).ToString();
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
				}
			}
			else
			{
				Resume_Session.IsVisible = false;
				LiveSession = false;
			}
		}

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
			Name.Text += temp_task.name;
			Decription.Text += temp_task.description;
			Estimation.Text += temp_task.estimation.ToString();
			tapped = temp_task;
			overlay.IsVisible = true;
			Tasks_List.IsVisible = false;
			View_Sessions.IsVisible = false;
			Delete_Project.IsVisible = false;
			Name.Focus();
		}

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

		void OnCancelButtonClicked(object sender, EventArgs args)
		{
			overlay.IsVisible = false;
			Tasks_List.IsVisible = true;
			View_Sessions.IsVisible = true;
			Delete_Project.IsVisible = true;
			tapped = null;
		}

		private async void Resume_Click(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				await Navigation.PushModalAsync(new SessionLive(database));
			}
		}

		private async void ViewSessionsClick(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				List<Session> projectSessions = database.GetSessionsOfProjects(selectedProject.Id);
				await Navigation.PushModalAsync(new View_Sessions(projectSessions, database));
			}
		}

		private async void ViewTaskSessionsClick(object sender, EventArgs e)
		{
			if (preventMovement())
			{
				List<Session> taskSessions = database.GetSessionsOfTask(tapped.Id);
				await Navigation.PushModalAsync(new View_Sessions(taskSessions, database));
			}
		}

		double GetEstimation(Project project)
		{
			double minutes = 0;
			foreach (var projectTask in project.tasks)
			{
				minutes += projectTask.estimation;
			}
			return minutes;
		}

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
}