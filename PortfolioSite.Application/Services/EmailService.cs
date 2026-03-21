

namespace PortfolioSite.Application.Services;

public class EmailService
{
    private readonly string _apiKey;
    private readonly string _from;

    public EmailService()
    {
        _apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        _from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? "onboarding@resend.dev";
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string toName)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var emailData = new
        {
            from = $"İpek Nur Öztürk <{_from}>",
            to = new[] { toEmail },
            subject = "Mesajınız Alındı — ipekozturk.com",
            html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; background: #f5f5f5; padding: 20px;'>
  <div style='max-width: 560px; margin: 0 auto; background: #fff; border-radius: 16px; overflow: hidden;'>
    <div style='background: #080b14; padding: 32px; text-align: center;'>
      <h1 style='color: #06b6d4; margin: 0; font-family: monospace;'>ipekozturk.com</h1>
      <p style='color: rgba(255,255,255,0.5); font-size: 13px; margin: 8px 0 0; font-family: monospace;'>// mesajınız alındı</p>
    </div>
    <div style='padding: 32px;'>
      <h2 style='color: #1a1a2e;'>Merhaba {toName}, 👋</h2>
      <p style='color: #555; line-height: 1.7;'>Mesajınız başarıyla alındı. En kısa sürede size geri dönüş yapacağım.</p>
      <div style='background: #f0fafa; border-left: 3px solid #06b6d4; padding: 12px 16px; border-radius: 0 8px 8px 0; color: #0891b2; margin-bottom: 24px;'>
        💬 Genellikle 1-2 iş günü içinde yanıt veriyorum.
      </div>
      <p style='color: #555;'>Teşekkürler,<br><strong>İpek Nur Öztürk</strong><br>.NET Backend Developer</p>
    </div>
    <div style='background: #f9f9f9; padding: 20px 32px; text-align: center; border-top: 1px solid #eee;'>
      <p style='color: #999; font-size: 12px; margin: 0;'>
        <a href='https://ipekozturk.com' style='color: #06b6d4;'>ipekozturk.com</a> · 
        <a href='https://github.com/ipeknroztrk' style='color: #06b6d4;'>GitHub</a> · 
        <a href='https://linkedin.com/in/ipek-nur-ozturk' style='color: #06b6d4;'>LinkedIn</a>
      </p>
    </div>
  </div>
</body>
</html>"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(emailData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync("https://api.resend.com/emails", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"Resend response: {response.StatusCode} - {responseBody}");
        
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Email gönderilemedi: {responseBody}");
    }
}