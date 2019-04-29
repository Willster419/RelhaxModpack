using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.AtlasesCreator.Packing
{
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
        /// <param name="outputImage">The resulting output image.</param>
        /// <param name="outputMap">The resulting output map of placement rectangles for the images.</param>
        /// <returns>0 if the packing was successful, error code otherwise.</returns>
        public FailCode PackImage(
            IEnumerable<Texture> imageFiles,
            Atlas.State requirePowerOfTwo,
            Atlas.State requireSquareImage,
            bool fastImagePacker,
            int maximumWidth,
            int maximumHeight,
            int imagePadding,
            out Bitmap outputImage,
            out Dictionary<string, Rectangle> outputMap)
        {
            files = new List<Texture>(imageFiles);
            requirePow2 = requirePowerOfTwo == Atlas.State.True;
            requireSquare = requireSquareImage == Atlas.State.True;
            acceptFirstPass = fastImagePacker;
            outputWidth = maximumWidth;
            outputHeight = maximumHeight;
            padding = imagePadding;

            outputImage = null;
            outputMap = null;

            // make sure our dictionaries are cleared before starting
            imageSizes.Clear();
            imagePlacement.Clear();

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
                    return f1.name.CompareTo(f2.name);
                });

            // try to pack the images
            if (!PackImageRectangles())
            {
                return FailCode.FailedToPackImage;
            }

            // make our output image
            outputImage = ImageHandler.GenerateImage(files, imagePlacement, outputWidth, outputHeight);
            if (outputImage == null)
                return FailCode.FailedToSaveImage;

            
            outputMap = MapExporter.GenerateMapData(imagePlacement, imageSizes);


            // clear our dictionaries just to free up some memory
            imageSizes.Clear();
            imagePlacement.Clear();

            return 0;
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
                Point origin;
                if (!rectanglePacker.TryPack(size.Width + padding, size.Height + padding, out origin))
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
    }
}
