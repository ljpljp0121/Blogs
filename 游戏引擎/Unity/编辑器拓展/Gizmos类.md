## 是什么
和Handles一样用来拓展Scene窗口
但是主要专注于绘制辅助线、图标、形状等
二Handles主要用于绘制编辑器控制图标

在继承MonoBehaviour的脚本中实现
OnDrawGizoms()每帧调用，绘制的内容随时可以在Scene窗口中看见
OnDrawGizomsSelected()只有脚本衣服的GameObject被选中时才会每帧调用绘制相关内容

方法也是Draw...
和Handles类其实类似

DrawFrustum(绘制中心,FOV(Field of View,视野),角度，远裁切平面,近裁切平面,屏幕长宽比)

改变绘制内容角度可以修改Gizoms的矩阵
Gizoms.matrix = Matrix4x4.TRS(位置，角度，缩放)

## 贴图、图标
Gizoms.DrawGUITexture(new Rect(x,y,w,h),图片信息)
只能在xy平面上绘制
Gizoms.DrawIcon(this.transform.position,"MyIcon");

## 线段、网格、射线
Gizoms.DrawLine
Gizom.DrawMesh