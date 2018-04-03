using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{

	[Table("Project")]
	public class Project
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string name { get; set; }
		public string description { get; set; }

		[ForeignKey(typeof(User))]
		public string userId { get; set; }

		[OneToMany]
		public List<Project_Task> tasks { get; set; }

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
	}
}
