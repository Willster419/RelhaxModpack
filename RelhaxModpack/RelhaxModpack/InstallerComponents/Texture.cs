using System.Drawing;

namespace RelhaxModpack
{
    public class Texture
    {
        //name of the texture
        public string name { get; set; }
        //x position of the texture in the atlas image
        public int x { get; set; }
        //y position of the texture in the atlas image
        public int y { get; set; }
        //width of the texture in the atlas image
        public int width { get; set; }
        //height of the texture in the atlas image
        public int height { get; set; }
        //the actual bitmap in memory of the image
        public Bitmap AtlasImage { get; set; }

        public Rectangle Rect;
        // internal Rectangle rect
        private Rectangle rect
        {
            get
            {
                Rect.X = x;
                Rect.Y = y;
                Rect.Width = width;
                Rect.Height = height;
                return Rect;
            }
            set
            {
                Rect.X = value.X;
                Rect.Y = value.Y;
                Rect.Width = value.Width;
                Rect.Height = value.Height;
            }
        }
    }
}
