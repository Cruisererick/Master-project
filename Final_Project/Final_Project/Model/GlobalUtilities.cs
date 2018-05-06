using System;
using System.Collections.Generic;
using System.Text;
using XLabs.Platform.Device;
using XLabs.Ioc;
using XLabs.Platform.Device;

namespace Final_Project.Model
{
	/* Author: Erick Lares
	 * A utility class that has global variable related to the android devices to keep control on them.
	 * Global utilities have aceelTime the time interval we allow the application to consult the accelerometer, locationTime the time interval we allow the application to consult the GPS. 
	 * still, moving, AccelPresiscion, MovementTicks, StillTicks are control values for the accelerometer explained in View-Session Live.
	 * Accelerometer is the accelerometer object, LastMovement the last values for any movement in the accelerometer.
	 */

    public static class GlobalUtilities
    {
		public static int aceelTime = 333; //Frecuency of accelerometer usage in miliseconds.
		public static int locationTime = 300000; //Frecuency of accelerometer usage in miliseconds.
		public static int still = 0;// Still Buffer.
		public static int moving = 0;// Movement Buffer.
		public static IAccelerometer accelerometer;
		public static XLabs.Vector3 LastMovement;
		public static int AccelPresiscion = 2; //The number of decimals used by the accelerometer.
		public static int MovementTicks = 5; // Movement Buffer limit, increase the value to allow more movement.
		public static int StillTicks = 10; // Still Buffer limit, increase the value to allow for more time before recognizing that the device is not moving.

		/*
		 * setAccel - Methor to initialize the accelerometer.
		 */
		public static void setAccel()
		{
			if (!Resolver.IsSet)
			{
				var container = new SimpleContainer();
				container.Register<IAccelerometer, Accelerometer>();
				Resolver.SetResolver(container.GetResolver());
			}
			accelerometer = Resolver.Resolve<IAccelerometer>();
			accelerometer.Interval = AccelerometerInterval.Normal;
		}
	}
	
}
