using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZRGPictureBox
{
    public enum enBitmapOriginPosition
    {
        TopLeft = 0,
        Custom = 4
    }

    public class cBackImageGraphics
    {
        internal Point Origin;
        private Bitmap BitmapImage;
        private enBitmapOriginPosition BitmapOrigin;
        private double PixelWidth = 100.0;

        private double PixelHeight = 100.0;

        private cBackImageGraphics()
        {
        }

        public cBackImageGraphics(Bitmap BitmapImg, int OriginX, int OriginY, enBitmapOriginPosition OriginPosition, double Pixel_Width, double Pixel_Height)
        {
            BitmapImage = BitmapImg;
            Origin.X = OriginX;
            Origin.Y = OriginY;
            BitmapOrigin = OriginPosition;
            PixelWidth = Pixel_Width;
            PixelHeight = Pixel_Height;
            if (PixelWidth < 10)
            {
                PixelHeight = 10;
            }
            if (PixelHeight < 10)
            {
                PixelHeight = 10;
            }
        }

        public void Draw(Graphics GR)
        {
            if (BitmapImage == null)
            {
                return;
            }
            GR.DrawImage(BitmapImage, new Rectangle(Origin.X, Origin.Y, (int)(BitmapImage.Width * PixelWidth), (int)(BitmapImage.Height * PixelWidth)), 0, 0, BitmapImage.Width, BitmapImage.Height, GraphicsUnit.Pixel);
        }

        public void Dispose()
        {
            try
            {
                this.BitmapImage.Dispose();
                this.BitmapImage = null;
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
