using TravelMonkey.State;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TravelMonkey.Factories;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Resources;
using System.Reflection;
using TravelMonkey.ViewModels;

namespace TravelMonkey.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FaceValidationPage : ContentPage
    {       

     
        public FaceValidationPage()
        {
            InitializeComponent();
            BindingContext = new FaceValidationViewModel();
            MessagingCenter.Subscribe<FaceValidationViewModel>(this, Constants.StopCameraMessage, (vm) => StopCamera());
            MessagingCenter.Subscribe<FaceValidationViewModel>(this, Constants.PictureAddedMessage, async (vm) => await Navigation.PopModalAsync(true));
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopCamera();
        }

        public void StopCamera()
        {
            Camera.StopRecording?.Invoke();
        }






    }


}