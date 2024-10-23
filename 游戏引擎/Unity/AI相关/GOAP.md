# GOAP
目标导向的行为规划
目标、状态、行为、计划

比如战斗为主的游戏，AI的目标就是杀死玩家
会有一个行为表，然后基于这个行为表去行动，行为之间可能有约束
比如AI需要用枪杀死玩家，但是枪需要子弹，所以就需要去寻找子弹

## GOAP的状态
如何描述一件事物往往是最重要的部分
面向对象编程最重要的就是面向接口编程
而在后续实现中，主要就是操纵状态类中的Value,这个Value便是状态类的值
这样封装是为了之后制作比较器能够自定义一个比较的方法
```cs
/// <summary>
/// GOAP状态类接口
/// 所有的状态类都继承自这个接口
/// </summary>
public interface IGOAPState
{
    public bool EqualsValue(IGOAPState other);
    public void SetValue(IGOAPState other);
    public IGOAPState Copy();
}

/// <summary>
/// GOAP状态类泛型抽象类
/// 增加泛型约束
/// </summary>
/// <typeparam name="T">GOAP状态类</typeparam>
/// <typeparam name="V">状态类的值</typeparam>
public abstract class AGOAPState<T, V> : IGOAPState where T : AGOAPState<T, V>, new()
{
    [SerializeField] protected V value;
    public V Value { get => value; protected set => this.value = value; }

    /// <summary>
    /// 本状态类的Value与传入的是否相等
    /// 供子类重写逻辑
    /// </summary>
    protected abstract bool EqualsValue(T other);

    /// <summary>
    /// 将本状态类的Value直接设置为传入的值
    /// 供子类重写逻辑
    /// </summary>
    protected virtual void SetValue(V value)
    {
        this.value = value;
    }

    /// <summary>
    /// 将本状态类的Value设置为传入的状态类的值
    /// 供子类重写逻辑
    /// </summary>
    protected virtual void SetValue(T other)
    {
        this.value = other.Value;
    }

    /// <summary>
    /// 本状态类的Value与传入的是否相等
    /// 供外界调用
    /// </summary>
    public bool EqualsValue(IGOAPState other)
    {
        return EqualsValue((T)other);
    }

    /// <summary>
    /// 将本状态类的Value直接设置为传入的值
    /// 供外界调用
    /// </summary>
    public void SetValue(IGOAPState other)
    {
        SetValue((T)other);
    }

    /// <summary>
    /// 复制本状态类并返回
    /// 供外界调用
    /// </summary>
    public IGOAPState Copy()
    {
        return new T() { value = this.value };
    }
}
```

## 状态的比较器
1. 作为行为的前提
2. 作为目标的倾向
3. 