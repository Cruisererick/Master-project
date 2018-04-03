using System;
using System.Collections.Generic;
using System.Text;
using Final_Project.Model;

namespace Final_Project.Control
{
    class Verfiy_Location
    {
		public static bool locationChange(Session session, double latitude, double longitude, List<String> address, Database_Controller databaseAccess)
		{
			Location local = new Location(longitude, latitude, address[0]);
			session.locations = databaseAccess.GetLocationOfSession(session.Id);
			bool beenThere = false;
			if (session.locations.Count == 0)
			{
				local.sessionId = session.Id;
				local.Id = databaseAccess.SaveLocation(local);
				session.locations.Add(local);
				databaseAccess.UpdateSession(session);
				return true;
			}
			else
			{
				foreach (var tempLocation in session.locations)
				{
					//if (tempLocation.address.CompareTo(local.address) == 0)
					if (tempLocation.latitude == local.latitude && tempLocation.longitude == local.longitude)
					{
						beenThere = true;
					}
				}
			}
			

			if (beenThere == false)
			{
				local.sessionId = session.Id;
				local.Id = databaseAccess.SaveLocation(local);
				session.locations.Add(local);
				databaseAccess.UpdateSession(session);
			}

			return beenThere;
		}
    }
}
