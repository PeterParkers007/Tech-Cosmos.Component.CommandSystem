# Tech-Cosmos.Component.CommandSystem

## 概述

**Tech-Cosmos.Component.CommandSystem** 是一个健壮、灵活、可扩展的命令系统，专为 Unity 游戏和应用设计。它提供了一套完整的解决方案，用于创建、管理、调度和执行命令。通过采用设计模式（如命令模式、组合模式、条件装饰器等），该系统能够轻松处理复杂逻辑，包括命令队列化、优先级排序、状态管理、取消操作以及顺序/条件组合。

核心特性包括：
- **命令抽象**：为所有命令提供统一的基类和接口。
- **状态生命周期**：明确且可追踪的命令状态流转（`Pending` -> `Executing` -> `Completed/Failed/Cancelled`）。
- **可扩展性**：通过 `BaseCommand<T>` 泛型基类轻松集成自定义目标类型。
- **命令组合**：使用组合命令（`CompositeCommand`）或流畅 API（`Then`）按顺序执行一系列命令。
- **条件执行**：使用 `ConditionalCommand` 或 `When` 扩展方法，仅在满足特定条件时执行命令。
- **优先级调度**：为命令分配优先级，队列会自动根据优先级进行排序。
- **取消机制**：支持取消单个命令或通过队列取消一组命令。
- **管理器集成**：`CommandManager` 提供了管理多个命令目标及其队列的集中式、持久化单例。

---

## 安装

1.  将整个 `Tech-Cosmos.Component.CommandSystem` 文件夹复制到您的 Unity 项目的 `Assets` 目录下。
2.  所有脚本都位于 `TechCosmos.CommandSystem.Runtime` 命名空间下，并根据功能组织在 `Interfaces`、`Enums`、`Command` 等子文件夹中。
3.  **注意**：`CommandManager` 继承自 `MonoBehaviour`。您需要创建一个游戏对象并挂载一个继承自 `CommandManager` 的自定义管理器脚本，或者直接使用它（如果它不是一个抽象类的话，但通常建议创建自定义派生类）。

---

## 核心架构

系统由以下几个关键部分组成，它们协同工作：

### 1. 接口

接口定义了系统的契约，是实现高度解耦和可扩展性的关键。

- **`ICommand`**：
    - 所有命令的基础契约。
    - 方法：`bool CanExecute()`、`void Execute()`。
    - 属性：`CommandStatus Status`。
    - 事件：`Action<CommandStatus> OnStatusChanged`，用于订阅状态变更。

- **`ICommandTarget`**：
    - 一个标记接口，用于标识能够接收和执行命令的“目标”（例如，一个游戏角色、一个UI面板等）。目前为空，用于在 `CommandManager` 中进行类型安全的字典管理。

- **`IPrioritizedCommand`**：
    - 继承自 `ICommand`。
    - 属性：`CommandPriority Priority { get; set; }`。允许命令在队列中按优先级排序。

- **`ICancellableCommand`**：
    - 继承自 `ICommand`。
    - 方法：`void Cancel()`。
    - 属性：`bool CanCancel { get; }`。用于判断命令是否可以被取消。

- **`IStatusUpdatable`**：
    - 方法：`void UpdateStatus(CommandStatus newStatus)`。
    - 允许外部（如 `CommandQueue`）安全地更新命令的状态。

### 2. 枚举

- **`CommandStatus`** (命令状态生命周期)：
    ```
    Pending -> Executing -> Completed
                        |-> Failed
                        |-> Cancelled
    ```
    - **`Pending`**：命令已创建，等待执行。
    - **`Executing`**：命令正在执行中。
    - **`Completed`**：命令成功执行完毕。
    - **`Failed`**：命令执行失败。
    - **`Cancelled`**：命令在执行前或执行中被取消。

- **`CommandPriority`** (命令优先级，值越大优先级越高)：
    - **`Low (0)`**：低优先级，例如后台任务。
    - **`Normal (1)`**：默认优先级，适用于大部分移动或交互。
    - **`High (2)`**：高优先级，例如战斗指令或系统重要通知。
    - **`Immediate (3)`**：最高优先级，应立即执行。常用于 `ExecuteImmediateCommand` 清除队列后插入。

---

## 核心组件详解

### 3. 抽象基类

- **`BaseCommand`** (继承 `ICommand`, `IStatusUpdatable`)：
    - 提供了 `Status` 属性的基础实现和 `UpdateStatus` 方法。
    - 包含四个便捷的保护方法，供子类在特定执行阶段调用以更新状态：
        - `OnExecuteStart()`
        - `OnExecuteComplete()`
        - `OnExecuteFailed()`
        - `OnExecuteCancelled()`
    - 在状态发生实际变化时，会自动调用 `OnStatusChanged` 回调。

- **`BaseCommand<T>`** (继承 `BaseCommand`, `ICancellableCommand`, `IPrioritizedCommand`)：
    - 为大多数通用命令提供的推荐基类。
    - 内置了取消支持（`Cancel()` 方法和 `_isCancelled` 标志位）和优先级属性（`Priority`，默认值为 `Normal`）。
    - 重写了 `CanExecute()`，包含了 `_isCancelled` 的检查，确保已取消的命令不会被执行。

### 4. 具体命令类

- **`CompositeCommand<T>`** (继承 `BaseCommand<T>`)：
    - **组合模式**实现，用于将多个命令按顺序链接在一起作为一个整体执行。
    - 内部维护一个 `_commands` 列表。当一个子命令完成时，会自动执行下一个。
    - 如果任一子命令失败或被取消，整个组合命令也会相应地标记为失败或取消。
    - 支持通过构造函数初始化或之后通过 `AddCommand` 动态添加子命令。

- **`ConditionalCommand<T>`** (继承 `BaseCommand<T>`)：
    - **装饰器模式**实现，为一个现有命令（`_wrappedCommand`）添加执行条件。
    - 执行时，会首先检查条件函数 `_condition` 是否返回 `true`，再检查内部命令的 `CanExecute()`。
    - 如果条件不满足，`CanExecute()` 返回 `false`，`Execute()` 会调用 `OnExecuteFailed()`。
    - 状态变化会从被包装的命令传递到 `ConditionalCommand`。

### 5. 静态扩展类

`CommandExtensions` 静态类提供了一套流畅的 API，用于以函数式风格构建命令链。

- **`Then<T>`**：将两个命令串联起来。
    - 如果第一个命令已经是 `CompositeCommand<T>`，则将第二个命令添加到它的子命令列表中。
    - 否则，创建一个新的 `CompositeCommand<T>` 来包含这两个命令。
    - **示例**：`moveCommand.Then<Unit>(attackCommand)`

- **`WithPriority`**：为实现了 `IPrioritizedCommand` 的命令设置优先级。
    - **示例**：`myCommand.WithPriority(CommandPriority.High)`

- **`When<T>`**：为一个命令添加一个执行条件，返回一个新的 `ConditionalCommand<T>`。
    - **示例**：`attackCommand.When<Unit>(() => isEnemyInRange)`

### 6. 命令队列 (`CommandQueue`)

- 负责管理单个 `ICommandTarget` 的命令序列。
- **入队 (Enqueue)**：
    - `Enqueue(ICommand command)`：将命令添加到队列。
    - **自动排序**：每次入队时，队列会按优先级降序重新排序（`Immediate` > `High` > `Normal` > `Low`）。
- **出队与执行 (ExecuteNext)**：
    - `ExecuteNext()` 方法被 `CommandManager` 的 `ObserverUpdate` 周期性调用。
    - 在执行前，会先清理队列中所有已完成、失败或取消的命令。
    - 取出队列最前面的命令（优先级最高且最先入队的命令）。
    - 检查其 `CanExecute()`，如果为 `true`，则执行它。
    - **健壮性检查**：执行后会检查命令状态，如果仍为 `Executing`，会发出警告，提示开发者命令应自行更新其最终状态。
- **取消 (CancelCurrent / Clear)**：
    - `CancelCurrent()`：取消当前正在执行的命令。
    - `Clear()`：取消当前命令并清空整个等待队列。

### 7. 命令管理器 (`CommandManager`)

- **抽象单例**：`CommandManager` 继承自 `MonoBehaviour`，并实现为抽象单例模式。
    - 在 `Awake` 中初始化，并调用 `DontDestroyOnLoad` 来保证在场景切换时持久存在。
    - 您需要创建一个继承自它的子类，以添加特定的游戏逻辑。
- **核心职责**：
    - 维护一个 `Dictionary<ICommandTarget, CommandQueue>`，为游戏中的每个“命令目标”管理一个独立的队列。
- **API 方法**：
    - `ExecuteCommand(ICommandTarget unit, ICommand command)`：不通过队列，立即尝试执行命令。如果 `CanExecute()` 返回 true，则立即执行。适用于需要立即生效但优先级不高于当前队列的操作。
    - `EnqueueCommand(ICommandTarget unit, ICommand command)`：将命令添加到指定目标的队列末尾（然后自动排序）。
    - `ExecuteImmediateCommand(ICommandTarget unit, ICommand command)`：**清除指定目标的所有待处理命令**，然后立即执行该命令。这是最高优先级的操作，用于打断一切。
    - `CancelAllCommands(ICommandTarget unit)`：取消指定目标的当前命令并清空其整个队列。
    - `CancelCurrentCommand(ICommandTarget unit)`：仅取消指定目标当前正在执行的命令。
    - `ObserverUpdate()`：需要在某个地方（例如，一个全局的 `Update` 或 `FixedUpdate` 方法中）被定期调用，以驱动所有队列的 `ExecuteNext()` 方法，从而推进命令序列的执行。

---

## 快速上手

### 1. 创建自定义命令

创建一个具体的命令，继承自 `BaseCommand<T>`，并填入您的游戏逻辑。

```csharp
using UnityEngine;
using TechCosmos.CommandSystem.Runtime;
using TechCosmos.CommandSystem.Runtime.Enums;

// 假设 'Player' 是你的 ICommandTarget 具体类
public class MoveCommand : BaseCommand<Player> 
{
    private Player _player;
    private Vector3 _destination;

    public MoveCommand(Player player, Vector3 destination)
    {
        _player = player;
        _destination = destination;
    }

    public override bool CanExecute()
    {
        // 基础检查 + 自定义条件（例如，玩家是否存活）
        return base.CanExecute() && _player != null && _player.IsAlive;
    }

    public override void Execute()
    {
        if (!CanExecute())
        {
            OnExecuteFailed();
            return;
        }

        OnExecuteStart();
        // 开始异步移动操作...
        _player.MoveTo(_destination, OnMoveComplete);
    }

    private void OnMoveComplete(bool success)
    {
        if (_isCancelled)
        {
            // 如果已被取消，就不再更新状态
            return;
        }

        if (success)
            OnExecuteComplete();
        else
            OnExecuteFailed();
    }
}
```

### 2. 设置 CommandManager

创建一个 `MonoBehaviour` 脚本，继承自 `CommandManager`，并将其挂载到场景中的一个游戏对象（如 "GameManager"）上。

```csharp
using TechCosmos.CommandSystem.Runtime;

public class MyGameCommandManager : CommandManager
{
    void Update()
    {
        // 驱动所有命令队列的运行
        ObserverUpdate();
    }
}
```

### 3. 使用命令系统

在您的游戏逻辑中获取管理器实例，并下达命令。

```csharp
using TechCosmos.CommandSystem.Runtime;
using TechCosmos.CommandSystem.Runtime.Command;
using static TechCosmos.CommandSystem.Runtime.CommandExtensions; // 用于流畅API

public class PlayerController : MonoBehaviour, ICommandTarget 
{
    public void IssueCommands()
    {
        var manager = MyGameCommandManager.Instance;
        var player = this;

        // 1. 使用流畅API构建一个带条件的命令链
        var complexCommand = new MoveCommand(player, transform.position + Vector3.forward * 5)
            .Then<Player>(new AttackCommand(player, currentTarget))
            .When<Player>(() => currentTarget != null) // 只有当前有目标时，攻击命令才会执行
            .WithPriority(CommandPriority.High); // 整个链的优先级为高

        // 2. 入队并执行
        manager.EnqueueCommand(player, complexCommand);

        // 3. 立即打断并执行一个紧急命令
        // manager.ExecuteImmediateCommand(player, new FleeCommand(player));
    }
}
```

---

## 高级用法与模式

### 命令取消

- **自动检查**：在 `BaseCommand<T>.CanExecute()` 中，会自动检查 `_isCancelled` 标志。如果为 true，命令不会执行。
- **协作式取消**：对于长时间运行的异步命令（如移动、动画），您必须在命令逻辑中定期检查 `_isCancelled` 标志，并在被取消时尽早退出。
- **级联取消**：`CompositeCommand<T>` 的 `Cancel()` 方法会取消自身，并调用当前正在执行的子命令的 `Cancel()` 方法。

### 状态监控

您可以订阅任何命令的 `OnStatusChanged` 事件来监控其生命周期。

```csharp
var moveCmd = new MoveCommand(player, destination);
moveCmd.OnStatusChanged += (status) => {
    Debug.Log($"Move Command Status Changed to: {status}");
    if (status == CommandStatus.Completed) {
        // 做一些后续处理...
    }
};
```

### 自定义 CommandTarget

`ICommandTarget` 是一个标记接口。最好的实践是让您的游戏实体（如 `Player`、`Unit`、`UIForm`）实现此接口。这使得 `CommandManager` 可以为每个实体独立管理命令队列，互不干扰。

---

## API 参考

### `ICommand`

| 成员 | 类型 | 描述 |
| :--- | :--- | :--- |
| `CanExecute()` | 方法 | 返回 `bool`。在执行前检查命令是否满足所有前提条件。 |
| `Execute()` | 方法 | 执行命令的核心逻辑。调用者应确保内部会更新状态。 |
| `Status` | 属性 | 获取命令的当前 `CommandStatus`。 |
| `OnStatusChanged` | 事件 | 当 `Status` 发生变化时触发。 |

### `BaseCommand<T>`

| 成员 | 类型 | 描述 |
| :--- | :--- | :--- |
| `Priority` | 属性 | `CommandPriority`。获取或设置命令的优先级。 |
| `CanCancel` | 虚属性 | 返回 `true`。可重写以定义能否取消。 |
| `Cancel()` | 虚方法 | 取消命令。设置 `_isCancelled` 标志并更新状态。 |

### `CommandManager`

| 方法 | 描述 |
| :--- | :--- |
| `ExecuteCommand(target, cmd)` | 立即尝试执行命令（不排队）。 |
| `EnqueueCommand(target, cmd)` | 将命令加入目标的队列。 |
| `ExecuteImmediateCommand(target, cmd)` | 清空目标队列，并立即执行此命令。最高优先级操作。 |
| `CancelAllCommands(target)` | 清空目标的所有队列，并取消当前命令。 |
| `CancelCurrentCommand(target)` | 仅取消目标当前正在执行的命令。 |
| `ObserverUpdate()` | **必须被周期性调用**。驱动所有队列检查并执行下一个命令。 |

### 扩展方法

| 方法 | 描述 |
| :--- | :--- |
| `cmd.Then<T>(nextCmd)` | 串联命令，返回 `CompositeCommand<T>`。 |
| `cmd.WithPriority(priority)` | 设置命令优先级，返回原命令。 |
| `cmd.When<T>(condition)` | 添加执行条件，返回 `ConditionalCommand<T>`。 |

---

## 依赖关系

- **Unity Engine**：`UnityEngine` 命名空间下的 `MonoBehaviour`、`Debug`、`Vector3` 等。
- **System**：`System`、`System.Collections.Generic`、`System.Linq`。
- 没有其他第三方依赖。

---

## 许可

本项目遵循 MIT 许可证。