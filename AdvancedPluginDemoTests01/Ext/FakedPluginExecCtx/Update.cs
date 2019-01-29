// ReSharper disable RedundantNameQualifier
namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  extern alias AdvPlg;
  using FakeXrmEasy;

  using Microsoft.Xrm.Sdk;

  public class UpdateBase : WithTargetEntityBase
  {
    public UpdateBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.MessageName = AdvPlg::AdvancedPlugin.Constants.MessageName.Update;
    }
  }

  public class UpdatePreValidationBase : UpdateBase
  {
    public UpdatePreValidationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreValidate;
    }
  }

  public class UpdatePreValidationSyncBase : UpdatePreValidationBase
  {
    public UpdatePreValidationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class UpdatePreValidationAsyncBase : UpdatePreValidationBase
  {
    public UpdatePreValidationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class UpdatePreOperationBase : UpdateBase
  {
    public UpdatePreOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreOperation;
    }
  }

  public class UpdatePreOperationSyncBase : UpdatePreOperationBase
  {
    public UpdatePreOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class UpdatePreOperationAsyncBase : UpdatePreOperationBase
  {
    public UpdatePreOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class UpdatePostOperationBase : UpdateBase
  {
    public UpdatePostOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PostOperation;
    }
  }

  public class UpdatePostOperationSyncBase : UpdatePostOperationBase
  {
    public UpdatePostOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class UpdatePostOperationAsyncBase : UpdatePostOperationBase
  {
    public UpdatePostOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }
}
