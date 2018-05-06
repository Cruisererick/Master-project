using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/*
	 * Settings contains the basic privacy setting for the application, 
	 * how much info should the application has access to, also how aggressive the 
	 * application should be with its notifications and finally how much time the 
	 * application should consider as idle time, currently not implemented.
	 */
	[Table("Settings")]
	public class Settings
    {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string IdleTime { get; set; }
		public string Agresives { get; set; }
		public string Privacy { get; set; }

		[OneToOne("O2OClassAKey", "BObject")]
		public User user { get; set; }
	}
}
