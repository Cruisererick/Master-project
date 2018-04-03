using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	[Table("User")]
	public class User
    {
		[PrimaryKey]
		public string Id { get; set; }

		[OneToOne]
		public Settings settings { get; set; }

		[OneToMany]
		public List<Project> projects { get; set; }

		public User() { }
		public User(string Id, Settings settings, List<Project> projects)
		{
			this.Id = Id;
			this.settings = settings;
			this.projects = projects;
		}
	}
}
