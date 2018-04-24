using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("RunningInfo")]
	public class RunningInfo
    {
		[PrimaryKey]
		public int Id { get; set; }
		public bool notificationNeeded { get; set; }
		public bool background { get; set; }

		public RunningInfo() { }
		public RunningInfo(int Id, bool notificationNeeded, bool background)
		{
			this.Id = Id;
			this.notificationNeeded = notificationNeeded;
			this.background = background;
		}
	}
}
