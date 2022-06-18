using Newtonsoft.Json;

namespace MyEvBackend.Controllers;

public class UserVehicleHistory
{
    public string LastCharge { get; set; }
    public string VehicleNumber { get; set; }
    public string LastServiceDate { get; set; }
    public string PlaceOfCharge { get; set; }
    public string ChargingStationName { get; set; }
    public string PaymentReciept { get; set; }
    public string PaymentAmount { get; set; }
    public string id { get; set; }
}