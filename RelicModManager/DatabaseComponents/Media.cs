namespace RelhaxModpack
{
    public enum MediaType
    {
        Picture = 1,
        Webpage = 2,
        MediaFile = 3,
        HTML = 4
    }
    public class Media
    {
        //SAMPLE YOUTUBE URL: https://www.youtube.com/v/ZwY2E0hjGuU?version=3&autoplay=1
        //http and https both work
        public string URL { get; set; }
        //media type, 1 = picture (default), 2 = youtube video
        public MediaType MediaType { get; set; }
        //constructor to setup the picture with the "name" and
        //the URL where the picture is located
        public Media()
        {
            //default media type to picture
            MediaType = MediaType.Picture;
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
