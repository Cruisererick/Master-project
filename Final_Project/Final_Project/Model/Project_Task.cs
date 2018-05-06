using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/* Author: Erick Lares
	 * A task is one small functionality or activity that need to be done for a project, 
	 * all tasks have one estimation of how much time they will need to be complete.
	 * A task has a name, description, a list of Sessions and a project they belong to. 
	 */

	[Table("Task")]
	public class Project_Task
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public double estimation { get; set; }

		//A task can have one or many Sessions.
		[OneToMany]
		public List<Session> sessions { get; set; }

		//A task has one project they belong to.
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
