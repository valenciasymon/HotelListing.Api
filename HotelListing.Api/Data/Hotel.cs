namespace HotelListing.Api.Data;

public class Hotel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public double Rating { get; set; }



    public int? CountryId { get; set; }

    public Country? Country { get; set; }

    public Hotel(int id , string name ,string address,double rating)
    {
        Id = id;
        Name = name;
        Address = address;
        Rating = rating;
    }
}
