# nahida_music_game
纳西妲主题音乐游戏-摩耶三十二重奏
unity版本：2021.3.34f1
脚本在Assets/Sscript
ButtonController.cs 控制按钮
NoteObject.cs控制音符
GameMenager.cs控制整体
front_page是首页的转场代码
SimplifySheet是简化谱面，谱面偏移相关代码
游戏中 会读取StreamingAssets/SheetMusic/ 为谱面文件目录
test_sheet_music.txt是谱面文件，跟呱呱的谱一样（B站up主 呱呱有什么坏心思呢 他群里的谱）
test_sheet_music.yaml是配置项 目前只有基础间隔 和et（第一个音符开始的毫秒数，肯定得小于从头落下的时间）

谱面解释：
=-+分别代表1,2,4间隔。这个间隔会乘yaml文件的毫秒数base_interval。
其余字母和数字为按键。文件夹中可以有任意格式的音乐，有音乐 情况下，演奏时会播放音乐。不过这时候要注意按键跟音乐节奏一致
yaml文件解释：
base_interval：一个间隔的毫秒数
et：第一个音符最开始的毫秒数
minimum_distance：简化谱面使用，最小允许间隔。程序会把间隔内音符转换为auto音符
max_overpressure：简化谱面使用，最大多压数，遇到更大多压时，会转换为该数字的多压数
offset：谱面偏移，自动向右偏移1度（7个音阶）。因为原神谱以zxc...开始以tyu结束，在本游戏中琴音会偏低
bear_tempo：下落速度，个人认为调成6读谱最舒服

加入了全局暂停和全局auto功能。超过100毫秒未按下 会全局暂停1s等待玩家按下。1s后玩家没有按下时，进入auto模式
按下任意键后，停止auto模式，进入正常演奏模式。
——目前有一个问题，按键会全局暂停，但音乐不会。介于这个游戏弹得几乎只有钢琴曲，不知道音乐是不是必要。

Assets\Imasge中default_skin是默认皮肤，随便做的很丑
front_page是首页图像
trans_image是转场图

后续预计 可以根据目录写一个前端，玩家可以自行选择不同目录的谱面来读取
可以自行设计不同皮肤目录来读取（还没做）

游戏游完视频见：https://www.youtube.com/watch?v=wIeMJJBGZXs
