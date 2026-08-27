namespace AISAM.Services.Exceptions;

public class DevelopmentModeException : Exception
{
    public DevelopmentModeException() 
        : base("Meta app đang ở chế độ Development. Vui lòng chuyển sang Live mode tại https://developers.facebook.com/apps/ rồi deploy lại.")
    {
    }
}
