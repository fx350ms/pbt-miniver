using System;

public class TrackingInfoModel
    {
        public string WaybillCode { get; set; }
        public int Status { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }