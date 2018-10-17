namespace RelhaxModpack
{
    public enum MediaType
    {
        Unknown = 0,
        Picture = 1,
        Webpage = 2,
        MediaFile = 3,
        HTML = 4
    }
    public class Media
    {
        //SAMPLE YOUTUBE URL: https://www.youtube.com/v/ZwY2E0hjGuU?version=3&autoplay=1
        //http and https both work
        public string URL = "";
        //media type
        public MediaType MediaType = MediaType.Picture;
        public Media() {  }

        public override string ToString()
        {
            if(URL.Length > 79)
                return "Type: " + (int)MediaType + " - " + URL.Substring(0, 80) + "...";
            else
                return "Type: " + (int)MediaType + " - " + URL;
        }
    }
}
