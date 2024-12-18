using System.ComponentModel.DataAnnotations;

public class BookingItem
{
    [Key]
    public long BookingId { get; set; }
    public int NoOfPeople { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
    public List<string> NamesOfPeople { get; set; } = [];
    public long StayId { get; set; }
    public string Comment { get; set; } = string.Empty;
}