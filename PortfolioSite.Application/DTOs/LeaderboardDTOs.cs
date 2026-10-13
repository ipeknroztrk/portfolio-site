namespace PortfolioSite.Application.DTOs;

// Skor gönderimi için sadece izin verilen alanlar (Id, CreatedAt gibi alanlar istemciden alınmaz)
public class SubmitScoreRequestDto
{
    public string? SessionId { get; set; }
    public string? PlayerName { get; set; }
    public int Points { get; set; }
}
