namespace AdvancedPlugin.Extensions
{
  public interface IFieldExt
  {
    /// <summary>
    /// Gets whether the new value is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is specified; otherwise, <c>false</c>.
    /// </value>
    bool IsSpecified { get; }

    /// <summary>
    /// Gets a value indicating whether [the value is modified].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [the value is modified]; otherwise, <c>false</c>.
    /// </value>
    bool IsModified { get; }

    /// <summary>
    /// Gets a value indicating whether [new value gets set to null].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [new value gets set to null]; otherwise, <c>false</c>.
    /// </value>
    bool IsSetToNull { get; }

    /// <summary>
    /// Gets a value indicating whether the value will be null after the action.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [it will be null]; otherwise, <c>false</c>.
    /// </value>
    bool IsNull { get; }
  }
}