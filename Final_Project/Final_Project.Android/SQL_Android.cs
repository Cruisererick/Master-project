using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Final_Project.Control;
using Final_Project.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQL_Android))]

/* 
 * Method require by Xamarin and android for the proper 
 * initialization of the database, Refer to the programmer manual for 
 * more information. 
 */
namespace Final_Project.Droid
{
	public class SQL_Android : SQL
	{
		public SQL_Android() { }
		public SQLite.SQLiteConnection GetConnection()
		{
			var sqliteFilename = "Projects_Database0.db3";
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			Console.WriteLine(documentsPath);
			var path = Path.Combine(documentsPath, sqliteFilename);
			var conn = new SQLite.SQLiteConnection(path);

			return conn;
		}
	}
}