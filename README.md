# 摩耶三十二重奏 (Maya's 32 Ensemble)

一款以原神纳西妲为主题的钢琴/节奏游戏。

## 目录

- [项目概述](#项目概述)
- [环境要求](#环境要求)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [谱面系统](#谱面系统)
- [配置文件](#配置文件)
- [游戏功能](#游戏功能)
- [脚本说明](#脚本说明)
- [自定义皮肤](#自定义皮肤)
- [常见问题](#常见问题)
- [演示与下载](#演示与下载)

## 项目概述

摩耶三十二重奏是一款基于Unity开发的音乐节奏游戏，玩家需要在音符下落到判定线时按下对应的键盘按键。游戏支持5个八度的钢琴音符（C1到B5），包含所有半音，共60个不同的音符。

游戏特色包括自动暂停等待机制、谱面简化算法、角色表情变化系统等。

## 环境要求

- Unity 2021.3.34f1 (LTS版本)
- .NET Framework 4.7.1

项目已包含所有必要的依赖包，无需额外安装：
- NAudio 1.10.0 (音频处理)
- Sanford.Multimedia.Midi 6.6.2 (MIDI支持)
- YamlDotNet 15.1.0 (配置文件解析)

## 快速开始

1. 安装 Unity Hub 和 Unity 2021.3.34f1
2. 克隆本仓库：
   ```bash
   git clone https://github.com/congmingwensi/nahida_music_game.git
   ```
3. 在 Unity Hub 中打开项目文件夹
4. 等待 Unity 自动导入资源和解析包
5. 打开 `Assets/Scenes/front_page.unity` 场景
6. 点击播放按钮开始游戏

## 项目结构

```
nahida_music_game/
├── Assets/
│   ├── Scenes/                    # 场景文件
│   │   ├── front_page.unity       # 首页/标题场景
│   │   └── music_scene.unity      # 游戏主场景
│   ├── Sscript/                   # 脚本文件
│   │   ├── GameMenager.cs         # 游戏主控制器
│   │   ├── NoteObject.cs          # 音符对象控制
│   │   ├── ButtonController.cs    # 按钮控制器
│   │   ├── SimplifySheet.cs       # 谱面简化算法
│   │   ├── AudioManager.cs        # 音频管理器
│   │   ├── CharacterControl.cs    # 角色表情控制
│   │   └── front_page/            # 首页相关脚本
│   ├── StreamingAssets/
│   │   └── SheetMusic/            # 谱面文件目录
│   │       ├── audio/             # 钢琴音频文件
│   │       ├── test_sheet_music.txt   # 谱面文件
│   │       └── test_sheet_music.yaml  # 配置文件
│   ├── Imasge/                    # 图像资源
│   │   ├── default_skin/          # 默认皮肤
│   │   ├── front_page/            # 首页图像
│   │   ├── trans_image/           # 转场图像
│   │   └── character/             # 角色图像
│   └── prefab/                    # 预制件
│       └── node_*.prefab          # 35个音符预制件
└── Packages/                      # 依赖包
```

## 谱面系统

### 音符映射

游戏使用键盘按键对应钢琴音符，覆盖5个八度（C1到B5）：

| 八度 | 音符按键 |
|------|----------|
| 第1八度 (C1-B1) | Z z X x C V v B b N n M |
| 第2八度 (C2-B2) | A a S s D F f G g H h J |
| 第3八度 (C3-B3) | Q q W w E R r T t Y y U |
| 第4八度 (C4-B4) | 1 ! 2 @ 3 4 $ 5 % 6 ^ 7 |
| 第5八度 (C5-B5) | 8 * 9 ( 0 I i O o K k L |

其中小写字母和特殊符号（如 z, x, v, b, n, a, s, f, g, h, q, w, r, t, y, !, @, $, %, ^, *, (, i, o, k）代表升半音（黑键）。

### 时间符号

谱面使用以下符号表示时间间隔，实际时长 = 符号值 × base_interval（毫秒）：

| 符号 | 间隔倍数 | 说明 |
|------|----------|------|
| `.` | 0.5 | 八分音符 |
| `=` | 1.0 | 四分音符 |
| `-` | 2.0 | 二分音符 |
| `+` | 4.0 | 全音符 |

### 自动音符

以 `~` 前缀标记的音符为自动音符，游戏会自动播放这些音符，玩家无需按键。例如 `~A` 表示自动播放A音。

### 谱面示例

```
==-5.W=T=1.E=4R=G.T=1.W=5=T.E=1R=4.W=T.Q=hE1=h.5.E=1=1Q.W==Q
```

解读：
- `==` - 等待2个间隔单位
- `-5` - 等待2个间隔单位后按5键
- `.W` - 等待0.5个间隔单位后按W键
- `=T=1` - 按T键，等待1个间隔，按1键，等待1个间隔

## 配置文件

谱面配置使用YAML格式，文件名需与谱面文件同名（如 `test_sheet_music.yaml`）：

```yaml
base_interval: 125      # 基础间隔（毫秒），时间符号的乘数
et: 1000                # 第一个音符开始时间（毫秒）
minimum_distance: 1     # 简化谱面：最小允许间隔
max_overpressure: 1     # 简化谱面：最大同时按键数
overpressure_probability: 0.001  # 多押概率（已弃用）
offset: 0               # 音高偏移（每单位=7个半音/1个八度）
bear_tempo: 10          # 音符下落速度
mid_start: 2            # MIDI起始位置
slice_mid: false        # 是否切片MIDI
```

### 参数详解

**base_interval**: 决定游戏节奏快慢。例如设为125ms时，`=`符号代表125ms间隔，`-`代表250ms间隔。

**et**: 第一个音符出现的时间，需要小于音符从顶部落到判定线的时间，否则第一个音符会来不及按。

**minimum_distance**: 谱面简化算法参数。当两个音符间隔小于此值时，后面的音符会被标记为自动音符。

**max_overpressure**: 最大同时按键数。超过此数量的同时音符会被简化，只保留音高最高的几个。

**offset**: 音高偏移。原神谱面通常以zxc开始以tyu结束，在本游戏中可能偏低，可以通过此参数向右偏移。设为1表示偏移一个八度（12个半音）。

**bear_tempo**: 音符下落速度。建议设为6-10，数值越大下落越快。

## 游戏功能

### 自动暂停与等待

当玩家超过100毫秒未按下任何键时，游戏会全局暂停1秒等待玩家操作。如果1秒后玩家仍未按键，游戏进入自动模式（Auto Mode），自动播放所有音符。玩家按下任意键后退出自动模式，恢复正常演奏。

### 评分系统

游戏根据按键时机给予不同评价：
- Perfect: 精准命中，100分
- Good: 稍有偏差，75分

连击（Combo）会增加分数倍率。

### 角色表情

游戏中的纳西妲角色会根据玩家表现显示不同表情：
- 普通 (Normal)
- 不满 (Annoyed)
- 开心 (Happy)
- 魅惑 (Seductive)
- 高潮 (Culmination)

### 谱面简化算法

`SimplifySheet.cs` 实现了智能谱面简化功能：

1. **密度检查**: 如果两个音符间隔小于 `minimum_distance`，后面的音符自动变为Auto音符
2. **多押检查**: 如果同时按键数超过 `max_overpressure`，只保留音高最高的几个音符，其余变为Auto音符

这使得玩家可以根据自己的水平调整难度。

## 脚本说明

### GameMenager.cs
游戏主控制器，负责：
- 读取和解析谱面文件
- 生成音符对象
- 检测键盘输入
- 计算分数和连击
- 管理游戏状态（暂停、自动模式等）

### NoteObject.cs
音符对象脚本，控制：
- 音符下落运动
- 判定区域检测
- 命中/错过判定
- 自动模式下的自动播放

### ButtonController.cs
按钮控制器，处理：
- 按键视觉反馈
- Perfect/Quick/Slow 不同效果显示

### SimplifySheet.cs
谱面简化算法，包含：
- `SimplifySpectrogram()`: 根据密度和多押规则简化谱面
- `MapGroups()`: 音高偏移映射

### AudioManager.cs
音频管理器，实现：
- 钢琴音频预加载
- 音频池化播放（避免同时播放过多音频）

### CharacterControl.cs
角色控制器，管理角色表情切换。

## 自定义皮肤

皮肤资源位于 `Assets/Imasge/` 目录：
- `default_skin/`: 默认皮肤
- `front_page/`: 首页图像
- `trans_image/`: 转场图像
- `character/`: 角色图像

目前皮肤系统尚未完全实现，后续计划支持玩家自定义皮肤目录。

## 添加新谱面

1. 在 `Assets/StreamingAssets/SheetMusic/` 目录下创建新的谱面文件（.txt）
2. 创建同名的配置文件（.yaml）
3. 如需背景音乐，将音频文件放入同一目录
4. 修改 `GameMenager.cs` 中的文件路径，或等待后续的谱面选择功能

谱面格式兼容B站UP主"呱呱有什么坏心思呢"群里分享的谱面格式。

## 常见问题

**Q: 音乐和按键不同步怎么办？**
A: 调整配置文件中的 `et` 参数，或检查 `base_interval` 是否与音乐BPM匹配。

**Q: 游戏太难/太简单怎么办？**
A: 调整 `minimum_distance` 和 `max_overpressure` 参数来简化谱面，或调整 `bear_tempo` 改变下落速度。

**Q: 为什么有些音符自动播放了？**
A: 这是谱面简化功能，以 `~` 前缀标记的音符会自动播放。可以通过调整配置参数来控制简化程度。

**Q: 音乐播放但按键不暂停怎么办？**
A: 这是已知问题。目前按键会触发全局暂停，但背景音乐不会暂停。对于纯钢琴曲，可以考虑不使用背景音乐。

## 演示与下载

游戏演示视频：[YouTube](https://www.youtube.com/watch?v=gT4zYMQvlWM)

下载包：
- 链接：https://pan.quark.cn/s/749e590e70bf
- 提取码：pGMk

## 后续计划

- [ ] 谱面选择界面：玩家可以选择不同目录的谱面
- [ ] 自定义皮肤系统：支持加载不同皮肤目录
- [ ] 背景音乐同步暂停
- [ ] 更多角色和主题

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

本项目采用 MIT 许可证。
