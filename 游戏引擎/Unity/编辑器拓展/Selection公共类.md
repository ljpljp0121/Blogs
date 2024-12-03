## 作用
用于获取当前再Unity编辑器中选择的对象
只能用于编辑器开发

## 常用静态成员

当前选中Object  Selection.activeObject
   其他同理
   Selection.activeGameObject
   Selection.activeTransform
   Selection.activeContext
   Selection.objects
   Selection.gameObjects

## c常用静态方法
Selection.Contains

Selection.GetFiltered

Selection.selectionChanged

## ISerializationCallbackReceiver接口
OnBeforeSerialize序列化之前
OnAfterDeserialize反序列化之后