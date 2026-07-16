using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace DbMonitorData
{
  /// <summary>
  /// Zentraler Datenzugriff auf Oracle-Systemviews für das DB-Monitor-Tool.
  /// Alle Methoden sind fehlergesichert, NULL-sicher und geben stets eine leere Liste zurück.
  /// </summary>
  public class DbQuery
  {
    #region Felder

    private readonly OracleConnectionProvider _connectionProvider;
    private readonly ILogger<DbQuery> _logger;

    #endregion

    #region Konstruktor

    public DbQuery(OracleConnectionProvider connectionProvider, ILogger<DbQuery> logger)
    {
      _connectionProvider = connectionProvider;
      _logger = logger;
    }

    #endregion

    #region Öffentliche Methoden – Verbindung

    /// <summary>
    /// Wechselt die aktive Datenbankverbindung anhand eines Connection-Strings.
    /// </summary>
    public void SetDatabase(string connectionString)
    {
      if (string.IsNullOrEmpty(connectionString))
      {
        _logger.LogError("SetDatabase: Connection-String ist leer.");
        return;
      }

      try
      {
        _connectionProvider.SetConnectionString(connectionString);
        _logger.LogInformation("SetDatabase: Verbindung hergestellt.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "SetDatabase: Fehler beim Verbindungsaufbau.");
        throw;
      }
    }

    #endregion

    #region Öffentliche Methoden – V$SESSION

    public List<V_SESSION_MONITOR> GetAktuelleSessionsList()
    {
      const string sql = @"
          SELECT s.sid,
                 s.serial#    AS serial,
                 NVL(s.username, 'N/A') AS username,
                 s.osuser,
                 s.machine,
                 s.terminal,
                 s.program,
                 s.type,
                 s.status,
                 s.logon_time,
                 NVL(p.spid, '0') AS process_id
          FROM   v$session s
          LEFT JOIN v$process p ON s.paddr = p.addr
          ORDER BY s.status ASC, s.sid ASC";

      return AusfuehrenUndLesen(sql, leser => new V_SESSION_MONITOR
      {
        SID = LeseDecimalNull(leser, "SID"),
        SERIAL = LeseDecimalNull(leser, "SERIAL"),
        USERNAME = leser["USERNAME"]?.ToString(),
        OSUSER = leser["OSUSER"]?.ToString(),
        MACHINE = leser["MACHINE"]?.ToString(),
        TERMINAL = leser["TERMINAL"]?.ToString(),
        PROGRAM = leser["PROGRAM"]?.ToString(),
        TYPE = leser["TYPE"]?.ToString(),
        STATUS = leser["STATUS"]?.ToString(),
        LOGON_TIME = LeseDateTimeNull(leser, "LOGON_TIME"),
        PROCESS_ID = leser["PROCESS_ID"]?.ToString()
      }, nameof(GetAktuelleSessionsList));
    }
    public SqlTextErgebnis GetCurrentSqlText(decimal sid, decimal serial)
    {
      if (!VerbindungPruefen(nameof(GetCurrentSqlText)))
        return SqlTextErgebnis.Leer("Hinweis: Keine Datenbankverbindung.");

      const string sqlText = @"
          SELECT q.sql_fulltext AS sql_text, q.sql_id
          FROM   v$session s
          JOIN   v$sql    q ON q.hash_value = s.sql_hash_value
                            AND q.address   = s.sql_address
          WHERE  s.sid     = :p_sid
          AND    s.serial# = :p_serial
          AND    ROWNUM    = 1";

      const string bindVarsSql = @"
          SELECT b.name            AS bind_name,
                 b.value_string    AS bind_wert,
                 b.datatype_string AS bind_typ
          FROM   v$session          s
          JOIN   v$sql_bind_capture b ON b.sql_id = s.sql_id
          WHERE  s.sid     = :p_sid
          AND    s.serial# = :p_serial
          ORDER BY b.position ASC";

      try
      {
        string rohSql = string.Empty;
        string sqlId = string.Empty;
        string bindVars = string.Empty;

        using (var cmd = ErstelleBefehl(sqlText))
        {
          cmd.Parameters.Add("p_sid", OracleDbType.Decimal).Value = sid;
          cmd.Parameters.Add("p_serial", OracleDbType.Decimal).Value = serial;

          using (var leser = cmd.ExecuteReader())
          {
            if (leser.Read() && leser["SQL_TEXT"] != DBNull.Value)
            {
              rohSql = leser["SQL_TEXT"].ToString();
              sqlId = leser["SQL_ID"]?.ToString() ?? string.Empty;
            }
          }
        }

        if (string.IsNullOrEmpty(rohSql))
          return SqlTextErgebnis.Leer("Hinweis: Kein aktives SQL gefunden.");

        if (!string.IsNullOrEmpty(sqlId))
        {
          using (var cmd = ErstelleBefehl(bindVarsSql))
          {
            cmd.Parameters.Add("p_sid", OracleDbType.Decimal).Value = sid;
            cmd.Parameters.Add("p_serial", OracleDbType.Decimal).Value = serial;

            using (var leser = cmd.ExecuteReader())
            {
              var sb = new StringBuilder();
              while (leser.Read())
                sb.AppendLine($"{leser["BIND_NAME"]} = {leser["BIND_WERT"]} [{leser["BIND_TYP"]}]");
              bindVars = sb.ToString().Trim();
            }
          }
        }

        return new SqlTextErgebnis(rohSql, string.Empty, bindVars);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Fehler in GetCurrentSqlText SID={Sid}.", sid);
        return SqlTextErgebnis.Leer($"Fehler: {ex.Message}");
      }
    }

    public void KillSession(decimal sid, decimal serial, string grund, string sqlText,
                        string programm, string osBenutzer, string terminal, string action = "KILL")
    {
      if (!VerbindungPruefen(nameof(KillSession))) return;

      try
      {
        using (var cmd = ErstelleBefehl("BEGIN kill_user_session(:sid, :serial); END;"))
        {
          cmd.Parameters.Add("sid", OracleDbType.Decimal).Value = sid;
          cmd.Parameters.Add("serial", OracleDbType.Decimal).Value = serial;
          try { cmd.ExecuteNonQuery(); }
          catch (OracleException ex) { if (ex.Number != 31) throw; }
        }

        const string auditSql = @"
        INSERT INTO dbmonitor_audit_log
          (id, kill_date, sid, serial_nr, reason, killed_by, sql_text, program, os_user, terminal, action)
        VALUES
          ((SELECT NVL(MAX(id), 0) + 1 FROM dbmonitor_audit_log),
           SYSDATE, :sid, :serial, :reason, USER, :sqlText,
           :program, :osUser, :terminal, :action)";

        using (var cmd = ErstelleBefehl(auditSql))
        {
          cmd.Parameters.Add("sid", OracleDbType.Decimal).Value = sid;
          cmd.Parameters.Add("serial", OracleDbType.Decimal).Value = serial;
          cmd.Parameters.Add("reason", OracleDbType.Varchar2).Value = grund ?? "Kein Grund angegeben";
          cmd.Parameters.Add("sqlText", OracleDbType.Clob).Value = string.IsNullOrEmpty(sqlText) ? (object)DBNull.Value : sqlText;
          cmd.Parameters.Add("program", OracleDbType.Varchar2).Value = (object)programm ?? DBNull.Value;
          cmd.Parameters.Add("osUser", OracleDbType.Varchar2).Value = (object)osBenutzer ?? DBNull.Value;
          cmd.Parameters.Add("terminal", OracleDbType.Varchar2).Value = (object)terminal ?? DBNull.Value;
          cmd.Parameters.Add("action", OracleDbType.Varchar2).Value = action;

          cmd.ExecuteNonQuery();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "KillSession: Fehler bei SID={Sid}.", sid);
        throw;
      }
    }

    #endregion

    #region Öffentliche Methoden – V$SQL

    public List<V_SQL> GetAllSqlList()
    {
      const string sql = @"
          SELECT sql_id, sql_fulltext, executions, disk_reads, buffer_gets, cpu_time
          FROM (
              SELECT sql_id, sql_fulltext, executions, disk_reads, buffer_gets, cpu_time
              FROM   v$sql ORDER BY buffer_gets DESC
          ) WHERE ROWNUM <= 100";

      return AusfuehrenUndLesen(sql, LeseSqlZeile, nameof(GetAllSqlList));
    }

    public List<V_SQL> GetTopExecutedSqlList()
    {
      const string sql = @"
          SELECT sql_id, sql_fulltext, executions, disk_reads, buffer_gets, cpu_time
          FROM (
              SELECT sql_id, sql_fulltext, executions, disk_reads, buffer_gets, cpu_time
              FROM   v$sql ORDER BY executions DESC
          ) WHERE ROWNUM <= 20";

      return AusfuehrenUndLesen(sql, LeseSqlZeile, nameof(GetTopExecutedSqlList));
    }

    #endregion

    #region Öffentliche Methoden – V$PROCESS

    public List<V_PROCESS> GetAllProcessesList()
    {
      const string sql = @"
          SELECT pid, spid, username, program, RAWTOHEX(addr) AS addr,
                 background, pga_used_mem, pga_alloc_mem,
                 ROUND(pga_used_mem / 1024 / 1024, 2) AS pga_used_mb
          FROM   v$process ORDER BY pga_used_mem DESC";

      return AusfuehrenUndLesen(sql, LeseProzessZeile, nameof(GetAllProcessesList));
    }

    public List<V_PROCESS> GetTopMemoryProcessesList()
    {
      const string sql = @"
          SELECT pid, spid, username, program, RAWTOHEX(addr) AS addr,
                 background, pga_used_mem, pga_alloc_mem,
                 ROUND(pga_used_mem / 1024 / 1024, 2) AS pga_used_mb
          FROM (
              SELECT pid, spid, username, program, addr,
                     background, pga_used_mem, pga_alloc_mem
              FROM   v$process ORDER BY pga_used_mem DESC
          ) WHERE ROWNUM <= 20";

      return AusfuehrenUndLesen(sql, LeseProzessZeile, nameof(GetTopMemoryProcessesList));
    }

    #endregion

    #region Öffentliche Methoden – V$LOCK

    public List<V_LOCK> GetLockList()
    {
      const string sql = @"
        SELECT 
            s.sid AS SID, 
            s.serial# AS SERIAL,
            s.username AS USERNAME, 
            s.osuser AS OSUSER, 
            s.machine AS MACHINE, 
            s.terminal AS TERMINAL,
            s.program AS PROGRAM, 
            s.status AS STATUS, 
            s.logon_time AS LOGON_TIME, 
            s.type AS SESSION_TYPE,
            l.id1 AS ID1, 
            l.type AS TYPE,
            DECODE(l.lmode, 0,'Waiting',1,'Null',2,'Row Shr',3,'Row Exc',
                4,'Share',5,'Shr Row X',6,'Exclusive',TO_CHAR(l.lmode)) AS STATUS_AKTUELL,
            DECODE(l.request, 0,'None',1,'Null',2,'Row Shr',3,'Row Exc',
                4,'Share',5,'Shr Row X',6,'Exclusive',TO_CHAR(l.request)) AS ANGEFRAGT
        FROM   v$session s 
        JOIN   v$lock l ON s.sid = l.sid
        WHERE  l.block > 0 OR l.request > 0";

      return AusfuehrenUndLesen(sql, leser => new V_LOCK
      {
        SID = LeseDecimal(leser, "SID"),
        SERIAL = LeseDecimal(leser, "SERIAL"),
        USERNAME = leser["USERNAME"]?.ToString(),
        OSUSER = leser["OSUSER"]?.ToString(),
        MACHINE = leser["MACHINE"]?.ToString(),
        TERMINAL = leser["TERMINAL"]?.ToString(),
        PROGRAM = leser["PROGRAM"]?.ToString(),
        STATUS = leser["STATUS"]?.ToString(),
        LOGON_TIME = LeseDateTimeNull(leser, "LOGON_TIME"),
        SESSION_TYPE = leser["SESSION_TYPE"]?.ToString(),
        ID1 = leser["ID1"]?.ToString(),
        TYPE = leser["TYPE"]?.ToString(),
        STATUS_AKTUELL = leser["STATUS_AKTUELL"]?.ToString(),
        ANGEFRAGT = leser["ANGEFRAGT"]?.ToString()
      }, nameof(GetLockList));
    }

    #endregion

    #region Öffentliche Methoden – DBA_SCHEDULER_JOBS

    public List<V_JOB> GetAlleJobList()
    {
      const string sql = @"
          SELECT j.job_name, j.job_action, j.state, j.start_date,
                 j.last_start_date, j.next_run_date, j.repeat_interval,
                 j.run_count, j.failure_count, l.status
          FROM dba_scheduler_jobs j
          LEFT JOIN (
              SELECT * FROM (
                  SELECT l.*, ROW_NUMBER() OVER (
                      PARTITION BY l.job_name ORDER BY l.log_date DESC) AS rn
                  FROM dba_scheduler_job_log l)
              WHERE rn = 1) l ON j.job_name = l.job_name
          WHERE j.state != 'DISABLED'
          ORDER BY j.state ASC, j.job_name ASC";

      return AusfuehrenUndLesen(sql, LeseJobZeile, nameof(GetAlleJobList));
    }

    public List<V_JOB> GetAuffaelligeJobList()
    {
      const string sql = @"
          SELECT j.job_name, j.job_action, j.state, j.start_date,
                 j.last_start_date, j.next_run_date, j.repeat_interval,
                 j.run_count, j.failure_count, l.status
          FROM dba_scheduler_jobs j
          LEFT JOIN (
              SELECT * FROM (
                  SELECT l.*, ROW_NUMBER() OVER (
                      PARTITION BY l.job_name ORDER BY l.log_date DESC) AS rn
                  FROM dba_scheduler_job_log l)
              WHERE rn = 1) l ON j.job_name = l.job_name
          WHERE j.state != 'DISABLED'
            AND (j.state != 'SCHEDULED'
                 OR (l.status != 'SUCCEEDED' AND l.operation = 'RUN'))
          ORDER BY j.state ASC, j.job_name ASC";

      return AusfuehrenUndLesen(sql, LeseJobZeile, nameof(GetAuffaelligeJobList));
    }

    public List<V_JOB_RUN_DETAILS> GetJobLog(string jobName)
    {
      if (string.IsNullOrWhiteSpace(jobName)) return new List<V_JOB_RUN_DETAILS>();
      if (!VerbindungPruefen(nameof(GetJobLog))) return new List<V_JOB_RUN_DETAILS>();

      const string sql = @"
          SELECT log_date, job_name, status, req_start_date, run_duration
          FROM   dba_scheduler_job_run_details
          WHERE  job_name = :p_jobName
          ORDER BY log_id DESC";

      var liste = new List<V_JOB_RUN_DETAILS>();
      try
      {
        using (var cmd = ErstelleBefehl(sql))
        {
          cmd.Parameters.Add(new OracleParameter("p_jobName", jobName));
          using (var leser = cmd.ExecuteReader())
          {
            while (leser.Read())
            {
              liste.Add(new V_JOB_RUN_DETAILS
              {
                LOG_DATE = LeseDateTimeOffsetNull(leser, "LOG_DATE"),
                JOB_NAME = leser["JOB_NAME"]?.ToString(),
                STATUS = leser["STATUS"]?.ToString(),
                REQ_START_DATE = LeseDateTimeOffsetNull(leser, "REQ_START_DATE"),
                RUN_DURATION = leser["RUN_DURATION"] != DBNull.Value
                                     ? leser["RUN_DURATION"].ToString() : "00:00:00"
              });
            }
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Fehler in GetJobLog für '{JobName}'.", jobName);
      }
      return liste;
    }

    #endregion

    #region Öffentliche Methoden – Audit-Log

    public List<AuditLogEntry> GetAuditLogEntries()
    {
      const string sql = @"
          SELECT * FROM dbmonitor_audit_log ORDER BY kill_date DESC";

      return AusfuehrenUndLesen(sql, leser => new AuditLogEntry
      {
        DAL_ID = LeseDecimal(leser, "ID"),
        INTER_CODE = leser["ACTION"]?.ToString(),
        KILL_DATE = Convert.ToDateTime(leser["KILL_DATE"]),
        DAL_REASON = leser["REASON"]?.ToString(),
        DAL_SID = LeseDecimal(leser, "SID"),
        DAL_SERIAL = LeseDecimal(leser, "SERIAL_NR"),
        DAL_USERNAME = leser["KILLED_BY"]?.ToString(),
        DAL_OSUSER = leser["OS_USER"]?.ToString(),
        DAL_TERMINAL = leser["TERMINAL"]?.ToString(),
        DAL_PROGRAM = leser["PROGRAM"]?.ToString(),
        DAL_LOGON_TIME = LeseDateTimeNull(leser, "KILL_DATE"),
        DAL_SQL_TEXT = leser["SQL_TEXT"]?.ToString()
      }, nameof(GetAuditLogEntries));
    }

    #endregion

    #region Private Hilfsmethoden

    private List<T> AusfuehrenUndLesen<T>(
        string sql, Func<OracleDataReader, T> mapper, string methodenName)
    {
      var liste = new List<T>();
      if (!VerbindungPruefen(methodenName)) return liste;

      try
      {
        using (var cmd = ErstelleBefehl(sql))
        using (var leser = cmd.ExecuteReader())
        {
          while (leser.Read()) liste.Add(mapper(leser));
        }
        _logger.LogDebug("{Methode}: {Anzahl} Einträge geladen.", methodenName, liste.Count);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Fehler in {Methode}.", methodenName);
      }
      return liste;
    }

    private OracleCommand ErstelleBefehl(string sql) =>
        new OracleCommand(sql, _connectionProvider.Connection) { BindByName = true };

    private bool VerbindungPruefen(string methodenName)
    {
      if (_connectionProvider?.IsConnected == true) return true;
      _logger.LogWarning("{Methode}: Datenbankverbindung ist nicht aktiv.", methodenName);
      return false;
    }

    private static V_JOB LeseJobZeile(OracleDataReader leser) => new V_JOB
    {
      JOB_NAME = leser["JOB_NAME"]?.ToString(),
      JOB_ACTION = leser["JOB_ACTION"]?.ToString(),
      STATE = leser["STATE"]?.ToString(),
      START_DATE = LeseDateTimeOffsetNull(leser, "START_DATE"),
      LAST_START_DATE = LeseDateTimeOffsetNull(leser, "LAST_START_DATE"),
      NEXT_RUN_DATE = LeseDateTimeOffsetNull(leser, "NEXT_RUN_DATE"),
      REPEAT_INTERVAL = leser["REPEAT_INTERVAL"]?.ToString(),
      RUN_COUNT = LeseDecimal(leser, "RUN_COUNT"),
      FAILURE_COUNT = LeseDecimal(leser, "FAILURE_COUNT"),
      STATUS = leser["STATUS"]?.ToString()
    };

    private static V_SQL LeseSqlZeile(OracleDataReader leser) => new V_SQL
    {
      SQL_ID = leser["SQL_ID"]?.ToString(),
      SQL_FULLTEXT = leser["SQL_FULLTEXT"]?.ToString() ?? string.Empty,
      EXECUTIONS = LeseDecimal(leser, "EXECUTIONS"),
      DISK_READS = LeseDecimal(leser, "DISK_READS"),
      BUFFER_GETS = LeseDecimal(leser, "BUFFER_GETS"),
      CPU_TIME = LeseDecimal(leser, "CPU_TIME")
    };

    private static V_PROCESS LeseProzessZeile(OracleDataReader leser) => new V_PROCESS
    {
      PID = LeseDecimalNull(leser, "PID"),
      SPID = LeseDecimalNull(leser, "SPID"),
      USERNAME = leser["USERNAME"]?.ToString(),
      PROGRAM = leser["PROGRAM"]?.ToString(),
      ADDR = leser["ADDR"]?.ToString(),
      BACKGROUND = leser["BACKGROUND"]?.ToString(),
      PGA_USED_MEM = LeseDecimal(leser, "PGA_USED_MEM"),
      PGA_ALLOC_MEM = LeseDecimal(leser, "PGA_ALLOC_MEM"),
      PGA_USED_MB = LeseDecimal(leser, "PGA_USED_MB")
    };

    private static DateTime? LeseDateTimeOffsetNull(OracleDataReader leser, string spalte)
    {
      if (leser[spalte] == DBNull.Value) return null;
      if (leser[spalte] is DateTimeOffset dto) return dto.LocalDateTime;
      return Convert.ToDateTime(leser[spalte]);
    }

    private static decimal LeseDecimal(OracleDataReader leser, string spalte) =>
        leser[spalte] != DBNull.Value ? Convert.ToDecimal(leser[spalte]) : 0m;

    private static decimal? LeseDecimalNull(OracleDataReader leser, string spalte) =>
        leser[spalte] != DBNull.Value ? Convert.ToDecimal(leser[spalte]) : (decimal?)null;

    private static DateTime? LeseDateTimeNull(OracleDataReader leser, string spalte) =>
        leser[spalte] != DBNull.Value ? (DateTime?)Convert.ToDateTime(leser[spalte]) : null;

    #endregion
  }
}
