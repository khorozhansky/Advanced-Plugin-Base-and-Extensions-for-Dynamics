namespace AdvancedPlugin.Logging
{
  public class LogSettings
  {
    /// <summary>
    /// Gets or sets a value indicating whether Input and Output Parameters should be saved.
    /// </summary>
    public bool SaveInputOutputParameters { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Shared Variables should be saved.
    /// </summary>
    public bool SaveSharedVariables { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Pre-Images and Post-Images should be saved.
    /// </summary>
    public bool SavePrePostImages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Stack Trace should be saved.
    /// </summary>
    public bool SaveStackTrace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Trace Text should be saved.
    /// </summary>
    public bool SaveTraceText { get; set; }
  }
}
