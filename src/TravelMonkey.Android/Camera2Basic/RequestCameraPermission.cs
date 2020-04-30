using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace TravelMonkey.Droid.Camera2Basic
{
	class RequestCameraPermission
	{
		public static async Task<bool> RequestPermissionsAsync()
		{
			var camera = await Permissions.RequestAsync<Permissions.Camera>();

			if (camera != PermissionStatus.Granted)
				return false;

			return true;
		}
	}
}