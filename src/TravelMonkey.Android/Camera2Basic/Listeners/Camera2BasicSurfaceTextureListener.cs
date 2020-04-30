using Android.Graphics;
using Android.Views;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelMonkey.State;
using Xamarin.Forms;

namespace TravelMonkey.Droid.Camera2Basic.Listeners
{
    public class Camera2BasicSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly ICamera2Basic owner;
        object lockObject = new object();
        AutoFitTextureView _textureView;
        Callbacks callbacks = Callbacks.Instance;
        bool skippedFirstFrame;
        bool processing = false;
        

        public Camera2BasicSurfaceTextureListener(ICamera2Basic owner, AutoFitTextureView textureView)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
            _textureView = textureView;
            skippedFirstFrame = false;
            
        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
           owner.OpenCamera(width, height);
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            //owner.ConfigureTransform(width, height);
        }

        public async void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {
            if (!skippedFirstFrame)
            {
                skippedFirstFrame = true;
                return;
            }
            if (processing)
            {
                return;
            }
            

            processing = true;
            Bitmap bitmap = _textureView.Bitmap;
            MemoryStream videoFrameStream = new MemoryStream();
            await bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, videoFrameStream);
            videoFrameStream.Seek(0, SeekOrigin.Begin);
            await callbacks.Detect(videoFrameStream);
            await Task.Run(async () =>
            {
                await Task.Delay(100);
                videoFrameStream.Dispose();
                processing = false;
            });
            
        }
    }
}