# TechCosmos Command System

---

一个灵活、可扩展的命令系统，用于在 Unity 中实现命令模式，支持命令队列管理和泛型命令目标。

## 功能特性

**🎯 泛型支持**：支持任意类型的命令目标

**📋 命令队列**：内置命令队列管理，支持顺序执行

**⚡ 即时执行**：支持立即执行或排队执行命令

**🎮 易于扩展**：轻松创建新的命令类型

**🔄 生命周期管理**：自动管理命令队列和清理

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
        // 绑定命令执行逻辑
        OnMove += MoveToPosition;
        OnAttack += AttackTarget;
        OnStop += StopActions;
    }

    private void MoveToPosition(Vector3 position)
    {
        // 实现移动逻辑
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
}
```
### 2. 使用命令系统
```csharp
public class GameController : MonoBehaviour
{
    public Unit playerUnit;
    public Unit enemyUnit;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 移动命令
            var moveCommand = new MoveCommand<Unit>(playerUnit, GetMouseWorldPosition());
            CommandManager<Unit>.Instance.ExecuteCommand(playerUnit, moveCommand);
        }

        if (Input.GetMouseButtonDown(1))
        {
            // 攻击命令
            var attackCommand = new AttackCommand<Unit>(playerUnit, enemyUnit);
            CommandManager<Unit>.Instance.EnqueueCommand(playerUnit, attackCommand);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 停止命令
            var stopCommand = new StopCommand<Unit>(playerUnit);
            CommandManager<Unit>.Instance.ExecuteCommand(playerUnit, stopCommand);
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

### ICommand 接口
**所有命令都需要实现的基础接口：**
```csharp 
public interface ICommand
{
    bool CanExecute();
    void Execute();
}
```
### 内置命令类型

- **MoveCommand**：移动命令
- **AttackCommand**：攻击命令
- **StopCommand**：停止命令

### CommandManager
**单例命令管理器，负责：**

- 执行即时命令

- 管理命令队列

- 清理单位命令

### CommandQueue
**命令队列，支持：**

- 顺序执行命令

- 队列清理

- 状态检查

### CommandTarget
**命令目标基类，提供：**

- 命令事件绑定

- 泛型类型支持

- 统一的命令接口

## 扩展自定义命令

**创建新的命令类型：**

```csharp
public class CustomCommand<T> : ICommand
{
    private CommandTarget<T> _commandTarget;
    private string _message;

    public CustomCommand(CommandTarget<T> commandTarget, string message)
    {
        _commandTarget = commandTarget;
        _message = message;
    }

    public bool CanExecute() => _commandTarget != null;

    public void Execute()
    {
        if (CanExecute())
        {
            Debug.Log(_message);
            // 执行自定义逻辑
        }
    }
}
```
## 最佳实践

1. **即时执行 vs 队列执行：**
- 使用 ExecuteCommand() 立即执行关键命令

- 使用 EnqueueCommand() 对非关键命令进行排队

2. **内存管理：**
- 及时调用 ClearUnitCommands() 清理不再需要的命令

- 在对象销毁时清理相关命令

3. **错误处理：**
- 始终检查 CanExecute() before execution

- 处理命令执行失败的情况

---

## 许可证
MIT License - 详见 [LICENSE](https://github.com/techcosmos/command-system/blob/main/LICENSE) 文件。