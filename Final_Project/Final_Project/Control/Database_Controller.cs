using Final_Project.Model;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Final_Project.Control
{
    public class Database_Controller
    {
		static object locker = new object();
		SQLiteConnection database;

		public Database_Controller()
		{
			database = DependencyService.Get<SQL>().GetConnection();
			CreateTables();
		}

		public void CreateTables()
		{
			database.CreateTable<User>();
			database.CreateTable<Settings>();
			database.CreateTable<Project>();
			database.CreateTable<Project_Task>();
			database.CreateTable<Session>();
			database.CreateTable<Interrupts>();
			database.CreateTable<Location>();
		}

		public void DropTables()
		{
			database.DropTable<User>();
			database.DropTable<Settings>();
			database.DropTable<Project>();
			database.DropTable<Project_Task>();
			database.DropTable<Session>();
			database.DropTable<Interrupts>();
			database.DropTable<Location>();
		}

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
	}
}
