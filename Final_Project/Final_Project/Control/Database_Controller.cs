using Final_Project.Model;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Final_Project.Control
{
	/* Author: Erick Lares
	 * Class that handles all connections to the database, 
	 * including initialization of the database, creating of the tables, 
	 * any queries require by the application. 
	 */
    public class Database_Controller
    {
		static object locker = new object();
		SQLiteConnection database;

		/* Database_Controller - initialization of the database.
		 */
		public Database_Controller()
		{
			database = DependencyService.Get<SQL>().GetConnection();
			//DropTables();
			CreateTables();
		}

		/* CreateTables - Method to create all the tables.
		 */
		public void CreateTables()
		{
			database.CreateTable<User>();
			database.CreateTable<Settings>();
			database.CreateTable<Project>();
			database.CreateTable<Project_Task>();
			database.CreateTable<Session>();
			database.CreateTable<Interrupts>();
			database.CreateTable<Location>();
			database.CreateTable<RunningInfo>();
		}

		/* DropTables - Method to delete all the tables.
		 */
		public void DropTables()
		{
			database.DropTable<User>();
			database.DropTable<Settings>();
			database.DropTable<Project>();
			database.DropTable<Project_Task>();
			database.DropTable<Session>();
			database.DropTable<Interrupts>();
			database.DropTable<Location>();
			database.DropTable<RunningInfo>();
		}

		/* SaveProject - Insert one project into the database.
		 * @param project - The Project.
		 * @return - Autoincrement Id of the project.
		 */
		public int SaveProject(Project project)
		{
			lock (locker)
			{
				if (project.name != "")
				{
					try
					{
						database.Insert(project);
						SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
						cmd.CommandText = "SELECT last_insert_rowid()";
						Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
						int LastRowID = (int)LastRowID64;
						return LastRowID;
					}
					catch (Exception e)
					{
						database.Update(project);
						return project.Id;
					}
				}
				else
					return -1;
			}
		}

		/* SaveUser - Insert one user into the database.
		 * @param SaveUser - The user.
		 * @return - Autoincrement Id of the user.
		 */
		public string SaveUser(User user)
		{
			lock (locker)
			{
				if (user.Id != "")
				{
					try
					{
						return database.Insert(user).ToString();
					}
					catch (Exception e)
					{
						return database.Update(user).ToString();
					}
				}
				else
				{
					return database.Insert(user).ToString();
				}


			}
		}

		/* SaveTask - Insert project_task into the database.
		 * @param task - The project_task.
		 * @return - Autoincrement Id of the Project_Task.
		 */
		public int SaveTask(Project_Task task)
		{
			lock (locker)
			{
				if (task.name != "")
				{
					try
					{
						database.Insert(task);
						SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
						cmd.CommandText = "SELECT last_insert_rowid()";
						Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
						int LastRowID = (int)LastRowID64;

						return LastRowID;
					}
					catch (Exception e)
					{
						database.Update(task);
						return task.Id;
					}
				}
				else
				{
					return -1;
				}


			}
		}

		/* SaveSession - Insert session into the database.
		 * @param session - The session.
		 * @return - Autoincrement Id of the Session.
		 */
		public int SaveSession(Session session)
		{
			lock (locker)
			{
				try
				{
					database.Insert(session);
					SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
					cmd.CommandText = "SELECT last_insert_rowid()";
					Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
					int LastRowID = (int)LastRowID64;

					return LastRowID;
				}
				catch (Exception e)
				{
					database.Update(session);
					return session.Id;
				}
			}
		}

		/* SaveInterrupt - Insert interrupts into the database.
		 * @param interrupts - The interrupts.
		 * @return - Autoincrement Id of the Interrupts.
		 */
		public int SaveInterrupt(Interrupts interrupts)
		{
			lock (locker)
			{
				try
				{
					database.Insert(interrupts);
					SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
					cmd.CommandText = "SELECT last_insert_rowid()";
					Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
					int LastRowID = (int)LastRowID64;

					return LastRowID;
				}
				catch (Exception e)
				{
					database.Update(interrupts);
					return interrupts.Id;
				}
			}
		}

		/* SaveLocation - Insert location into the database.
		 * @param location - The location.
		 * @return - Autoincrement Id of the Location.
		 */
		public int SaveLocation(Location location)
		{
			lock (locker)
			{
				try
				{
					database.Insert(location);
					SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
					cmd.CommandText = "SELECT last_insert_rowid()";
					Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
					int LastRowID = (int)LastRowID64;

					return LastRowID;
				}
				catch (Exception e)
				{
					database.Update(location);
					return location.Id;
				}
			}
		}

		/* GetProject - Get a list of all the project in the database.
		 * @return - A list of projects.
		 */
		public List<Project> GetProject()
		{
			List<Project> ProjectL = new List<Project>();
			lock (locker)
			{
				if (database.Table<Project>().Count() == 0)
				{
					return null;
				}
				else
				{
					ProjectL = database.Query<Project>("SELECT * from Project ");
					return ProjectL;
				}

			}
		}

		/* GetOneProject - Get one project that match the id provided.
		 * @param id - Project id.
		 * @return - A project.
		 */
		public Project GetOneProject(int id)
		{
			List<Project> ProjectL = new List<Project>();
			lock (locker)
			{
				if (database.Table<Project>().Count() == 0)
				{
					return null;
				}
				else
				{
					ProjectL = database.Query<Project>("SELECT * from Project "+
													   "WHERE Id="+ id);
					return ProjectL[0];
				}

			}
		}

		/* GetOneTask - Get one Project_Task that match the id provided.
		 * @param id - Project_Task id.
		 * @return - A Project_Task.
		 */
		public Project_Task GetOneTask(int id)
		{
			List<Project_Task> TaskL = new List<Project_Task>();
			lock (locker)
			{
				if (database.Table<Project_Task>().Count() == 0)
				{
					return null;
				}
				else
				{
					TaskL = database.Query<Project_Task>("SELECT * from Task " +
														 "WHERE Id=" + id);
					return TaskL[0];
				}

			}
		}

		/* GetTaskOfProjects - Get all the Project_Task related to 
		 * a specific project.
		 * @param id - Project id.
		 * @return - A list of Project_Task.
		 */
		public List<Project_Task> GetTaskOfProjects(int id)
		{
			List<Project_Task> ProjectL = new List<Project_Task>();
			lock (locker)
			{
				if (database.Table<Project_Task>().Count() == 0)
				{
					return ProjectL;
				}
				else
				{
					ProjectL = database.Query<Project_Task>("SELECT * from Task " +
															"WHERE projectId='" + id + "'");
					return ProjectL;
				}

			}
		}

		/* GetSessionsOfProjects - Get all the Session related to 
		 * a specific project.
		 * @param id - Project id.
		 * @return - A list of Session.
		 */
		public List<Session> GetSessionsOfProjects(int id)
		{
			List<Session> SessionsL = new List<Session>();
			lock (locker)
			{
				if (database.Table<Project_Task>().Count() == 0)
				{
					return null;
				}
				else
				{
					SessionsL = database.Query<Session>("SELECT * from Sessions " +
														"WHERE project=" + id);
					return SessionsL;
				}

			}
		}

		/* GetSessionsOfTask - Get all the Session related to 
		 * a specific Project_Task.
		 * @param id - Project_Task id.
		 * @return - A list of Session.
		 */
		public List<Session> GetSessionsOfTask(int id)
		{
			List<Session> SessionsL = new List<Session>();
			lock (locker)
			{
				if (database.Table<Project_Task>().Count() == 0)
				{
					return SessionsL;
				}
				else
				{
					SessionsL = database.Query<Session>("SELECT * from Sessions " +
														"WHERE taskId=" + id);
					return SessionsL;
				}

			}
		}

		/* GetLocationOfSession - Get all the locations related to 
		 * a session.
		 * @param id - session id.
		 * @return - A list of Location.
		 */
		public List<Location> GetLocationOfSession(int id)
		{
			List<Location> LocationL = new List<Location>();
			lock (locker)
			{
				if (database.Table<Location>().Count() == 0)
				{
					return LocationL;
				}
				else
				{
					LocationL = database.Query<Location>("SELECT * from Location " +
														 "WHERE sessionid=" + id);
					return LocationL;
				}

			}
		}

		/* GetUser - Get the default user.
		 * @return - Default user.
		 */
		public User GetUser()
		{
			List<User> UserL = new List<User>();
			lock (locker)
			{
				if (database.Table<User>().Count() == 0)
				{
					return null;
				}
				else
				{
					UserL = database.Query<User>("SELECT * from User " +
												"WHERE Id='Erick'");
					return UserL[0];
				}

			}
		}

		/* GetProjectUser - Get all the project of a specific 
		 * user.
		 * @param User - User name.
		 * @return - A list of Project.
		 */
		public List<Project> GetProjectUser(string User)
		{
			List<Project> ProjectL = new List<Project>();
			lock (locker)
			{
				if (database.Table<User>().Count() == 0)
				{
					return null;
				}
				else
				{
					ProjectL = database.Query<Project>("SELECT * from Project " +
														"WHERE Project.userId='Erick'");
					return ProjectL;
				}
			}
		}

		/* AddProjectToUser - Update the projects of a user.
		 * @param user - user object that contains the projects.
		 */
		public void AddProjectToUser(User user)
		{
			lock (locker)
			{
				if (database.Table<User>().Count() == 0)
				{
					return;
				}
				else
				{
					database.UpdateWithChildren(user);
				}

			}
		}

		/* AddTaskToProject - Update the project_tasks of a project.
		 * @param project - Project object to be updated.
		 */
		public void AddTaskToProject(Project project)
		{
			lock (locker)
			{
				if (database.Table<Project>().Count() == 0)
				{
					return;
				}
				else
				{
					database.UpdateWithChildren(project);
				}

			}
		}

		/* GetliveSession - Get the session that has no end time, at all times
		 * there can only be one session with no end time, hence a live session.
		 * return - A live session.
		 */
		public Session GetliveSession()
		{
			List<Session> SessionL = new List<Session>();
			lock (locker)
			{
				if (database.Table<Session>().Count() == 0)
				{
					return null;
				}
				else
				{
					SessionL = database.Query<Session>("SELECT * from Sessions " +
													   "WHERE end IS NULL");
					if (SessionL.Count > 0)
					{
						return SessionL[0];
					}
					else
					{
						return null;
					}
				}

			}
		}

		/* UpdateSession - Update a specific session.
		 * @param session - session to be updated.
		 */
		public void UpdateSession(Session session)
		{
			lock (locker)
			{
				if (database.Table<Session>().Count() == 0)
				{
					return;
				}
				else
				{
					database.Update(session);
				}
			}
		}

		/* UpdateInterrupt - Update a specific interrupt.
		 * @param interrupt - interrupt to be updated.
		 */
		public void UpdateInterrupt(Interrupts interrupt)
		{
			lock (locker)
			{
				if (database.Table<Session>().Count() == 0)
				{
					return;
				}
				else
				{
					database.Update(interrupt);
				}
			}
		}

		/* GetInterruptsOfSession - Get all the interrupts of one session.
		 * @param sessionId - Session id.
		 * return - A list of Interrupts.
		 */
		public List<Interrupts> GetInterruptsOfSession(int sessionId)
		{
			List<Interrupts> interruptL = new List<Interrupts>();
			lock (locker)
			{
				if (database.Table<Interrupts>().Count() == 0)
				{
					return interruptL;
				}
				else
				{
					interruptL = database.Query<Interrupts>("SELECT * from Interrupts " +
															"WHERE Interrupts.sessionId='"+ sessionId + "'");
					return interruptL;
				}
			}
		}

		/* GetliveInterrupt - Get the Interrupt that has no end time, at all times
		 * there can only be one Interrupt with no end time, hence a live Interrupt.
		 * return - A live interrupt.
		 */
		public Interrupts GetliveInterrupt()
		{
			List<Interrupts> InterruptL = new List<Interrupts>();
			lock (locker)
			{
				if (database.Table<Interrupts>().Count() == 0)
				{
					return null;
				}
				else
				{
					InterruptL = database.Query<Interrupts>("SELECT * from Interrupts " +
															"WHERE end IS NULL");
					if (InterruptL.Count > 0)
					{
						return InterruptL[0];
					}
					else
					{
						return null;
					}
				}

			}
		}

		/* SaveRunningInfo - Insert a RunningInfo into the database.
		 * @param info - RunningInfo to be inserted.
		 * return - Autoincrement Id of the RunningInfo.
		 */
		public int SaveRunningInfo(RunningInfo info)
		{
			lock (locker)
			{
				if (info.Id != -1)
				{
					try
					{
						database.Insert(info);
						SQLiteCommand cmd = database.CreateCommand("SELECT last_insert_rowid()");
						cmd.CommandText = "SELECT last_insert_rowid()";
						Int64 LastRowID64 = cmd.ExecuteScalar<Int64>();
						int LastRowID = (int)LastRowID64;
						return LastRowID;
					}
					catch (Exception e)
					{
						database.Update(info);
						return info.Id;
					}
				}
				else
					return -1;
			}
		}

		/* getRunningInfo - Get the one specific RunningInfo.
		 * @param id - Id of the RunningInfo.
		 * return - RunningInfo.
		 */
		public RunningInfo getRunningInfo(int id)
		{
			List<RunningInfo> RunningInfoL = new List<RunningInfo>();
			lock (locker)
			{
				if (database.Table<RunningInfo>().Count() == 0)
				{
					return null;
				}
				else
				{
					RunningInfoL = database.Query<RunningInfo>("SELECT * from RunningInfo " +
															   "WHERE Id=" + id);
					return RunningInfoL[0];
				}

			}
		}

		/* UpdateRunningInfo - Update specific running info.
		 * @param info - RunningInfo to be update.
		 */
		public void UpdateRunningInfo(RunningInfo info)
		{
			lock (locker)
			{
				if (database.Table<RunningInfo>().Count() == 0)
				{
					return;
				}
				else
				{
					database.Update(info);
				}
			}
		}

	}
}
