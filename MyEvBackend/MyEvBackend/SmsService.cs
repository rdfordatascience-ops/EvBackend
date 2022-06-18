using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MyEvBackend;

public class SmsService:ISmsApi
{
    public SmsService(IConfiguration configuration)
    {
        TwilioClient.Init(configuration.GetSection("ConnectionStrings").GetSection("TwilioAccoundSid").Value,configuration.GetSection("ConnectionStrings").GetSection("TwilioSecret").Value);
    }
    public async Task<string> SendApi(string phoneNumber)
    {
        var otptobeGenerated = await GenerateRandomOTP();
        // var message = MessageResource.Create(
        //     body: otptobeGenerated.ToString(),
        //     from: new Twilio.Types.PhoneNumber("+17163338803"),
        //     to: new Twilio.Types.PhoneNumber(phoneNumber)
        // );
        //
        //
        // return message.ErrorCode!=null?message.ErrorMessage:otptobeGenerated;
        return otptobeGenerated;
    }
    
    private async Task<string> GenerateRandomOTP()  
  
    {
        string[] saAllowedCharacters  =  { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        string sOTP = String.Empty;  
  
        string sTempChars = String.Empty;  
  
        Random rand = new Random();  
  
        for (int i = 0; i < 4; i++)  
  
        {  
  
            int p = rand.Next(0, saAllowedCharacters.Length);  
  
            sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];  
  
            sOTP += sTempChars;  
  
        }  
  
        return sOTP;  
  
    }  
}