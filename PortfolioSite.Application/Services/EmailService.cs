using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortfolioSite.Application.Services;

public class EmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _pass;
    private readonly string _from;

    public EmailService()
    {
        _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        _port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        _user = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
        _pass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";
        _from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? "";
    }

    public async Task SendConfirmationEmailAsync(string toEmail, string toName)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("İpek Nur Öztürk", _from));
        email.To.Add(new MailboxAddress(toName, toEmail));
        email.Subject = "Mesajınız Alındı — ipek.dev";

        var builder = new BodyBuilder();
        builder.HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: 'Inter', Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
    .container {{ max-width: 560px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); }}
    .header {{ background: #080b14; padding: 32px; text-align: center; }}
    .header h1 {{ color: #06b6d4; font-size: 24px; margin: 0; font-family: monospace; }}
    .header p {{ color: rgba(255,255,255,0.5); font-size: 13px; margin: 8px 0 0; font-family: monospace; }}
    .body {{ padding: 32px; }}
    .body h2 {{ color: #1a1a2e; font-size: 20px; margin-bottom: 12px; }}
    .body p {{ color: #555; font-size: 15px; line-height: 1.7; margin-bottom: 16px; }}
    .highlight {{ background: #f0fafa; border-left: 3px solid #06b6d4; padding: 12px 16px; border-radius: 0 8px 8px 0; color: #0891b2; font-size: 14px; margin-bottom: 24px; }}
    .footer {{ background: #f9f9f9; padding: 20px 32px; text-align: center; border-top: 1px solid #eee; }}
    .footer p {{ color: #999; font-size: 12px; margin: 0; }}
    .footer a {{ color: #06b6d4; text-decoration: none; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>ipek.dev</h1>
      <p>// mesajınız alındı</p>
    </div>
    <div class='body'>
      <h2>Merhaba {toName}, 👋</h2>
      <p>Mesajınız başarıyla alındı. En kısa sürede size geri dönüş yapacağım.</p>
      <div class='highlight'>
        💬 Genellikle 1-2 iş günü içinde yanıt veriyorum.
      </div>
      <p>Bu süreçte projelerimi inceleyebilir veya LinkedIn profilimi ziyaret edebilirsiniz.</p>
      <p>Teşekkürler,<br><strong>İpek Nur Öztürk</strong><br>.NET Backend Developer</p>
    </div>
    <div class='footer'>
      <p>
        <a href='https://ipek.dev'>ipek.dev</a> · 
        <a href='https://github.com/ipeknroztrk'>GitHub</a> · 
        <a href='https://linkedin.com/in/ipek-nur-ozturk'>LinkedIn</a>
      </p>
    </div>
  </div>
</body>
</html>";

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_user, _pass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}