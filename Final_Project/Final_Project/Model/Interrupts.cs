using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("Interrupts")]
	public class Interrupts
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime start { get; set; }
		public DateTime? end { get; set; }

		[ForeignKey(typeof(Session))]
		public int sessionId { get; set; }

		public Interrupts() { }
		public Interrupts(DateTime start, DateTime end, int sessionId)
		{
			this.start = start;
			this.end = end;
			this.sessionId = sessionId;
		}
	}
}
