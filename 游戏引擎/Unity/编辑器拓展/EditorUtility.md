## 是什么
是一个Unity编辑器的使用工具类
提供了一系列用于编辑器脚本和自定义编辑器的使用功能

在编辑器相关处都可以使用EditorUtility公共类中的相关内容

## 编辑器默认窗口相关
EditorUtility.DisplayDialog("标题","显示信息","确定键名"); 显示提示窗口

int EditorUtility.DisplayDialogComplex("标题","显示信息","按钮1","按钮3","按钮2")

## 文件面板相关
string path = EditorUtility.SaveFilePanel("窗口标题","打开的目录","保存的文件名称","文件后缀格式")   用于在编辑器中保存新创建的文件或选择文件的保存路径

string path = EditorUtility.SaveFilePanelInProject("窗口标题","保存的文件名称","文件后缀格式"，"对话框窗口中显示的文本摘要")

string path = EditorUtility.SaveFolderPanel("窗口标题","文件夹","默认名称")

## 其他内容
