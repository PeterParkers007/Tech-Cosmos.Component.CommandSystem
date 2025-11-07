# TechCosmos Command System

---

一个灵活、可扩展的命令系统，用于在 Unity 中实现命令模式，支持命令队列管理和泛型命令目标。

## 功能特性

**🚀 完整生命周期管理** - 支持命令状态跟踪（Pending、Executing、Completed、Failed、Cancelled）

**🎯 泛型支持** - 支持任意类型的命令目标

**📋 智能命令队列** - 基于优先级的命令排序和执行

**⚡ 多模式执行** - 支持立即执行、排队执行、立即打断执行

**🔄 命令取消机制** - 支持取消正在执行的命令

**🎮 易于扩展** - 基类支持快速创建自定义命令

**🔗 命令组合** - 支持复合命令、条件命令等高级特性

## 安装

### 通过 Git URL 安装

1. 打开 Unity Package Manager
2. 点击 "+" 按钮
3. 选择 "Add package from git URL"
4. 输入：https://github.com/PeterParkers007/Tech-Cosmos.Component.CommandSystem.git


**或通过 package.json 安装**
将包文件夹放置在项目的 Packages 目录中。

## 快速开始

### 1. 创建命令目标
```csharp
public class Unit : CommandTarget<Unit>
{
    private void Start()
    {
        // 绑定基础命令执行逻辑
        OnMove += MoveToPosition;
        OnAttack += AttackTarget;
        OnStop += StopActions;
        
        // 注册自定义动作
        RegisterAction<ResourceNode>("Gather", GatherResource);
        RegisterAction("Dance", Dance);
    }

    private void MoveToPosition(Vector3 position)
    {
        // 实现移动逻辑
        transform.position = position;
        Debug.Log($"Moving to {position}");
    }

    private void AttackTarget(Unit target)
    {
        // 实现攻击逻辑
        Debug.Log($"Attacking {target.name}");
    }

    private void StopActions()
    {
        // 实现停止逻辑
        Debug.Log("Stopping all actions");
    }
    
    private void GatherResource(ResourceNode node)
    {
        Debug.Log($"Gathering from {node.name}");
    }
    
    private void Dance()
    {
        Debug.Log("Dancing!");
    }
}
```
### 2. 使用命令系统
```csharp
public class GameController : MonoBehaviour
{
    public Unit playerUnit;
    public Unit enemyUnit;
    public ResourceNode resourceNode;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 移动命令 - 立即执行
            var moveCommand = new MoveCommand<Unit>(playerUnit, GetMouseWorldPosition());
            CommandManager<Unit>.Instance.ExecuteCommand(playerUnit, moveCommand);
        }

        if (Input.GetMouseButtonDown(1))
        {
            // 攻击命令 - 排队执行
            var attackCommand = new AttackCommand<Unit>(playerUnit, enemyUnit, CommandPriority.High);
            CommandManager<Unit>.Instance.EnqueueCommand(playerUnit, attackCommand);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 停止命令 - 高优先级立即执行
            var stopCommand = new StopCommand<Unit>(playerUnit);
            CommandManager<Unit>.Instance.ExecuteImmediateCommand(playerUnit, stopCommand);
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            // 使用扩展方法创建条件命令
            var gatherCommand = new GatherCommand<Unit>(playerUnit, resourceNode)
                .When(() => playerUnit.HasTools) // HasTools 属性未定义
                .WithPriority(CommandPriority.Normal);
                
            CommandManager<Unit>.Instance.EnqueueCommand(playerUnit, gatherCommand);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}
```
### 3. 配置命令管理器
**确保场景中有一个GameObject挂载了具体的命令管理器：**
```csharp
public class UnitCommandManager : CommandManager<Unit>
{
    private void Update()
    {
        // 每帧处理命令队列
        ObserverUpdate();
    }
}
```
## 核心组件

### ICommand 接口体系
**完整的命令接口生态系统：**
```csharp 
public interface ICommand
{
    bool CanExecute();
    void Execute();
    CommandStatus Status { get; }
    Action<CommandStatus> OnStatusChanged { get; set; }
}

public interface ICancellableCommand : ICommand
{
    void Cancel();
    bool CanCancel { get; }
}

public interface IPrioritizedCommand
{
    CommandPriority Priority { get; set; }
}
```
### 内置命令类型

- **MoveCommand** - 移动命令，支持优先级配置

- **AttackCommand** - 攻击命令，完整的异常处理

- **StopCommand** - 停止命令，默认高优先级

- **CompositeCommand** - 组合命令，顺序执行多个命令

- **ConditionalCommand** - 条件命令，满足条件时执行

### CommandManager
**单例命令管理器，负责：**

- 执行即时命令 (ExecuteCommand)

- 排队执行命令 (EnqueueCommand)

- 立即打断执行 (ExecuteImmediateCommand)

- 取消命令 (CancelCurrentCommand, CancelAllCommands)

- 队列状态监控 (ObserverUpdate)

### CommandQueue
**智能命令队列，支持：**

- 基于优先级的命令排序

- 自动清理已完成/失败命令

- 当前命令取消支持

- 队列状态调试信息

### CommandTarget
**可扩展的命令目标基类，提供：**

- 内置基础动作 (Move、Attack、Stop)

- 动态动作注册系统 (RegisterAction)

- 类型安全的动作执行 (ExecuteAction<T>)

- 泛型类型支持

## 🛠️ 扩展自定义命令

**方式1：继承 BaseCommand（推荐）**
```csharp
public class ResourceNode 
{
    public bool HasResources => true;
    public string name = "ResourceNode";
}

public class GatherCommand<T> : BaseCommand<T>
{
    private CommandTarget<T> _gatherer;
    private ResourceNode _resourceNode;

    public GatherCommand(CommandTarget<T> gatherer, ResourceNode node)
    {
        _gatherer = gatherer;
        _resourceNode = node;
    }

    public override bool CanExecute() => 
        base.CanExecute() && _gatherer != null && _resourceNode != null && _resourceNode.HasResources;

    public override void Execute()
    {
        if (!CanExecute())
        {
            OnExecuteFailed();
            return;
        }

        OnExecuteStart();
        
        try
        {
            if (!_isCancelled)
            {
                _gatherer.ExecuteAction("Gather", _resourceNode);
                OnExecuteComplete();
            }
            else
            {
                OnExecuteCancelled();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Gather command failed: {ex.Message}");
            OnExecuteFailed();
        }
    }
}
```
**方式2：使用扩展方法创建复杂命令**
```csharp
// 链式命令组合
var patrolCommand = new MoveCommand<Unit>(unit, pointA)
    .Then(new MoveCommand<Unit>(unit, pointB))
    .Then(new AttackCommand<Unit>(unit, enemy))
    .WithPriority(CommandPriority.Normal);

// 条件命令
var safeAttack = new AttackCommand<Unit>(unit, enemy)
    .When(() => unit.Health > 0.3f && unit.HasAmmo); // Health、HasAmmo 属性未定义
```

## ⚡ 高级用法
### 命令状态监听
```csharp
var command = new MoveCommand<Unit>(unit, position);
command.OnStatusChanged += (status) =>
{
    switch (status)
    {
        case CommandStatus.Completed:
            Debug.Log("Move command completed successfully");
            break;
        case CommandStatus.Failed:
            Debug.LogError("Move command failed");
            break;
        case CommandStatus.Cancelled:
            Debug.Log("Move command was cancelled");
            break;
    }
};
```
### 自定义 CommandTarget 动作
```csharp
public class AdvancedUnit : CommandTarget<AdvancedUnit>
{
    private void Start()
    {
        RegisterAction<Vector3, float>("MoveSmooth", MoveSmoothly);
        RegisterAction<string>("PlayAnimation", PlayAnimation);
    }
    
    private void MoveSmoothly(Vector3 position, float duration)
    {
        // 平滑移动实现
        StartCoroutine(MoveCoroutine(position, duration));
    }
    
    private void PlayAnimation(string animationName)
    {
        // 动画播放逻辑
        GetComponent<Animator>().Play(animationName);
    }
}
```
## 最佳实践

1. **执行模式选择**
- ExecuteCommand() - 立即执行关键命令（移动、停止）

- EnqueueCommand() - 排队执行非关键命令（采集、建造）

- ExecuteImmediateCommand() - 打断当前命令执行紧急命令（躲避、紧急停止）

2. **优先级配置**
- Low - 采集、建造等后台任务

- Normal - 移动、工作等常规命令

- High - 战斗、技能释放等重要命令

- Immediate - 紧急躲避、强制停止等关键命令

3. **内存管理**
- 及时调用 ClearUnitCommands() 清理不再需要的命令

- 在对象销毁时调用 CancelAllCommands() 取消相关命令

- 使用命令状态事件进行资源清理

4. **错误处理**
- 始终在 Execute() 方法中使用 try-catch 块

- 通过 OnStatusChanged 事件监听命令执行状态

- 在 CanExecute() 中进行前置条件检查

5. **性能优化**
- 避免在每帧创建大量命令对象

- 使用命令队列管理批量命令

- 合理使用命令优先级减少不必要的命令打断

---

## 许可证
MIT License - 详见 [LICENSE](https://github.com/PeterParkers007/Tech-Cosmos.Component.CommandSystem/blob/main/LICENSE) 文件。