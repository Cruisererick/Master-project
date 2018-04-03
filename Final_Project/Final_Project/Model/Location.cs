using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("Location")]
	public class Location
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public double longitude { get; set; }
		public double latitude { get; set; }
		public string address { get; set; }

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
