namespace RelhaxModpack
{
    public interface UIComponent
    {
        Category catagory { get; set; }
        SelectablePackage mod { get; set; }
        SelectablePackage config { get; set; }
    }
}
