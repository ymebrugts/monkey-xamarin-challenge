using Acr.UserDialogs;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelMonkey.Data;
using TravelMonkey.Factories;
using TravelMonkey.Models;
using TravelMonkey.Services;
using TravelMonkey.State;
using Xamarin.Forms;

namespace TravelMonkey.ViewModels
{
    public class FaceValidationViewModel : BaseViewModel
    {
        private readonly ComputerVisionService _computerVisionService = new ComputerVisionService();

        public bool ShowImagePlaceholder => !ShowPhoto;
        public bool ShowPhoto => _photoSource != null;

        FaceClient faceClient;

        StreamImageSource _photoSource;
        MemoryStream _memoryStreamImage;
        bool _savePictureVisibility;
        bool _saveIsEnabled;

        string _cameraText;

        public bool SavePictureVisibility
        {
            get => _savePictureVisibility;
            set
            {
                if (Set(ref _savePictureVisibility, value))
                {
                    RaisePropertyChanged(nameof(SavePictureVisibility));
                }
            }
        }        
        public bool SaveIsEnabled
        {
            get => _saveIsEnabled;
            set
            {
                if (Set(ref _saveIsEnabled, value))
                {
                    RaisePropertyChanged(nameof(SaveIsEnabled));
                }
            }
        }

        public StreamImageSource PhotoSource
        {
            get => _photoSource;
            set
            {
                if (Set(ref _photoSource, value))
                {
                    RaisePropertyChanged(nameof(ShowPhoto));
                    RaisePropertyChanged(nameof(ShowImagePlaceholder));
                }
            }
        }
        public string CameraText
        {
            get => _cameraText;
            set
            {
                if (Set(ref _cameraText, value))
                {
                    RaisePropertyChanged(nameof(CameraText));
                }
            }
        }

        private bool _isPosting;
        public bool IsPosting
        {
            get => _isPosting;
            set => Set(ref _isPosting, value);
        }

        private Color _pictureAccentColor = Color.SteelBlue;
        public Color PictureAccentColor
        {
            get => _pictureAccentColor;
            set => Set(ref _pictureAccentColor, value);
        }

        private string _pictureDescription;
        public string PictureDescription
        {
            get => _pictureDescription;
            set => Set(ref _pictureDescription, value);
        }

        public Command TakePhotoCommandValidation { get; }
        public Command AddPictureCommand { get; }

        public FaceValidationViewModel()
        {
            CameraText = "You need to look like Travel Monkey! :D";
            TakePhotoCommandValidation = new Command(async () => await TakePhoto());
            AddPictureCommand = new Command(() =>
            {
                MockDataStore.Pictures.Add(new PictureEntry { Description = _pictureDescription, Image = _photoSource });
                MessagingCenter.Send(this, Constants.PictureAddedMessage);
            });

            var faceAPIFactory = new FaceAPIFactory();
            faceClient = faceAPIFactory.CreateFaceClient();

            Callbacks.Instance.Detect = new Func<MemoryStream, Task>(async (cameraFrameStream) =>
            {
                await Detect(cameraFrameStream);
            });
        }

        private async Task TakePhoto()
        {
            SaveIsEnabled = false;
            _memoryStreamImage.Seek(0, SeekOrigin.Begin);            
            PhotoSource = (StreamImageSource)ImageSource.FromStream(() =>(Stream) _memoryStreamImage);
            if (_photoSource != null)
            {
                
                await Post();
                MockDataStore.Pictures.Add(new PictureEntry { Description = _pictureDescription, Image = _photoSource });
                MessagingCenter.Send(this, Constants.PictureAddedMessage);
            } 
            
        }

        private async Task Post()
        {
            if (_memoryStreamImage == null)
            {
                await UserDialogs.Instance.AlertAsync("Please select an image first", "No image selected");
                return;
            }

            IsPosting = true;
            AddPictureResult result;
            try
            {
                _memoryStreamImage.Seek(0, SeekOrigin.Begin);
                using (var _tempMemoryStream = new MemoryStream()) { 
                    await _memoryStreamImage.CopyToAsync(_tempMemoryStream);
                    _memoryStreamImage.Seek(0, SeekOrigin.Begin);
                    _tempMemoryStream.Seek(0, SeekOrigin.Begin);
                    result = await _computerVisionService.AddPicture((Stream)_tempMemoryStream);
                }
                if (!result.Succeeded)
                {
                    MessagingCenter.Send(this, Constants.PictureFailedMessage);
                    return;
                }

                PictureAccentColor = result.AccentColor;

                PictureDescription = result.Description;

                if (!string.IsNullOrWhiteSpace(result.LandmarkDescription))
                    PictureDescription += $". {result.LandmarkDescription}";
            }
            finally
            {
                IsPosting = false;                
            }
        }


        public async Task Detect(Stream stream)
        {
            if (faceClient == null)
            {
                CameraText = "Face verification failed :(";
                return;
            }
            SaveIsEnabled = true;
            stream.Seek(0, SeekOrigin.Begin);
            _memoryStreamImage = new MemoryStream();
            await stream.CopyToAsync(_memoryStreamImage);
            stream.Seek(0, SeekOrigin.Begin);
            var facesResults = await faceClient.Face.DetectWithStreamAsync(stream, true, false, new FaceAttributeType[] { FaceAttributeType.HeadPose, FaceAttributeType.Smile, FaceAttributeType.Age, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Accessories });
            //IList<DetectedFace> facesResults = null;
            if (facesResults != null && facesResults.Count > 0)
            {
                await compareCameraPhotoToTravelMonkey(facesResults, (MemoryStream)stream);
            }
            else
            {
                CameraText = "We can't find a face :(\nRetrying now...";
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
                CameraText = "You need to look more like our mascot the Travel Monkey.\nTry putting on glasses";
                return;
            }

            if (glasses.Confidence < 0.8)
            {
                CameraText = "You need to look more like our mascot the Travel Monkey.\nWe are not confident enough you are wearing glasses";
                return;
            }

            if (headwear == null)
            {
                CameraText = "Stop tricking Travel Monkey!\nTry putting on a hat";
                return;
            }

            if (glasses.Confidence < 0.8)
            {
                CameraText = "Stop tricking Travel Monkey!\nWe are not confident enough you are wearing a hat";
                return;
            }

            if (headwear == null)
            {
                CameraText = "Stop tricking Travel Monkey!\nTry putting on a hat";
                return;
            }

            if (smile < 0.8)
            {
                CameraText = "You should smile more.\nYou are travelling, be happy!";
                return;
            }
            CameraText = "Wow are you Travel Monkey?";

            _memoryStreamImage.Seek(0, SeekOrigin.Begin);
            MessagingCenter.Send(this, Constants.StopCameraMessage);
            SavePictureVisibility = true;                


            //TODO: Save                                
            //await NextStep();
        }

        private async Task NextStep()
        {

        }









    }
}
