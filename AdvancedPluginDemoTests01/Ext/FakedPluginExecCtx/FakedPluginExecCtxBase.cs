namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  using System;

  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk;

  public abstract class FakedPluginExecCtxBase : XrmFakedPluginExecutionContext
  {
    public const string CustomVerboseLogSwitchedOnKeyword = "CustomVerboseLogSwitchedOn";

    protected FakedPluginExecCtxBase(XrmFakedContext xrmFakedContext)
    {
      this.Depth = 1;
      this.IsExecutingOffline = false;
      this.MessageName = "Create";
      this.InputParameters = new ParameterCollection();
      this.OutputParameters = new ParameterCollection();
      this.SharedVariables = new ParameterCollection();
      this.PreEntityImages = new EntityImageCollection();
      this.PostEntityImages = new EntityImageCollection();
      this.UserId = xrmFakedContext.CallerId?.Id ?? Guid.NewGuid();
      this.InitiatingUserId = xrmFakedContext.CallerId?.Id ?? Guid.NewGuid();
      this.BusinessUnitId = xrmFakedContext.BusinessUnitId?.Id ?? Guid.NewGuid();
    }

    public void SetPreImageEntity(Entity value)
    {
      this.PreEntityImages["PreImage"] = value;
    }

    public void SetPreImageEntity(string name, Entity value)
    {
      this.PreEntityImages[name] = value;
    }

    public void SetPostImageEntity(Entity value)
    {
      this.PostEntityImages["PostImage"] = value;
    }

    public void SetPostImageEntity(string name, Entity value)
    {
      this.PostEntityImages[name] = value;
    }

    public void SetSharedVariable(string name, object value)
    {
      this.SetParameter(this.SharedVariables, name, value);
    }

    public TK GetInputParameter<TK>(string key)
    {
      var parameters = this.InputParameters;
      return this.GetParameter<TK>(parameters, key);
    }

    public object GetInputParameter(string key)
    {
      var parameters = this.InputParameters;
      return this.GetParameter(parameters, key);
    }

    public TK GetOutputParameter<TK>(string key)
    {
      var parameters = this.OutputParameters;
      return this.GetParameter<TK>(parameters, key);
    }

    public object GetOutputParameter(string key)
    {
      var parameters = this.OutputParameters;
      return this.GetParameter(parameters, key);
    }

    public TK GetParameter<TK>(ParameterCollection parameters, string key)
    {
      if (!parameters.ContainsKey(key))
      {
        return default(TK);
      }

      return (TK)parameters[key];
    }

    public object GetParameter(ParameterCollection parameters, string key)
    {
      return !parameters.ContainsKey(key) ? null : parameters[key];
    }

    public void SetParameter(ParameterCollection parameters, string key, object value)
    {
      if (parameters == null)
      {
        parameters = new ParameterCollection();
      }

      parameters[key] = value;
    }

    public void SetInputParameter(string key, object value)
    {
      this.SetParameter(this.InputParameters, key, value);
    }

    public void SetOutputParameter(string key, object value)
    {
      this.SetParameter(this.OutputParameters, key, value);
    }

  }
}
