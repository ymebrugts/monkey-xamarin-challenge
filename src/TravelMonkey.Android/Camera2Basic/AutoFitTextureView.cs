using System;
using System.Drawing.Drawing2D;
using Android.Content;
using Android.Util;
using Android.Views;

namespace TravelMonkey.Droid.Camera2Basic
{
	public class AutoFitTextureView : TextureView
	{
		private int mRatioWidth = 0;
		private int mRatioHeight = 0;

		public AutoFitTextureView(Context context)
			: this (context, null)
		{

		}
		public AutoFitTextureView (Context context, IAttributeSet attrs)
			: this (context, attrs, 0)
		{

		}
		public AutoFitTextureView (Context context, IAttributeSet attrs, int defStyle)
			: base (context, attrs, defStyle)
		{

		}

		public void SetAspectRatio(int width, int height)
		{
			if (width == 0 || height == 0)
				throw new ArgumentException ("Size cannot be negative.");
			mRatioWidth = width;
			mRatioHeight = height;
			RequestLayout();
		}	

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
			int width = MeasureSpec.GetSize (widthMeasureSpec);
			int height = MeasureSpec.GetSize (heightMeasureSpec);
			int ratiowidth = mRatioWidth;
			int ratioheight = mRatioHeight;
			if (0 == mRatioWidth || 0 == mRatioHeight) {
				SetMeasuredDimension (width, height);
			} else {
				var newWidth = (height * ratiowidth / ratioheight);
				var translate = (newWidth - width) / 2;
				this.TranslationX = -translate;
				SetMeasuredDimension (newWidth, height);
			}
			
		}
	}
}

