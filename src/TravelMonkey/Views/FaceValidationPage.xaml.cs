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

namespace TravelMonkey.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FaceValidationPage : ContentPage
    {       
        Callbacks callbacks = Callbacks.Instance;
       
        FaceClient faceClient;
     
        public FaceValidationPage()
        {
            InitializeComponent();
            var faceAPIFactory = new FaceAPIFactory();
            faceClient = faceAPIFactory.CreateFaceClient();

            callbacks.Detect = new Func<MemoryStream, Task>(async (cameraFrameStream) =>
            {
            await Detect(cameraFrameStream);                              
            });            

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

        private async Task Detect(Stream stream)
        {
            if (faceClient == null)
            {
                cameraText.Text = "Face verification failed :(";
                return;
            }
            var facesResults = await faceClient.Face.DetectWithStreamAsync(stream, true, false,  new FaceAttributeType[] { FaceAttributeType.HeadPose, FaceAttributeType.Smile, FaceAttributeType.Age, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Accessories } );
            facesResults = null;
            if (facesResults != null && facesResults.Count > 0)
            {
                await compareCameraPhotoToTravelMonkey(facesResults, (MemoryStream) stream);
            } else
            {
                cameraText.Text = "We can't find a face :(\nRetrying now...";
            }
        }

        private async Task compareCameraPhotoToTravelMonkey(IList<DetectedFace> faceResults, MemoryStream stream)
        {
            var selfieFaceId = faceResults[0].FaceId;
            
            //Smile and width of face from camera
            var smile = faceResults[0].FaceAttributes.Smile;
            var width = faceResults[0].FaceRectangle.Width;
            GlassesType? Glasses = faceResults[0].FaceAttributes.Glasses;

            var glasses = faceResults[0].FaceAttributes?.Accessories?.Where(c => c.Type == AccessoryType.Glasses).FirstOrDefault();
            var headwear = faceResults[0].FaceAttributes?.Accessories?.Where(c => c.Type == AccessoryType.HeadWear).FirstOrDefault();


            if (Glasses == null || !Glasses.HasValue || Glasses.Value == GlassesType.NoGlasses)
            {
                cameraText.Text = "You need to look more like our mascot the Travel Monkey.\nTry putting on glasses";
                return;
            }

            if (glasses.Confidence < 0.8)
            {
                cameraText.Text = "You need to look more like our mascot the Travel Monkey.\nWe are not confident enough you are wearing glasses";
                return;
            }

            if (headwear == null)
            {
                cameraText.Text = "Stop tricking Travel Monkey!\nTry putting on a hat";
                return;
            }

            if (glasses.Confidence < 0.8)
            {
                cameraText.Text = "Stop tricking Travel Monkey!\nWe are not confident enough you are wearing a hat";
                return;
            }

            if (headwear == null)
            {
                cameraText.Text = "Stop tricking Travel Monkey!\nTry putting on a hat";
                return;
            }

            if (smile < 0.8)
            {
                cameraText.Text = "You should smile more.\nYou are travelling, be happy!";
                return;
            }
            cameraText.Text = "Wow are you Travel Monkey?";
            await Task.Run(() =>
            {
                Task.Delay(3000);
                StopCamera();
                savePicture.IsVisible = true;
            });
            
            //TODO: Save                                
            //await NextStep();
        }

        private async Task NextStep()
        {
            
            
        }

        


    }


}