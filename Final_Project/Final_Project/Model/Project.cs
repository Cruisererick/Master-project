using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Final_Project.Control;

namespace Final_Project.Model
{
	/* Author: Erick Lares
	 * A project is the focus of the user, a user will work on a project through it tasks, 
	 * a user will dedicate time too a project and project object that wraps all that information.
	 * An Project has a name, description, a list of tasks, a list of Sessions and a user that it belongs to.
	 */
	[Table("Project")]
	public class Project
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string name { get; set; }
		public string description { get; set; }

		// A user can have one or many projects.
		[ForeignKey(typeof(User))]
		public string userId { get; set; }
		
		//A Project can have one or many tasks.
		[OneToMany]
		public List<Project_Task> tasks { get; set; }

		//A Project can have one or many Sessions.
		[OneToMany]
		public List<Session> Sessions { get; set; }

		public Project() { }
		public Project(string name, string description, List<Project_Task> tasks, List<Session> Sessions)
		{
			this.name = name;
			this.description = description;
			this.tasks = tasks;
			this.Sessions = Sessions;
		}

		/*
		 * GetEstimation - Get the overall estimation for one project,
		 * This methos simply add all the estimations in the tasks of the project.
		 * @param project -The Project.
		 * @param database -The database.
		 */
		public static TimeSpan GetEstimation(Project project, Database_Controller database)
		{
			double minutes = 0;
			project.tasks = database.GetTaskOfProjects(project.Id);
			foreach (var projectTask in project.tasks)
			{
				minutes += projectTask.estimation;
			}
			return TimeSpan.FromMinutes(minutes);
		}

		/*
		 * GetEngageTime - Get the total time the user has work on this particular
		 * project, this methos add the time in all the sessions subtracting the 
		 * interrupts.
		 * @param selectedProject - The Project.
		 * @param database - The database.
		 */
		public static TimeSpan GetEngageTime(Project selectedProject, Database_Controller database)
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
			return TimeSpan.FromMinutes(minutes);
		}

		/*
		 * AverageOfTasks - Get the average time spend in tasks.
		 * @param selectedProject -The Project.
		 * @param database -The database.
		 */
		public static TimeSpan AverageOfTasks(Project selectedProject, Database_Controller database)
		{
			double minutes = 0;
			double Final = 0;
			TimeSpan difference;
			List<Session> projectSessions = database.GetSessionsOfProjects(selectedProject.Id);
			selectedProject.tasks = database.GetTaskOfProjects(selectedProject.Id);
			foreach (var tempTask in selectedProject.tasks)
			{
				tempTask.sessions = database.GetSessionsOfTask(tempTask.Id);
				foreach (var tempSession in tempTask.sessions)
				{
					minutes = 0;
					try
					{
						double lessTime = 0;
						List<Interrupts> interruptsList = database.GetInterruptsOfSession(tempSession.Id);
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
						if (tempSession.end is null)
						{
							minutes += 0;
						}
						else
						{
							difference = (TimeSpan)(tempSession.end - tempSession.start);
							minutes += ((double)difference.TotalMinutes) - lessTime;
						}
					}
					catch
					{
						minutes += 0;
					}
					Final += minutes;
				}
				
			}
			return TimeSpan.FromMinutes(Final/selectedProject.tasks.Count);
		}

		/*
		 * AverageOfTasksEstimation - Get the average estimation time for all tasks in the project.
		 * @param project -The Project.
		 * @param database -The database.
		 */
		public static TimeSpan AverageOfTasksEstimation(Project project, Database_Controller database)
		{
			double minutes = 0;
			project.tasks = database.GetTaskOfProjects(project.Id);
			foreach (var projectTask in project.tasks)
			{
				minutes += projectTask.estimation;
			}
			return TimeSpan.FromMinutes(minutes/ project.tasks.Count);
		}
	}
}
