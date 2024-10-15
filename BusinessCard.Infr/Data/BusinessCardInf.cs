namespace BusinessCard_Rahaf.Modals
{
    public class BusinessCardInf
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PhotoBase64 { get; set; }  // Optional
    }
}
