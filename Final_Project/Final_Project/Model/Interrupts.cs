using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/* Author: Erick Lares.
	 * An interrupt is defined as anything that could cause a work session to temporary stop, 
	 * currently only tree types of interrupts are implemented: 
	 * Changing locations.
	 * Irregular movement detected. 
	 * Manual user interrupt. 
	 */

	[Table("Interrupts")]
	public class Interrupts
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public DateTime start { get; set; }
		public DateTime? end { get; set; }
		public string reason { get; set; }

		//An interrupt has a session they belong to.
		[ForeignKey(typeof(Session))]
		public int sessionId { get; set; }

		public Interrupts() { }
		public Interrupts(DateTime start, DateTime end, int sessionId, string reason)
		{
			this.start = start;
			this.end = end;
			this.sessionId = sessionId;
			this.reason = reason;
		}
	}
}
