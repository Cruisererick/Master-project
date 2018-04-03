using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("Sessions")]
	public class Session
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime start { get; set; }
		public DateTime? end { get; set; }

		[ForeignKey(typeof(Project_Task))]
		public int taskId { get; set; }

		[ForeignKey(typeof(Project))]
		public int project { get; set; }

		[OneToMany]
		public List<Interrupts> interrupts { get; set; }

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
