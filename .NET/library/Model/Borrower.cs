namespace OneBeyondApi.Model
{
    public class Borrower(string name, string emailAddress) : Entity
    {
        public string Name { get; set; } = name;
        public string EmailAddress { get; set; } = emailAddress;
        public double FinesOwed { get; set; }
    }
}
