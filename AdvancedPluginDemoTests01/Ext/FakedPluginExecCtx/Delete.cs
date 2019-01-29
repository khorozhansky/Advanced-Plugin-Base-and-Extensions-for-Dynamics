// ReSharper disable RedundantNameQualifier
namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  extern alias AdvPlg;
  using FakeXrmEasy;

  public class DeleteBase : WithTargetEntityReferenceBase
  {
    public DeleteBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.MessageName = AdvPlg::AdvancedPlugin.Constants.MessageName.Delete;
    }
  }

  public class DeletePreValidationBase : DeleteBase
  {
    public DeletePreValidationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreValidate;
    }
  }

  public class DeletePreValidationSyncBase : DeletePreValidationBase
  {
    public DeletePreValidationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class DeletePreValidationAsyncBase : DeletePreValidationBase
  {
    public DeletePreValidationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class DeletePreOperationBase : DeleteBase
  {
    public DeletePreOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreOperation;
    }
  }

  public class DeletePreOperationSyncBase : DeletePreOperationBase
  {
    public DeletePreOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class DeletePreOperationAsyncBase : DeletePreOperationBase
  {
    public DeletePreOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class DeletePostOperationBase : DeleteBase
  {
    public DeletePostOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PostOperation;
    }
  }

  public class DeletePostOperationSyncBase : DeletePostOperationBase
  {
    public DeletePostOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class DeletePostOperationAsyncBase : DeletePostOperationBase
  {
    public DeletePostOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }
}
