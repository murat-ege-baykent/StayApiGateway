using System.ComponentModel.DataAnnotations;
public class StayItem
{
    [Key]
    public long StayId { get; set; }
    public int MaxNoOfPeople { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; } = 0.0; // Average rating
}
