namespace OneBeyondApi.Model
{
    public class Reservation : Entity
    {
        public Reservation()
        {

        }

        public Reservation(Guid bookId, Guid borrowerId, DateTime reservedDate)
        {
            BookId = bookId;
            BorrowerId = borrowerId;
            ReservedDate = reservedDate;
        }

        public Guid BookId { get; set; }
        public Guid BorrowerId { get; set; }
        public Book Book { get; set; }
        public Borrower ReservedBy { get; set; }
        public DateTime ReservedDate { get; set; }
        //public int QueuePosition { get; set; }
    }
}
