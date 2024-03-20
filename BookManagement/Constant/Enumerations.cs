namespace BookManagement.Constant
{
    public class Enumerations
    {
        public enum ToastType: int
        {
            None = 0, 
            Success = 1, 
            Error = 2,
            Warning = 3,
        }

        public enum OrderStatus : int
        {
            Waiting = 1,
            Shipping = 2,
            Complete = 3,
            Cancel = 4
        }
    }
}
