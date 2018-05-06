using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/* Author: Erick Lares
	 * A session is the time spend doing a task, a session focus only on one task,
	 * the session belongs to the related task as well as to the project of said task.
	 * A session has a start time, end time, a list of interrupts 
	 * and a list of location where the sessions is being done.
	 */

	[Table("Sessions")]
	public class Session
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime start { get; set; }
		public DateTime? end { get; set; }

		//A session has one task they belong to.
		[ForeignKey(typeof(Project_Task))]
		public int taskId { get; set; }

		//A session has one project they belong to.
		[ForeignKey(typeof(Project))]
		public int project { get; set; }

		//A session can have none or many interrupts
		[OneToMany]
		public List<Interrupts> interrupts { get; set; }

		//A session can have one or many locations.
		[OneToMany]
		public List<Location> locations { get; set; }

		public Session() { }
		public Session(DateTime start, DateTime end, int task)
		{
			this.start = start;
			this.end = end;
			this.taskId = task;
			this.start = start;
		}
	}
}
