using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("Task")]
	public class Project_Task
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public double estimation { get; set; }


		[OneToMany]
		public List<Session> sessions { get; set; }

		[ForeignKey(typeof(Project))]
		public int projectId { get; set; }

		public Project_Task() { }
		public Project_Task(string name, string description, double estimation, List<Session> sessions)
		{
			this.name = name;
			this.description = description;
			this.estimation = estimation;
			this.sessions = sessions;
		}
	}
}
