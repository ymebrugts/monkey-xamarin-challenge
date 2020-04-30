using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TravelMonkey.Services;

namespace TravelMonkey.Droid.Services
{
    public class MediaFolder : IMediaFolder
    {
        public string Path
        {
            get
            {
                return Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;
            }
        }
    }
}