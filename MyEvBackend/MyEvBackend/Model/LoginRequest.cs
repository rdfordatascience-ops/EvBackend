namespace MyEvBackend.Controllers;

public class LoginRequest
{
    public string UniqueVehicleNumber { get; set; }
}

public class VerifyRequest
{
    public string VerificationOtp { get; set; }
    public string VehicleNumber { get; set; }
}

public class UserDetails
{
    public string OwnerName { get; set; }
    public string VehicleNumber { get; set; }
    public string MakeModel { get; set; }
    public string CurrentRange { get; set; }
    public string MobileNumber { get; set; }
    public string BatteryHealth { get; set; }
    
}