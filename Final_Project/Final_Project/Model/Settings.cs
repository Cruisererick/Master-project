using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
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
