using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/* Author : Erick Lares
	 * Running info is a class that server as a bridge between Xamarin and the 
	 * information about the application that android provides, more specific 
	 * if the application is in background and if a notification is needed.
	 * Every time android detects that the application has gone to sleep the
	 * background bool will be true, as soon as the application resume, 
	 * this will value will be false, a notification is needed if the
	 * application is in background and the program detect that an 
	 * interrupt has happened, this will produce a notification.
	 */

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
