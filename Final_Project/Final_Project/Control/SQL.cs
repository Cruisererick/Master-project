using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Control
{
	/* 
	* Interface require by Xamarin and android for the proper 
	* initialization of the database, Refer to the programmer manual for 
	* more information. 
	*/
	public interface SQL
    {
		SQLiteConnection GetConnection();
	}
}
