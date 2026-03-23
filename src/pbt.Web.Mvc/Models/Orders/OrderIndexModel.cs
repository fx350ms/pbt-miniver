namespace pbt.Web.Models.Orders
{
    public class OrderIndexModel
    {
        public int Total { get; set; }
        public int MyOrderTotal { get; set; }
        public int CustomerOrderTotal { get; set; }

        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }    
        public int TotalCompleted { get; set; }
        public int TotalCancel {  get; set; }
    }
}
