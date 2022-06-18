using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MyEvBackend.Controllers;

[Route("api/[controller]/[action]")]
public class EvController : ControllerBase
{
   

    private readonly ILogger<EvController> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IConfiguration _configuration;
    private readonly ISmsApi _smsApi;
    public EvController(ILogger<EvController> logger,ICosmosDbService cosmosDbService,IConfiguration configuration,ISmsApi smsapi)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _configuration = configuration;
        _smsApi = smsapi;
    }

    [HttpPost, ActionName("AuthenticateVehicle")]
    public async Task<IActionResult> AuthenticateRequest([FromBody] LoginRequest model)
    {
        try
        {
            var query = "SELECT* FROM c where c.VehicleNumber = '" + model.UniqueVehicleNumber + "'";
            var result = await _cosmosDbService.GetMultipleAsync(query, _configuration.GetSection("ConnectionStrings").GetSection("EvUserContainer").Value);
            if (result.Count == 0 || result is null)
                return NotFound("No vehicle details exist");
            var otp = (string)await _smsApi.SendApi(result.FirstOrDefault().MobileNumber.ToString());
            if(otp?.Length<4||otp?.Length>4)
                return Problem("Unable to send OTP..contact support");
            await UpdateUserRecord(model.UniqueVehicleNumber, result, otp);
            return Ok(otp);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
        
    }

    private async Task UpdateUserRecord(string vehicleNumber, List<dynamic> result, string? otp)
    {
        var itemtobeUpdated = new
        {
            OwnerName = result.FirstOrDefault().OwnerName.ToString(),
            VehicleNumber = result.FirstOrDefault().VehicleNumber.ToString(),
            MakeModel = result.FirstOrDefault().MakeModel.ToString(),
            CurrentRange = result.FirstOrDefault().CurrentRange.ToString(),
            BatteryHealth = result.FirstOrDefault().BatteryHealth.ToString(),
            MobileNumber = result.FirstOrDefault().MobileNumber.ToString(),
            UUID = result.FirstOrDefault().UUID.ToString(),
            TotalOdometer = result.FirstOrDefault().TotalOdometer.ToString(),
            TempOtp = otp,
            id = result.FirstOrDefault().VehicleNumber.ToString()
        };
        result.FirstOrDefault().TempOtp = otp;
        var updateQuery = "SELECT* FROM c where c.VehicleNumber = '" + vehicleNumber + "'";
        await _cosmosDbService.UpdateAsyncv2(result.FirstOrDefault().VehicleNumber.ToString(), itemtobeUpdated,
            _configuration.GetSection("ConnectionStrings").GetSection("EvUserContainer").Value, updateQuery);
    }

    [HttpPost, ActionName("VerifyUser")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyRequest model)
    {
        try
        {
            var query = "SELECT* FROM c where c.VehicleNumber = '" + model.VehicleNumber + "'";
            var result = await _cosmosDbService.GetMultipleAsync(query, _configuration.GetSection("ConnectionStrings").GetSection("EvUserContainer").Value);
            if (result.Count == 0 || result is null)
                return NotFound("No vehicle details exist");
            if (result.FirstOrDefault().TempOtp == model.VerificationOtp)
            {
                await UpdateUserRecord(model.VehicleNumber, result, "");
                return Ok(new UserDetails
                {
                    OwnerName = result.FirstOrDefault().OwnerName,
                    VehicleNumber = result.FirstOrDefault().VehicleNumber,
                    MakeModel = result.FirstOrDefault().MakeModel,
                    CurrentRange = result.FirstOrDefault().CurrentRange,
                    BatteryHealth = result.FirstOrDefault().BatteryHealth,
                    MobileNumber = result.FirstOrDefault().MobileNumber,
                    TotalOdometer = result.FirstOrDefault().TotalOdometer
                
                });
            }
            else
            {
                return Problem("Unable to verify OTP try again in 1 minute");
            }
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
    
    [HttpPost, ActionName("UpdatevehicleCharge")]
    public async Task<IActionResult> UpdateCharge([FromBody] UserVehicleHistory model)
    {
        try
        {
            model.id = Guid.NewGuid().ToString();
            await _cosmosDbService.AddAsync(model.id,model,_configuration.GetSection("ConnectionStrings").GetSection("EvUserHistoryContainer").Value);
            return Ok("Updated Charge successfully");

        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
    
    
    [HttpGet(Name = "GetUserVehicleHistory")]
    public async Task<IActionResult> GetUserVehicleHistory(string vehicleNumber)
    {
        try
        {
            var query = "SELECT* FROM c where c.VehicleNumber = '" + vehicleNumber + "'";
            var result = await _cosmosDbService.GetMultipleAsync(query, _configuration.GetSection("ConnectionStrings").GetSection("EvUserHistoryContainer").Value);
            if (result.Count == 0 || result is null)
                return NotFound("No vehicle details exist");
            var userHistory = new UserVehicleHistory();
            var userHistoryList = new List<UserVehicleHistory>();
            foreach (var item in result)
            {
                userHistory.VehicleNumber = item.VehicleNumber;
                userHistory.LastCharge = item.LastCharge;
                userHistory.LastServiceDate = item.LastServiceDate;
                userHistory.PlaceOfCharge = item.PlaceOfCharge;
                userHistory.PaymentReciept = item.PaymentReciept;
                userHistory.PaymentAmount = item.PaymentAmount;
                userHistory.ChargingStationName = item.ChargingStationName;
                userHistoryList.Add(userHistory);
            }

            return Ok(userHistoryList);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
}