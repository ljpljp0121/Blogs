## GUILayout
//GUILayout 是一个GUI自动布局的公共类
        //它其中的方法和GUI基本一模一样，都是用来绘制、响应各种UI控件的
        //只不过它在GUI的基础上加入了自动布局功能
        //我们无需过多的去关心UI控件的位置和大小
        //GUILayoutOption 布局选项
        //控件的固定宽高
        //GUILayout.Width(300);
        //GUILayout.Height(200);
        //允许控件的最小宽高
        //GUILayout.MinWidth(50);
        //GUILayout.MinHeight(50);
        //允许控件的最大宽高
        //GUILayout.MaxWidth(100);
        //GUILayout.MaxHeight(100);
        //允许或禁止水平拓展
        //GUILayout.ExpandWidth(true);//允许
        //GUILayout.ExpandHeight(false);//禁止
        //GUILayout.ExpandHeight(true);//允许
        //GUILayout.ExpandHeight(false);//禁止
        //2.准备工作
        //创建一个编辑器窗口 
        #endregion
        #region 知识点 EditorGUI是什么？
        //EditorGUI 类似 GUI
        //是一个主要用于绘制编辑器拓展 UI 的工具类
        //它提供了一些 GUI 中没有的API
        //主要是 编辑器功能中会用到的一些 特殊控件
        //而EditorGUILayout 类似于 GUILayout
        //是一个带有自动布局功能的 EditorGUI 绘制工具类
        //我们经常会将 EditorGUI 和 GUI 混合使用 来制作一些编辑器拓展功能
        //但是由于更多时候我们会用到自动布局功能
        //因此我们接下来着重讲解 EditorGUILayout 中的功能
        //EditorGUI和它的区别仅仅是需要自己设置位置而已
        //详细内容：https://docs.unity.cn/cn/2022.3/ScriptReference/EditorGUILayout.html
        #endregion

## 文本、和标签
#region 知识点一 EditorGUILayout中的文本控件
        //EditorGUILayout.LabelField("文本标题", "文本内容");
        #endregion
        #region 知识点二 EditorGUILayout中的层级、标签选择
        //Layer
        //  int变量 = EditorGUILayout.LayerField("层级选择", int变量);
        //Tag
        //  string变量 = EditorGUILayout.TagField("标签选择", string变量);
        #endregion
         #region 知识点三 EditorGUILayout中的颜色获取
        //color变量 = EditorGUILayout.ColorField(new GUIContent("标题"),
        //                                      color变量, 是否显示拾色器, 是否显示透明度通道, 是否支持HDR);
        #endregion
## 枚举选择按下按钮

 #region 知识点一 枚举选择控件
        //枚举选择
        //  枚举变量 = (枚举类型)EditorGUILayout.EnumPopup("枚举选择", 枚举变量);
        //多选枚举
        //(注意：多选枚举进行的是或运算，声明枚举时一定注意其中的赋值，并且一定要有多种情况的搭配值)
        //  枚举变量 = (枚举类型)EditorGUILayout.EnumFlagsField("枚举多选", 枚举变量);
        #endregion
        #region 知识点二 整数选择控件
        //int变量 = EditorGUILayout.IntPopup("整数单选框", int变量, 字符串数组, int数组);
        #endregion
        #region 知识点三 按下就触发的按钮控件
        //EditorGUILayout.DropdownButton(new GUIContent("按钮上文字"), FocusType.Passive)
        //FocusType枚举时告诉UI系统能够获得键盘焦点 当用户按Tab键时在控件之间进行切换
        //Keyboard  该控件可接收键盘焦点。
        //Passive 该控件不能接收键盘焦点。
        #endregion

## 对象关联和各类类型输入控件

#region 知识点一 对象关联控件
        //对象变量 = EditorGUILayout.ObjectField(对象变量, typeof(对象类型), 是否允许关联场景上对象资源) as 对象类型;
        #endregion
        #region 知识点二 各类型输入控件
     
   //int变量 = EditorGUILayout.IntField("Int输入框", int变量);
        //long变量 = EditorGUILayout.LongField("long输入框", long变量);
        //float变量 = EditorGUILayout.FloatField("Float 输入：", float变量);
        //double变量 = EditorGUILayout.DoubleField("double 输入：", double变量);
        //string变量 = EditorGUILayout.TextField("Text输入：", string变量);
        //vector2变量 = EditorGUILayout.Vector2Field("Vec2输入： ", vector2变量);
        //vector3变量 = EditorGUILayout.Vector3Field("Vec3输入： ", vector3变量);
        //vector4变量 = EditorGUILayout.Vector4Field("Vec4输入： ", vector4变量);
        //rect变量 = EditorGUILayout.RectField("rect输入： ", rect变量);
        //bounds变量 = EditorGUILayout.BoundsField("Bounds输入： ", bounds变量);
        //boundsInt变量 = EditorGUILayout.BoundsIntField("Bounds输入： ", boundsInt变量);
        //注意：EditorGUILayout中还有一些Delayed开头的输入控件
        //     他们和普通输入控件最主要的区别是：在用户按 Enter 键或将焦点从字段移开之前，返回值不会更改
        #endregion
## 折叠控件
isHide = EditorGUILayout.Foldout(isHide, "折叠控件", true);
if(isHide)
{
    type = (E_TestType)EditorGUILayout.EnumPopup("枚举选择", type);
    type2 = (E_TestType)EditorGUILayout.EnumFlagsField("枚举多选", type2);
    index = EditorGUILayout.IntPopup("整数单选框", index, strs, num);
    EditorGUILayout.LabelField(index.ToString());
    if (EditorGUILayout.DropdownButton(new GUIContent("按钮上文字"), FocusType.Passive))
    {
        Debug.Log("按下按钮响应");
    }
}     
