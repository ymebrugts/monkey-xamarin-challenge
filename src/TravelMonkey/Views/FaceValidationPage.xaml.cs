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

        private double? smile;
        private int width;
        private double smileThreshold = 0.7;
        private double confidenceTreshold = 0.4;
        Guid TravelMonkeyFaceID;
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


        private async Task processMonkey()
        {
            cameraText.Text = "Travel Monkey is currently being processed. \n You will be next.";

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream TravelMonkeyAsStream = assembly.GetManifestResourceStream("TravelMonkey.Resources.TravelMonkey.png");
            TravelMonkeyAsStream.Seek(0, SeekOrigin.Begin);
            var facesResults = await faceClient.Face.DetectWithStreamAsync(TravelMonkeyAsStream, true, false, new FaceAttributeType[] { FaceAttributeType.HeadPose, FaceAttributeType.Smile });
            if (facesResults[0].FaceId != null)
            {
                TravelMonkeyFaceID = (Guid)facesResults[0].FaceId;

            } else
            {
                cameraText.Text = "Could not detect face on Travel Monkey";
            }

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (TravelMonkeyFaceID == Guid.Empty)
            {
                await processMonkey();
            }                     
            this.Title = "Try to smile and look just like the Travel Monkey";
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
            Guid? identityPhotoGuid = TravelMonkeyFaceID;
            if (identityPhotoGuid == null)
            {
                cameraText.Text = "Travel Monkey is currently being processed. \n You will be next.";
                return;
            }

            var facesResults = await faceClient.Face.DetectWithStreamAsync(stream, true, false,  new FaceAttributeType[] { FaceAttributeType.HeadPose, FaceAttributeType.Smile, FaceAttributeType.Age, FaceAttributeType.Emotion, FaceAttributeType.Glasses } );
            if (facesResults != null && facesResults.Count > 0)
            {
                await compareCameraPhotoToIdentityPhoto(facesResults, (MemoryStream) stream);
            }
        }

        private async Task compareCameraPhotoToIdentityPhoto(IList<DetectedFace> faceResults, MemoryStream stream)
        {
            var selfieFaceId = faceResults[0].FaceId;
            
            //Smile and width of face from camera
            smile = faceResults[0].FaceAttributes.Smile;
            width = faceResults[0].FaceRectangle.Width;

            //get Id from face api from identitydocument photo
            if (smile < smileThreshold)
            {
                cameraText.Text = "You should smile more";
                return;
            }

            if (TravelMonkeyFaceID != null && selfieFaceId != null)
            {
                cameraText.Text = "Verifying";
                var identityPhotoGuid = TravelMonkeyFaceID;
                var selfieCameraFrameGuid = (Guid)selfieFaceId;
                var faceComparison = await faceClient.Face.VerifyFaceToFaceAsync(selfieCameraFrameGuid, identityPhotoGuid);
                if (faceComparison.Confidence > confidenceTreshold)
                {                    
                    StopCamera();
                    //TODO: Save
                                       
                    await NextStep();
                    
                } else
                {
                    cameraText.Text = "You need to look more like our mascot the Travel Monkey. \nTry putting on glasses";
                    
                }
            } else
            {
                cameraText.Text = "Verifying";                
            }            
        }

        private async Task NextStep()
        {
            
            
        }

        


    }


}