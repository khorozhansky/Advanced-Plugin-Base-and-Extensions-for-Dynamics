namespace AdvancedPluginDemoUnitTests
{
  using System.Collections;
  using System.Collections.Generic;

  public abstract class TheoryData : IEnumerable<object[]>
  {
    private readonly List<object[]> data = new List<object[]>();

    protected void AddRow(params object[] values)
    {
      this.data.Add(values);
    }

    public IEnumerator<object[]> GetEnumerator()
    {
      return this.data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
  }

  public class TheoryData<T1> : TheoryData
  {
    public void Add(T1 p1)
    {
      this.AddRow(p1);
    }
  }

  public class TheoryData<T1, T2> : TheoryData
  {
    public void Add(T1 p1, T2 p2)
    {
      this.AddRow(p1, p2);
    }
  }

  public class TheoryData<T1, T2, T3> : TheoryData
  {
    public void Add(T1 p1, T2 p2, T3 p3)
    {
      this.AddRow(p1, p2, p3);
    }
  }

  public class TheoryData<T1, T2, T3, T4> : TheoryData
  {
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4)
    {
      this.AddRow(p1, p2, p3, p4);
    }
  }

  public class TheoryData<T1, T2, T3, T4, T5> : TheoryData
  {
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
    {
      this.AddRow(p1, p2, p3, p4, p5);
    }
  }

  public class TheoryData<T1, T2, T3, T4, T5, T6> : TheoryData
  {
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
      this.AddRow(p1, p2, p3, p4, p5, p6);
    }
  }

  public class TheoryData<T1, T2, T3, T4, T5, T6, T7> : TheoryData
  {
    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
    {
      this.AddRow(p1, p2, p3, p4, p5, p6, p7);
    }
  }
}
