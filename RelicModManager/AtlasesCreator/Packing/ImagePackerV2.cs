using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RelhaxModpack.AtlasesCreator
{
    // https://bitbucket.org/Ktulho/ccatlas/src/9e66e3d088a0b4fb29807edb441f34e3932322f8/CreationAtlas.pas?at=default&fileviewer=file-view-default

    public class ImagePackerV2
    {
        int I = 0;
        int J = 0;
        int K = 0;
        int len = 0;
        int CurrentY = 0;

        // some dictionaries to hold the image sizes and destination rectangles
        private readonly Dictionary<Texture, Size> imageSizes = new Dictionary<Texture, Size>();
        private readonly Dictionary<Texture, Rectangle> imagePlacement = new Dictionary<Texture, Rectangle>();

        // Rectangle[] TakePlace = null;
        Texture[] TakePlace = null;
        Texture[] ArrayImage = null;

        /// <summary>
        /// Packs a collection of images into a single image.
        /// </summary>
        /// <param name="imageFiles">The list of Textures of the images to be combined.</param>
        /// <param name="requirePowerOfTwo">Whether or not the output image must have a power of two size.</param>
        /// <param name="requireSquareImage">Whether or not the output image must be a square.</param>
        /// <param name="fastImagePacker">Accept the first successfull image packing.</param>
        /// <param name="TextureWidth">The maximum width of the output image.</param>
        /// <param name="TextureHeight">The maximum height of the output image.</param>
        /// <param name="imagePadding">The amount of blank space to insert in between individual images.</param>
        /// <param name="generateMap">Whether or not to generate the map dictionary.</param>
        /// <param name="outputImage">The resulting output image.</param>
        /// <param name="outputMap">The resulting output map of placement rectangles for the images.</param>
        /// <returns>0 if the packing was successful, error code otherwise.</returns>
        public int PackImage(
            IEnumerable<Texture> imageFiles,
            Atlas.State requirePowerOfTwo,
            Atlas.State requireSquareImage,
            bool fastImagePacker,
            int TextureWidth,
            int TextureHeight,
            int imagePadding,
            bool generateMap,
            out Bitmap outputImage,
            out Dictionary<string, Rectangle> outputMap)
        {
            outputImage = null;
            outputMap = null;

            List<Texture> files = new List<Texture>();

            // make sure our dictionaries are cleared before starting
            imageSizes.Clear();
            imagePlacement.Clear();

            int NewHeight;
            bool isTrimAtlas = false;
            // int Result = 0;

            // Rect[] TakePlace = new Rect[imageFiles.Count()];
            // TakePlace = new Rectangle[imageFiles.Count()];
            // TakePlace = new TakePlace[imageFiles.Count()];
            TakePlace = new Texture[imageFiles.Count()];
            

            ArrayImage = imageFiles.ToArray();
            try
            {
                TakePlace[0] = ArrayImage[0];
                // outputMap.Add(ArrayImage[0].name, Rectangle.FromLTRB(ArrayImage[0].x, ArrayImage[0].y, ArrayImage[0].width + ArrayImage[0].x, ArrayImage[0].height + ArrayImage[0].y));
                // TakePlace[0].Rect = Rectangle.FromLTRB(ArrayImage[0].x, ArrayImage[0].y, ArrayImage[0].width + ArrayImage[0].x, ArrayImage[0].height + ArrayImage[0].y);
                // TakePlace[0].Name = ArrayImage[0].name;
                // TakePlace[0].AtlasImage = ArrayImage[0].AtlasImage;
                // TakePlace[0] = new Rect(ArrayImage[0].x, ArrayImage[0].y, ArrayImage[0].width + ArrayImage[0].x, ArrayImage[0].height + ArrayImage[0].y);
                len = 1;
                for (I = 1; I < ArrayImage.Count(); I++)
                {
                    J = 0;
                    CurrentY = TextureHeight;
                    FindFreePlace(TextureWidth, TextureHeight);
                    if (ArrayImage[I].height > TextureHeight)
                    {
                        Logging.Manager("ImpossiblePlace");
                        return (int)FailCode.FailedToPackImage;
                    }
                    J = len - 1;
                    while ((J >= 0) && (TakePlace[J].Rect.Bottom > ArrayImage[I].y + ArrayImage[I].height))
                        J--;
                    K = J;
                    while ((K >= 0) && (TakePlace[K].Rect.Bottom == ArrayImage[I].y + ArrayImage[I].height))
                    {
                        if ((ArrayImage[I].x == TakePlace[K].Rect.Right + 1) && (ArrayImage[I].y == TakePlace[K].Rect.Top))
                        {
                            // TakePlace[K].Right = ArrayImage[I].rX;
                            TakePlace[K].Rect.X = ArrayImage[I].x;
                            len--;
                            K = -1;
                            J = -10;
                        }
                        else
                        {
                            K--;
                        }
                    }
                    if (J != -10)
                    {
                        J++;
                        for (K = len; len > J; K--)         //     ?????? ggf. J + 1 ???
                        {
                            TakePlace[K] = TakePlace[K - 1];
                        }
                        TakePlace[J] = ArrayImage[I];
                        // TakePlace[J] = ArrayImage[I].Rct;
                        // TakePlace[J].Rect = Rectangle.FromLTRB(ArrayImage[I].x, ArrayImage[I].y, ArrayImage[I].width + ArrayImage[I].x, ArrayImage[I].height + ArrayImage[I].y);
                        // TakePlace[J].Name = ArrayImage[I].name;
                        // TakePlace[J].AtlasImage = ArrayImage[I].AtlasImage;
                    }
                    len++;
                }
                if (isTrimAtlas)
                {
                    NewHeight = 0;
                    for (I = 0; I < len; I++)
                    {
                        if (NewHeight < TakePlace[I].Rect.Bottom)
                            NewHeight = TakePlace[I].Rect.Bottom;
                    }
                    if (NewHeight > 0)
                    {
                        // https://stackoverflow.com/questions/5383050/how-can-i-calculate-divide-and-modulo-for-integers
                        // if (NewHeight mod 4 == 0)
                        if (NewHeight % 4 == 0)
                            TextureHeight = NewHeight;
                        else
                            // TextureHeight = (NewHeight div 4 + 1) * 4;
                            TextureHeight = (NewHeight / 4 + 1) * 4;
                    }
                }
                // Result = len;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("ImagePackerV2", "PackImage", ex);
            }

            // List<Texture> files = null;
            foreach (var iF in TakePlace)
            {
                imagePlacement.Add(iF, iF.Rect);
                files.Add(iF);
            }
            // List<Texture> files = new List<Texture>(imageFiles);
            

            // make our output image
            // outputImage = CreateOutputImage();
            outputImage = CreateOutputImage.generateImage(files, imagePlacement, TextureWidth, TextureHeight, imagePadding);
            if (outputImage == null)
                return (int)FailCode.FailedToSaveImage;

            Installer.args.ChildProcessed++;
            Installer.InstallWorker.ReportProgress(0);

            if (generateMap)
            {
                // get the sizes of all the images
                int i = 0;
                foreach (var image in ArrayImage)
                {
                    imageSizes.Add(image, image.AtlasImage.Size);
                    i++;
                }
                outputMap = CreateOutputMapData.generateMapData(imagePlacement, imageSizes);
            }

            Installer.InstallWorker.ReportProgress(0);

            // clear our dictionaries just to free up some memory
            imageSizes.Clear();
            imagePlacement.Clear();

            return 0;
        }

        private void FindFreePlace(int TextureWidth, int TextureHeight)
        {
            while (J < len)
            {
                if (TakePlace[J].Rect.IntersectsWith(new Rectangle(ArrayImage[I].x, ArrayImage[I].y, ArrayImage[I].x + ArrayImage[I].width + 1, ArrayImage[I].y + ArrayImage[I].height + 1)))
                {
                    ArrayImage[I].x = TakePlace[J].Rect.Right + 1;
                    if (TakePlace[J].Rect.Bottom > ArrayImage[I].y)
                        CurrentY = Math.Min(CurrentY, TakePlace[J].Rect.Bottom - ArrayImage[I].y + 1);
                    if (ArrayImage[I].x + ArrayImage[I].width > TextureWidth)
                    {
                        ArrayImage[I].y = ArrayImage[I].y + CurrentY;
                        ArrayImage[I].x = 0;
                        CurrentY = TextureHeight;
                    }
                    J = len - 1;
                    while ((J > 0) && (TakePlace[J].Rect.Bottom > ArrayImage[I].y))
                        J++;
                }
                else
                    J++;
            }
        }

        /*
        function RadixSort: Integer;
        var
          I, J, t, CInd: Integer;
          C: array[0.. 9] of Integer;
        begin
          t:= 1;
                SetLength(B, Length(ArrayImage));
          for I:= 1 to 4 do
            begin
              For J:= 0 to 9 do
                C[J]:= 0;
              For J:= 0 to Length(ArrayImage) - 1 do
                begin
                  CInd:= (ArrayImage[J].MaxSize mod(t* 10)) div t;
                C[CInd]:= C[CInd] + 1;
                end;
              For J:= 8 downto 0 do
                C[J]:= C[J + 1] + C[J];
              For J:= Length(ArrayImage) - 1 downto 0 do
                begin
                  CInd:= (ArrayImage[J].MaxSize mod(t* 10)) div t;
                B[C[CInd] - 1]:= ArrayImage[J];
                  C[CInd]:= C[CInd] - 1;
                end;
              t:= t* 10;
              ArrayImage:= Copy(B);
                end;
          SetLength(B, 0);
                Result:= 1;
        end;*/
    }

    public class TakePlace
    {
        public Rectangle Rect;
        // internal Rectangle rect
        private Rectangle rect
        {
            get { return Rect; }
            set { Rect = value; }
        }
        public string Name { get; set; } = "";
        public Bitmap AtlasImage { get; set; } = null;
    }
}
