using System;

namespace pbt.ChangeLogger.Dto;

public class LogEntryDto
{
    public string Actor { get; set; }
    public String Timestamp { get; set; }
    public string Description { get; set; }
}