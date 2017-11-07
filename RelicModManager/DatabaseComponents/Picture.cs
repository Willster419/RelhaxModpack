
namespace RelhaxModpack
{
    public enum MediaType
    {
        Picture = 1,
        Youtube = 2
    }
    public class Media
    {
        //two-part variable. specifies wether it's part of a mod, or a mod's config
        //and the mod/config name
        public string Name { get; set; }
        //SAMPLE YOUTUBE URL: https://www.youtube.com/v/ZwY2E0hjGuU?version=3&autoplay=1
        //http and https both work
        public string URL { get; set; }
        //media type, 1 = picture (default), 2 = youtube video
        public MediaType MediaType { get; set; }
        //constructor to setup the picture with the "name" and
        //the URL where the picture is located
        public Media(string newName, string newURL)
        {
            Name = newName;
            URL = newURL;
            MediaType = MediaType.Picture;
        }
        //sorts the mods
        public static int ComparePictures(Media x, Media y)
        {
            //name looks like this
            //Mod:name
            //Config:name
            //seperate all 4 things
            string xType = x.Name.Split(':')[0];
            string yType = y.Name.Split(':')[0];
            string xName = x.Name.Split(':')[1];
            string yName = y.Name.Split(':')[1];
            //check the mod vs config first
            int typeResult = xType.CompareTo(yType);
            if (typeResult == 0)
            {
                //eithor both mod or both config, sort alphabetically
                return xName.CompareTo(yName);
            }
            //mod first, then config
            return typeResult * -1;
        }

        public override string ToString()
        {
            if(URL.Length > 79)
                return "Type: " + (int)MediaType + " - " + URL.Substring(0, 80) + "...";
            else
                return "Type: " + (int)MediaType + " - " + URL;
        }
    }
}
