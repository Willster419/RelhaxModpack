
namespace RelhaxModpack
{
    //This class is for saving all the lines in an .xc xvm config file
    //the "best json api" can't handle "$" refrences, so they must be removed
    //prior to patching. This class stores all required information for that purpose.
    public class StringSave
    {
        //the name of the property to put it back on later
        public string name { get; set; }
        //the value of the property (the refrence)
        public string value { get; set; }
        public StringSave()
        {

        }
    }
}
