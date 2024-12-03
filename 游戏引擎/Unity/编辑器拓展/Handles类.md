## 作用

提供API可以再Scene窗口中绘制自定义内容
和GUI、EditorGUI类似，不过它专门提供给Scene窗口使用

需要再对应的响应函数中处理

## 更新响应函数

1. 单独为某个脚本实现一个自定义脚本，要继承自Editor
   
2. 脚本命名为该脚本名+Editor
   
3. 脚本 加特性 [CustomEditor(脚本类)]
在脚本中实现OnSceneGUI方法
该方法会在我们选中挂载自定义脚本的对象时自动更新
只有选中时才会执行，没有选中不执行

## 自定义窗口中监听Scene窗口更新响应函数
可以在自定义窗口显示时
监听更新事件
SceneView.duringSceneGui += 事件函数
SceneView.duringSceneGui -= 事件函数

## 文本、线段和虚线控件

Handlers.color 设置控件颜色
Handlers.label
Handles.DrawLine
Handles.DrawDottedLine

## 弧、圆、立方体和集合体

Handler.DrawWireArc(圆心,法线,绘制朝向,角度,半径)
Handler.DrawSolidArc(圆心,法线,绘制朝向,角度,半径)
Handler.DrawWireDisc(圆心,法线,半径)
Handler.DrawSolidDisc(圆心,法线,半径)
其他同理

## 移动旋转缩放

Handles.DoPositionHandle(位置,角度)
Handles.PositionHandle(位置,角度)
其他同理

## 自由移动、自由旋转

Handles.FreeMoveHandle(位置,句柄大小,移动步进值,渲染控制手柄的回调函数)

## 显示GUI

Handles.BeginGUI();
GUI代码
Handles.EndGUI();

获取Scene窗口信息
SceneView.currentDrawingSceneView

## HandleUtility公共类
是一个工具类
用于处理场景中的Handles以及其他与编辑器交互相关的功能
主要用于处理编辑器的鼠标转换、坐标转换和其他与Handles相关的功能