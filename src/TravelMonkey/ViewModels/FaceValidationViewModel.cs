using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TravelMonkey.Data;
using TravelMonkey.Models;
using TravelMonkey.Services;
using Xamarin.Forms;

namespace TravelMonkey.ViewModels
{
    public class FaceValidationViewModel : BaseViewModel
    {
        private readonly ComputerVisionService _computerVisionService = new ComputerVisionService();

        public bool ShowImagePlaceholder => !ShowPhoto;
        public bool ShowPhoto => _photoSource != null;

        StreamImageSource _photoSource;
        MemoryStream _memoryStreamImage;
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

        public Command TakePhotoCommand { get; }
        public Command AddPictureCommand { get; }

        public FaceValidationViewModel()
        {
            TakePhotoCommand = new Command(async () => await TakePhoto());
            AddPictureCommand = new Command(() =>
            {
                MockDataStore.Pictures.Add(new PictureEntry { Description = _pictureDescription, Image = _photoSource });
                MessagingCenter.Send(this, Constants.PictureAddedMessage);
            });
        }

        private async Task TakePhoto()
        {
            if (PhotoSource != null)
            {
                await Post();
            }                
        }

        private async Task Post()
        {
            if (PhotoSource == null || _memoryStreamImage == null)
            {
                await UserDialogs.Instance.AlertAsync("Please select an image first", "No image selected");
                return;
            }

            IsPosting = true;

            try
            {
                var result = await _computerVisionService.AddPicture(_memoryStreamImage);

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
                _memoryStreamImage = null;
                PhotoSource = null;
            }
        }
    }
}
