using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Final_Project.Control
{
    public interface SQL
    {
		SQLiteConnection GetConnection();
	}
}
