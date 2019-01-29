// ReSharper disable RedundantNameQualifier
namespace AdvancedPluginDemoUnitTests.Ext.FakedPluginExecCtx
{
  extern alias AdvPlg;
  using FakeXrmEasy;

  public class CreateBase : WithTargetEntityBase
  {
    public CreateBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.MessageName = AdvPlg::AdvancedPlugin.Constants.MessageName.Create;
    }
  }

  public class CreatePreValidationBase : CreateBase
  {
    public CreatePreValidationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreValidate;
    }
  }

  public class CreatePreValidationSyncBase : CreatePreValidationBase
  {
    public CreatePreValidationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class CreatePreValidationAsyncBase : CreatePreValidationBase
  {
    public CreatePreValidationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class CreatePreOperationBase : CreateBase
  {
    public CreatePreOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PreOperation;
    }
  }

  public class CreatePreOperationSyncBase : CreatePreOperationBase
  {
    public CreatePreOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class CreatePreOperationAsyncBase : CreatePreOperationBase
  {
    public CreatePreOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }

  public class CreatePostOperationBase : CreateBase
  {
    public CreatePostOperationBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Stage = (int)AdvPlg::AdvancedPlugin.Constants.Stage.PostOperation;
    }
  }

  public class CreatePostOperationSyncBase : CreatePostOperationBase
  {
    public CreatePostOperationSyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Synchronous;
    }
  }

  public class CreatePostOperationAsyncBase : CreatePostOperationBase
  {
    public CreatePostOperationAsyncBase(XrmFakedContext xrmFakedContext) : base(xrmFakedContext)
    {
      this.Mode = (int)AdvPlg::AdvancedPlugin.Constants.Mode.Asynchronous;
    }
  }
}
