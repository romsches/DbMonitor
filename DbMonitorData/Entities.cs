using System;

namespace DbMonitorData
{
  public class V_SESSION_MONITOR
  {
    public decimal? SID { get; set; }
    public decimal? SERIAL { get; set; }
    public string USERNAME { get; set; }
    public string OSUSER { get; set; }
    public string MACHINE { get; set; }
    public string TERMINAL { get; set; }
    public string PROGRAM { get; set; }
    public string TYPE { get; set; }
    public string STATUS { get; set; }
    public DateTime? LOGON_TIME { get; set; }
    public string PROCESS_ID { get; set; }
  }

  public class V_SQL
  {
    public string SQL_ID { get; set; }
    public string SQL_FULLTEXT { get; set; }
    public decimal EXECUTIONS { get; set; }
    public decimal DISK_READS { get; set; }
    public decimal BUFFER_GETS { get; set; }
    public decimal CPU_TIME { get; set; }
  }

  public class V_PROCESS
  {
    public decimal? PID { get; set; }
    public decimal? SPID { get; set; }
    public string USERNAME { get; set; }
    public string PROGRAM { get; set; }
    public string ADDR { get; set; }
    public string BACKGROUND { get; set; }
    public decimal PGA_USED_MEM { get; set; }
    public decimal PGA_ALLOC_MEM { get; set; }
    public decimal PGA_USED_MB { get; set; }
  }

  public class V_LOCK
  {
    public decimal SID { get; set; }
    public decimal SERIAL { get; set; }
    public string USERNAME { get; set; }
    public string OSUSER { get; set; }
    public string MACHINE { get; set; }
    public string TERMINAL { get; set; }
    public string PROGRAM { get; set; }
    public string STATUS { get; set; }
    public DateTime? LOGON_TIME { get; set; }
    public string SESSION_TYPE { get; set; }
    public string ID1 { get; set; }
    public string TYPE { get; set; }
    public string STATUS_AKTUELL { get; set; }
    public string ANGEFRAGT { get; set; }
  }

  public class V_JOB
  {
    public string JOB_NAME { get; set; }
    public string JOB_TYPE { get; set; }
    public string JOB_ACTION { get; set; }
    public DateTime? START_DATE { get; set; }
    public string REPEAT_INTERVAL { get; set; }
    public string ENABLED { get; set; }
    public string STATE { get; set; }
    public decimal RUN_COUNT { get; set; }
    public decimal FAILURE_COUNT { get; set; }
    public string STATUS { get; set; }
    public DateTime? LAST_START_DATE { get; set; }
    public string LAST_RUN_DURATION { get; set; }
    public DateTime? NEXT_RUN_DATE { get; set; }
    public string COMMENTS { get; set; }
  }

  public class V_JOB_RUN_DETAILS
  {
    public DateTime? LOG_DATE { get; set; }
    public string JOB_NAME { get; set; }
    public string STATUS { get; set; }
    public DateTime? REQ_START_DATE { get; set; }
    public string RUN_DURATION { get; set; }
  }

  public class AuditLogEntry
  {
    public decimal DAL_ID { get; set; }
    public string INTER_CODE { get; set; }
    public DateTime KILL_DATE { get; set; }
    public string DAL_REASON { get; set; }
    public decimal DAL_SID { get; set; }
    public decimal DAL_SERIAL { get; set; }
    public string DAL_USERNAME { get; set; }
    public string DAL_OSUSER { get; set; }
    public string DAL_TERMINAL { get; set; }
    public string DAL_PROGRAM { get; set; }
    public DateTime? DAL_LOGON_TIME { get; set; }
    public string DAL_SQL_TEXT { get; set; }
  }
}
