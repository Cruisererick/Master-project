using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Model
{
	/*
	 * User has no real functionality outside being a possible class that it will 
	 * be needed for the scalability of the application, if the application is move 
	 * into and online environment where users can upload and their information to an 
	 * online server to check their information everywhere the connection between the 
	 * user and the information is already there.
	 */
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
