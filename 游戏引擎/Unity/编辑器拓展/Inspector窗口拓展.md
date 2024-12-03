## 是什么
可以完全自定义某一个脚本再Inspector窗口中的显示

## SerializedObject和SeializedProperty
主要用于Unity编辑器操作和修改序列化对象的属性
SerializedObject 代表脚本对象
SeializedProperty 代表脚本对象中的属性

## 自定义
单独为某个脚本实现一个自定义脚本，要继承自Editor

脚本命名为该脚本名+Editor

脚本加特性 [CustomEditor(脚本类)]

重写OnInspectorGUI函数，他控制Inspector窗口中显示的内容

## 数组、List属性自定义显示、

EditorGUILayout.PropertyField(SeializedProperty对象，标题)

完全自定义可以用SeializedProperty数组相关API来完成
1. arraySize 获取数组容量
2. InsertArrayElementAtIndex(索引)为数组再指定索引插入默认元素
3. DeleteArrayElementAtIndex(索引)为数组再指定索引删除元素
4. GetArrayElementAtIndex(索引)获取指定索引位置SeializedProperty对象
5. 
## 自定义属性
EditorGUILayout.PropertyField(SeializedProperty对象，标题)
SeializedProperty.FindPropertyRelative(属性)
SeializedProperty.FindProperty(属性.子属性)

## 字典属性 自定义显示