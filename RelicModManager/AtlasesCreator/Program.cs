#region MIT License

/*
 * Copyright (c) 2009-2010 Nick Gravelyn (nick@gravelyn.com), Markus Ewald (cygon@nuclex.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software 
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 * 
 */

#endregion

/// https://github.com/nickgravelyn/SpriteSheetPacker

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace RelhaxModpack.AtlasesCreator
{
    public enum FailCode
    {
        FailedParsingArguments = 1,
        ImageExporter,
        MapExporter,
        NoImages,
        ImageNameCollision,

        FailedToLoadImage,
        FailedToPackImage,
        FailedToCreateImage,
        FailedToSaveImage,
        FailedToSaveMap
    }

    public class Program
    {
        private static readonly Stopwatch stopWatch = new Stopwatch();

        public static void Run(Atlas args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            int result = 0;
            try
            {
                result = Launch(args);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("AtlasesCreator.Run", "Launch", ex);
            }
            sw.Stop();
            
            if (result != 0)
                ShowBuildError("Error packing images: " + SpaceErrorCode((FailCode)result));
            else
                Logging.Installer("Build for " + Path.GetFileName(args.AtlasFile) + " completed in " + sw.Elapsed.TotalSeconds.ToString("N3", System.Globalization.CultureInfo.InvariantCulture) + " seconds.");
            return;
        }

        private static int Launch(Atlas args)
        {
            if (args.AtlasFile.Equals(""))
            {
                return (int)FailCode.FailedParsingArguments;
            }
            else
            {
                // make sure we found some images
                if (args.TextureList.Count == 0)
                {
                    Logging.Manager("No images to pack for " + args.AtlasFile);
                    return (int)FailCode.NoImages;
                }

                // generate our output
                ImagePacker imagePacker = new ImagePacker();

                // pack the image, generating a map only if desired
                int result = imagePacker.PackImage(args.TextureList, args.PowOf2, args.Square, args.FastImagePacker, args.AtlasWidth, args.AtlasHeight, args.Padding, args.mapExporter != null, out Bitmap outputImage, out Dictionary<string, Rectangle> outputMap);
                if (result != 0)
                {
                    Logging.Manager("There was an error making the image sheet.");
                    //error result 7 = "failed to pack image" most likely it won't fit
                    return result;
                }
                else
                {
                    Logging.Manager(string.Format("Packing '{0}' to {1} x {2} pixel", Path.GetFileName(args.AtlasFile), outputImage.Height, outputImage.Width));
                }

                // try to save using our exporters
                try
                {
                    if (File.Exists(args.AtlasFile))
                        File.Delete(args.AtlasFile);
                    args.imageHandler.Save(args.AtlasFile, outputImage);
                    Logging.InstallerGroup("created Atlases");                                              // write comment
                    Logging.Installer(Utils.ReplaceDirectorySeparatorChar(args.AtlasFile));                 // write created filename with path
                }
                catch (Exception e)
                {
                    Logging.Manager("Error saving file: " + e.Message);
                    return (int)FailCode.FailedToSaveImage;
                }

                if (args.mapExporter != null)
                {
                    try
                    {
                        if (File.Exists(args.MapFile))
                            File.Delete(args.MapFile);
                        args.mapExporter.Save(args.MapFile, outputMap);
                        Logging.Installer(Utils.ReplaceDirectorySeparatorChar(args.MapFile));                 // write created filename with path
                    }
                    catch (Exception e)
                    {
                        Logging.Manager("Error saving file: " + e.Message);
                        return (int)FailCode.FailedToSaveMap;
                    }
                }
            }
            return 0;
        }

        private static string SpaceErrorCode(FailCode failCode)
        {
            string error = failCode.ToString();

            string result = error[0].ToString();

            for (int i = 1; i < error.Length; i++)
            {
                char c = error[i];
                if (char.IsUpper(c))
                    result += " ";
                result += c;
            }

            return result;
        }

        private static void ShowBuildError(string error)
        {
            Logging.Manager("AtlasesCreator: " + error);
        }
    }
}