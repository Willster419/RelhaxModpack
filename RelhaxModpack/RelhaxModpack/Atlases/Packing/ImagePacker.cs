using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Atlases.Packing
{
    /// <summary>
    /// Handles packing a list of small bitmap objects into one large bitmap object 
    /// </summary>
    public class ImagePacker
    {
        // various properties of the resulting image
        private bool requirePow2, requireSquare, acceptFirstPass;
        private int padding;
        private int outputWidth, outputHeight;

        // the input list of image files
        private List<Texture> files;

        // some dictionaries to hold the image sizes and destination rectangles
        private readonly Dictionary<Texture, Size> imageSizes = new Dictionary<Texture, Size>();
        private readonly Dictionary<Texture, Rectangle> imagePlacement = new Dictionary<Texture, Rectangle>();

        //for diagnostics timing
        private readonly Stopwatch stopwatch = new Stopwatch();
        private long imagePackingMilliseconds = 0;

        /// <summary>
        /// Packs a collection of images into a single image.
        /// </summary>
        /// <param name="imageFiles">The list of Textures of the images to be combined.</param>
        /// <param name="requirePowerOfTwo">Whether or not the output image must have a power of two size.</param>
        /// <param name="requireSquareImage">Whether or not the output image must be a square.</param>
        /// <param name="fastImagePacker">Accept the first successful image packing.</param>
        /// <param name="maximumWidth">The maximum width of the output image.</param>
        /// <param name="maximumHeight">The maximum height of the output image.</param>
        /// <param name="imagePadding">The amount of blank space to insert in between individual images.</param>
        /// <param name="atlasImageName">The name of the Atlas image. Used for logging and can be null.</param>
        /// <param name="outputImage">The resulting output image.</param>
        /// <param name="outputMap">The resulting output map of placement rectangles for the images.</param>
        /// <returns>0 if the packing was successful, error code otherwise.</returns>
        public FailCode PackImage(
            IEnumerable<Texture> imageFiles,
            bool requirePowerOfTwo,
            bool requireSquareImage,
            bool fastImagePacker,
            int maximumWidth,
            int maximumHeight,
            int imagePadding,
            string atlasImageName,
            out Bitmap outputImage,
            out Dictionary<string, Rectangle> outputMap)
        {
            files = new List<Texture>(imageFiles);
            requirePow2 = requirePowerOfTwo == true;
            requireSquare = requireSquareImage == true;
            acceptFirstPass = fastImagePacker;
            outputWidth = maximumWidth;
            outputHeight = maximumHeight;
            padding = imagePadding;

            outputImage = null;
            outputMap = null;

            imagePackingMilliseconds = 0;

            // make sure our dictionaries are cleared before starting
            imageSizes.Clear();
            imagePlacement.Clear();

            stopwatch.Restart();
            LogStatus("Preparing for packing", atlasImageName, false);
            // get the sizes of all the images
            int i = 0;
            foreach (var image in files)
            {
                imageSizes.Add(image, image.AtlasImage.Size);
                i++;
            }

            // sort our files by file size so we place large sprites first
            files.Sort(
                (f1, f2) =>
                {
                    Size b1 = imageSizes[f1];
                    Size b2 = imageSizes[f2];

                    //check surface size first
                    int c = -(b1.Width * b1.Height).CompareTo(b2.Width * b2.Height);
                    if (c != 0)
                        return c;

                    //check sizes first
                    c = -b1.Width.CompareTo(b2.Width);
                    if (c != 0)
                        return c;

                    c = -b1.Height.CompareTo(b2.Height);
                    if (c != 0)
                        return c;

                    //same size? go alphabetical i guess
                    return f1.Name.CompareTo(f2.Name);
                });
            LogStatus(string.Format("Preparing completed in {0} msec", stopwatch.ElapsedMilliseconds), atlasImageName, true);

            LogStatus("Packing images into atlas", atlasImageName, false);
            if (!PackImageRectangles())
            {
                return FailCode.FailedToPackImage;
            }
            LogStatus(string.Format("Packing images completed in {0} msec", stopwatch.ElapsedMilliseconds), atlasImageName, true);

            LogStatus("Generating atlas bitmap", atlasImageName, false);
            outputImage = GenerateAtlasImageData(files, imagePlacement, outputWidth, outputHeight);
            if (outputImage == null)
                return FailCode.FailedToCreateBitmapAtlas;
            LogStatus(string.Format("Generating atlas bitmap completed in {0} msec", stopwatch.ElapsedMilliseconds), atlasImageName, true);

            LogStatus("Generating atlas map data", atlasImageName, false);
            outputMap = GenerateMapData(imagePlacement, imageSizes);
            LogStatus(string.Format("Generating atlas map data completed in {0} msec", stopwatch.ElapsedMilliseconds), atlasImageName, true);

            // clear our dictionaries just to free up some memory
            imageSizes.Clear();
            imagePlacement.Clear();

            return FailCode.None;
        }

        private Dictionary<string, Rectangle> GenerateMapData(Dictionary<Texture, Rectangle> imagePlacement, Dictionary<Texture, Size> imageSizes)
        {
            // go through our image placements and replace the width/height found in there with
            // each image's actual width/height (since the ones in imagePlacement will have padding)
            Texture[] keys = new Texture[imagePlacement.Keys.Count];
            imagePlacement.Keys.CopyTo(keys, 0);

            foreach (var k in keys)
            {
                // get the actual size
                Size s = imageSizes[k];

                // get the placement rectangle
                Rectangle r = imagePlacement[k];

                // set the proper size
                r.Width = s.Width;
                r.Height = s.Height;

                // insert back into the dictionary
                imagePlacement[k] = r;
            }

            // copy the placement dictionary to the output
            Dictionary<string, Rectangle> outputMap = new Dictionary<string, Rectangle>();
            foreach (var pair in imagePlacement)
            {
                outputMap.Add(pair.Key.Name, pair.Value);
            }
            return outputMap;
        }

        private Bitmap GenerateAtlasImageData(List<Texture> files, Dictionary<Texture, Rectangle> imagePlacement, int outputWidth, int outputHeight)
        {
            Bitmap atlas = new Bitmap(outputWidth, outputHeight, PixelFormat.Format32bppArgb);

            //lock the bits and copy the atlas to bytes
            BitmapData atlasData = atlas.LockBits(new Rectangle(0, 0, atlas.Width, atlas.Height), ImageLockMode.WriteOnly, atlas.PixelFormat);
            int atlasByteDepth = Math.Abs(atlasData.Stride) * atlas.Height;
            byte[] atlasByte = new byte[atlasByteDepth];
            Marshal.Copy(atlasData.Scan0, atlasByte, 0, atlasByteDepth);

            //loop through each texture to copy it's data over
            foreach (Texture texture in files)
            {
                lock (texture.AtlasImage)
                {
                    CopyTextureIntoAtlasLock(ref atlasByte, texture.AtlasImage, imagePlacement[texture], atlasData.Stride);
                }
            }

            //copy back and unlock
            Marshal.Copy(atlasByte, 0, atlasData.Scan0, atlasByteDepth);
            atlas.UnlockBits(atlasData);

            //for debugging, verify the image is bit for bit accurate
            //https://online-image-comparison.com/
            //http://onlinemd5.com/
            //atlas.Save(@"C:\Users\Willster419\Desktop\custom2.png",ImageFormat.Png);

            return atlas;
        }

        private void CopyTextureIntoAtlasLock(ref byte[] atlasByte, Bitmap texture, Rectangle locationOnAtlas, int atlasStride)
        {
            //define the area on the atlas that we actually want to copy over based on the texture (but don't copy padding)
            Rectangle actualLocationToCopyOntoAtlas = new Rectangle(locationOnAtlas.X, locationOnAtlas.Y, texture.Width, texture.Height);

            //lock the texture and get bitmap lock data
            BitmapData textureData = texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height), ImageLockMode.ReadOnly, texture.PixelFormat);

            /*
             * each of the 4 image channels (alpha, red, green, blue) is a byte, which is included into one pixel.
             * the row of pixels is represented as the width, and if each pixel has 4 bytes,
             * then the 'true' width is (width * 4), also called the stride.
             * 
             * we want to copy the texture's image data to a byte array, so get the stride * height to ensure it all fits.
            */

            int textureByteDepth = Math.Abs(textureData.Stride) * actualLocationToCopyOntoAtlas.Height;
            byte[] textureByte = new byte[textureByteDepth];
            //           source           , destination, start index, length
            Marshal.Copy(textureData.Scan0, textureByte, 0          , textureByteDepth);

            /*
             * as stated above, 4 places in the array = 1 pixel.
             * so to get pixel 3 (1 based), it would be byte indexes 8 to 11
             * the formula here is base ((pixel-1) * 4) to ((pixel-1) * 4) + 4
             * 
             * the amount that you want to copy for each row is width * 4
             * and will loop for texture height
             * 
             * to get the starting point of where to replace the atlas data
             * in the byte array,  (stride * (Y + row_loop)) + (X * 4)
             * where:
             * Y = starting row of the top-left of the texture that we want to replace
             * X = into the row of the atlas image of the starting point of the top-left
             *     of the texture we want to replace
            */

            //use a counter for indexing into texture's byte array when copying data to the atlas byte array
            int tempTextureCount = 0;
            for (int row = 0; row < actualLocationToCopyOntoAtlas.Height; row++)
            {
                //get the starting point for indexing into the atlas data's byte array using formula above
                int atlasStartingPoint = ((actualLocationToCopyOntoAtlas.Y + row) * atlasStride) + (actualLocationToCopyOntoAtlas.X * 4);

                //we're copying an whole row, which is texture width * 4 aka stride
                int ammountToCopy = textureData.Stride;

                //now that we have the starting point on the atlas byte array,
                //and we know how much to copy, copy the bytes over
                //using the tempTextureCount is for tracking each stride
                //we copy from the texture
                for (int j = 0; j < ammountToCopy; j++)
                {
                    atlasByte[atlasStartingPoint + j] = textureByte[tempTextureCount++];
                }
            }

            //unlock and we're done
            texture.UnlockBits(textureData);
        }

        // This method does some trickery type stuff where we perform the TestPackingImages method over and over, 
        // trying to reduce the image size until we have found the smallest possible image we can fit.
        private bool PackImageRectangles()
        {
            bool quit = false;

            // create a dictionary for our test image placements
            Dictionary<Texture, Rectangle> testImagePlacement = new Dictionary<Texture, Rectangle>();

            // get the size of our smallest image
            int smallestWidth = int.MaxValue;
            int smallestHeight = int.MaxValue;
            foreach (var size in imageSizes)
            {
                smallestWidth = Math.Min(smallestWidth, size.Value.Width);
                smallestHeight = Math.Min(smallestHeight, size.Value.Height);
            }

            // values to control loop
            int whileLoops = 0;
            int ready = int.MaxValue;

            // we need a couple values for testing
            int testWidth = outputWidth;
            int testHeight = outputHeight;

            bool shrinkVertical = false;

            try
            {
                // used for detection of the optimization phase
                // just keep looping...
                while (whileLoops < ready)
                {
                    whileLoops++;
                    // make sure our test dictionary is empty
                    testImagePlacement.Clear();

                    // try to pack the images into our current test size
                    if (!TestPackingImages(testWidth, testHeight, testImagePlacement))
                    {
                        // if that failed...

                        // if we have no images in imagePlacement, i.e. we've never succeeded at PackImages,
                        // show an error and return false since there is no way to fit the images into our
                        // maximum size texture
                        if (testImagePlacement.Count == 0)
                        {
                            return false;
                        }

                        // otherwise return true to use our last good results
                        if (shrinkVertical)
                            return true;

                        shrinkVertical = true;
                        testWidth += smallestWidth + padding + padding;
                        testHeight += smallestHeight + padding + padding;
                        continue;
                    }
                    else
                    {
                        // is the packing result true: is fastImagePacker enabled?
                        if (acceptFirstPass)
                            quit = true;
                    }

                    // clear the imagePlacement dictionary and add our test results in
                    imagePlacement.Clear();
                    foreach (var pair in testImagePlacement)
                        imagePlacement.Add(pair.Key, pair.Value);

                    // figure out the smallest bitmap that will hold all the images
                    testWidth = testHeight = 0;
                    foreach (var pair in imagePlacement)
                    {
                        testWidth = Math.Max(testWidth, pair.Value.Right);
                        testHeight = Math.Max(testHeight, pair.Value.Bottom);
                    }

                    // subtract the extra padding on the right and bottom
                    if (!shrinkVertical)
                        testWidth -= padding;
                    testHeight -= padding;

                    // if we require a power of two texture, find the next power of two that can fit this image
                    if (requirePow2)
                    {
                        testWidth = FindNextPowerOfTwo(testWidth + padding + padding);
                        testHeight = FindNextPowerOfTwo(testHeight + padding + padding);
                    }

                    // if we require a square texture, set the width and height to the larger of the two
                    if (requireSquare)
                    {
                        int max = Math.Max(testWidth, testHeight);
                        if (requirePow2)
                            testWidth = testHeight = max;
                        else
                            testWidth = testHeight = (max + padding + padding);
                    }

                    // if the test results are the same as our last output results, we've reached an optimal size,
                    // so we can just be done
                    if (testWidth == outputWidth && testHeight == outputHeight)
                    {
                        if (shrinkVertical)
                            return true;

                        shrinkVertical = true;
                    }

                    // save the test results as our last known good results
                    outputWidth = testWidth;
                    outputHeight = testHeight;

                    // option for faster finishing
                    if (quit)
                    {
                        if (!(requireSquare || requirePow2))
                        {
                            outputWidth += padding;
                            outputHeight += padding + padding;
                        }
                        return true;
                    }

                    // subtract the smallest image size out for the next test iteration
                    if (!shrinkVertical)
                        testWidth -= smallestWidth;
                    testHeight -= smallestHeight;
                }
                return false;
            }
            finally
            {
                Logging.Info("{0} loop(s) done for packing", whileLoops);
            }
        }

        private bool TestPackingImages(int testWidth, int testHeight, Dictionary<Texture, Rectangle> testImagePlacement)
        {
            // create the rectangle packer
            ArevaloRectanglePacker rectanglePacker = new ArevaloRectanglePacker(testWidth, testHeight);

            foreach (var image in files)
            {
                // get the bitmap for this file
                Size size = imageSizes[image];

                // pack the image
                if (!rectanglePacker.TryPack(size.Width + padding, size.Height + padding, out Point origin))
                {
                    return false;
                }

                // add the destination rectangle to our dictionary
                testImagePlacement.Add(image, new Rectangle(origin.X, origin.Y, size.Width + padding, size.Height + padding));
            }
            return true;
        }

        // stolen from http://en.wikipedia.org/wiki/Power_of_two#Algorithm_to_find_the_next-highest_power_of_two
        private int FindNextPowerOfTwo(int k)
        {
            k--;
            for (int i = 1; i < sizeof(int) * 8; i <<= 1)
                k = k | k >> i;
            return k + 1;
        }

        private void LogStatus(string message, string atlasFilename, bool logTime)
        {
            Logging.Debug(LogOptions.ClassName, "{0}{1}", string.IsNullOrEmpty(atlasFilename) ? string.Empty : string.Format("[atlas file {0}]: ", atlasFilename), message);
            if (logTime)
            {
                imagePackingMilliseconds += stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
            }
        }
    }
}
