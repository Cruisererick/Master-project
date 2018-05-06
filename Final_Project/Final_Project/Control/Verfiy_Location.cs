using System;
using System.Collections.Generic;
using System.Text;
using Final_Project.Model;

namespace Final_Project.Control
{
    class Verfiy_Location
    {
		/* Author: Erick Lares.
		 * locationChange - Public method to verify if a session has change 
		 * locations, if the location has change the location is save in the 
		 * database and the change is returned, else the fact that the location 
		 * hasn’t change is returned. 
		 * @param session - The session.
		 * @param latitude - current latitude.
		 * @param longitude - current longitude.
		 * @param address - list of previouly visited address.
		 * @param database - database.
		 * Return - bool true if location change, false if location did not change.
		 */
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
