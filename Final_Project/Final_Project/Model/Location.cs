using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/* Author: Erick Lares
	 * Location is the geological information of where the session is being done.
	 */
	[Table("Location")]
	public class Location
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public double longitude { get; set; }
		public double latitude { get; set; }
		public string address { get; set; }

		//A location has a session they belong to.
		[ForeignKey(typeof(Project))]
		public int sessionId { get; set; }

		public Location() { }
		public Location(double longitude, double latitude, string address)
		{
			this.longitude = longitude;
			this.longitude = latitude;
			this.address = address;
		}
	}
}
