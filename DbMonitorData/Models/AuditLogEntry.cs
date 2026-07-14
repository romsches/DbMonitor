using System;

namespace DbMonitorWPF.Models
{
  public class AuditLogEntry
  {
    public decimal DAL_ID { get; set; }
    public string DAL_REASON { get; set; }
    public decimal DAL_SID { get; set; }
    public decimal DAL_SERIAL { get; set; }
    public string DAL_USERNAME { get; set; }
    public DateTime KILL_DATE { get; set; }
    public string DAL_TERMINAL { get; set; }
    public string DAL_PROGRAM { get; set; }
  }
}
