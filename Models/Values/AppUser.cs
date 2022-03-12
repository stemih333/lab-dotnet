namespace Models.Values
{
    public class AppUser
    {
        public string? Name { get; set; }
        public string Email { get; set; }
        public bool IsBookingReader { get; set; }
        public bool IsBookingWriter { get; set; }
        public bool StatusChanger { get; set; }
    }
}
