namespace RelhaxModpack
{
    public interface UIComponent
    {
        Category catagory { get; set; }
        Mod mod { get; set; }
        Config config { get; set; }
    }
}
